using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Linq;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Model;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Base
{
    public abstract class PluginBase : IPlugin
    {
        public string PluginName { get; private set; }
        public ContextMessageName MessageName { get; private set; }
        public ContextStageName StageName { get; private set; }
        public IPluginExecutionContext Context { get; private set; }
        public IOrganizationServiceFactory ServiceFactory { get; private set; }
        public IOrganizationService Service { get; private set; }
        public ITracingService TracingService { get; private set; }

        public PluginBase(string PluginName)
        {
            this.PluginName = PluginName;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                Service = ServiceFactory.CreateOrganizationService(Context.InitiatingUserId != null ? Context.InitiatingUserId : Context.UserId);
                StageName = (ContextStageName)Context.Stage;
                switch (Context.MessageName.ToLower())
                {
                    case "create":
                        MessageName = ContextMessageName.Create;
                        break;
                    case "update":
                        MessageName = ContextMessageName.Update;
                        break;
                    case "delete":
                        MessageName = ContextMessageName.Delete;
                        break;
                    case "associate":
                        MessageName = ContextMessageName.Associate;
                        break;
                    case "disassociate":
                        MessageName = ContextMessageName.Disassociate;
                        break;
                    case "tb_copyproductdetailing":
                        MessageName = ContextMessageName.CopyProductDetailing;
                        break;
                    default:
                        MessageName = ContextMessageName.NotFound;
                        break;
                }

                TraceLog($"Execution started ({MessageName} {StageName} {(ContextMode)Context.Mode})");

                ExecutePlugin();

                TraceLog("Execution finished");
            }
            catch (Exception ex)
            {
                TraceLog("Execution finished with error");
                TraceLog($"{ex.Message}");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, ex.Message);
            }
        }

        public abstract void ExecutePlugin();

        public void TraceLog(string log)
        {
            if (TracingService != null)
                TracingService.Trace($"{PluginName} -> {log}");
        }

        [Obsolete("Deprecated, to be removed")]
        private int RetrieveUserUILanguageCode()
        {
            QueryExpression userSettingsQuery = new QueryExpression("usersettings");
            userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
            userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, Context.InitiatingUserId);
            EntityCollection userSettings = Service.RetrieveMultiple(userSettingsQuery);
            if (userSettings.Entities.Count > 0)
            {
                return (int)userSettings.Entities[0]["uilanguageid"];
            }
            return 0;
        }

        [Obsolete("Deprecated, to be removed")]
        private Entity RetriveWebResource(string name)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "webresource",
                ColumnSet = new ColumnSet("content"),
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                        AttributeName = "name",
                        Operator = ConditionOperator.Equal,
                        Values = { name }
                        }
                    }
                }
            };

            Entity webResource = Service.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (webResource == null)
                throw new System.Exception($"Web Resources {name} not found");
            return webResource;
        }

        [Obsolete("Deprecated, to be removed")]
        private XmlDocument RetrieveWebResourceXmlContent(string webresourceName)
        {
            Entity webresource = RetriveWebResource(webresourceName);
            if (webresource != null)
            {
                byte[] bytes = Convert.FromBase64String((string)webresource["content"]);
                XmlDocument document = new XmlDocument();
                document.XmlResolver = null;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        document.Load(sr);
                    }
                }
                return document;
            }
            else
            {
                throw new InvalidPluginExecutionException(String.Format("Unable to locate the web resource {0}.", webresourceName));

            }
        }

        [Obsolete("Deprecated, to be removed")]
        private string RetrieveLocalizedStringFromWebResource(XmlDocument resource, string resourceId)
        {
            XmlNode valueNode = resource.SelectSingleNode(string.Format(CultureInfo.InvariantCulture, "./root/data[@name='{0}']/value", resourceId));
            if (valueNode != null)
            {
                return valueNode.InnerText;
            }

            return resourceId;
        }


        [Obsolete("Deprecated, use RetrieveLocalizedString in BusinessLogicBase.cs")]
        public string RetrieveLocalizedString(string resourceId)
        {
            int langCode = RetrieveUserUILanguageCode();
            string webresourceName = "";
            switch (langCode)
            {
                case 1033:
                    webresourceName = "tb_localizedStrings.en_US.xml";
                    break;
                case 1041:
                    webresourceName = "tb_localizedStrings.it_IT.xml";
                    break;
                default:
                    webresourceName = "tb_localizedStrings.en_US.xml";
                    break;
            }
            XmlDocument resource = RetrieveWebResourceXmlContent(webresourceName);
            return RetrieveLocalizedStringFromWebResource(resource, resourceId);
        }
    }

    [Obsolete("Deprecated, use MessageName and StageName in BusinessLogicBase.cs")]
    public static class PluginMessage
    {
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
        public const string Retrieve = "RETRIEVE";
    }

    [Obsolete("Deprecated, to be removed")]
    public static class PluginInputParameter
    {
        public const string Target = "Target";
        public const string PreImage = "PreImage";
        public const string PostImage = "PostImage";
    }
}
