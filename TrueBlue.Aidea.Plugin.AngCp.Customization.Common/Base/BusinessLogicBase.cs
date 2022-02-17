using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Model;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Utilities;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Base
{
   public abstract class BusinessLogicBase
    {
        #region Properties
        protected string PluginName { get; private set; }
        protected ContextMessageName MessageName { get; private set; }
        protected ContextStageName StageName { get; private set; }
        protected IPluginExecutionContext Context { get; private set; }
        protected IOrganizationServiceFactory ServiceFactory { get; private set; }
        protected IOrganizationService Service { get; private set; }

        protected Utils Utils { get; private set; }

        protected PluginBase PluginBase;

        private int? TimeZoneCode = null;

        #endregion

        #region Ctor

        public BusinessLogicBase(PluginBase plugin)
        {
            PluginBase = plugin;
            PluginName = plugin.PluginName;
            MessageName = plugin.MessageName;
            StageName = plugin.StageName;
            Context = plugin.Context;
            ServiceFactory = plugin.ServiceFactory;
            Service = plugin.Service;
            Utils = new Utils(Service, Context.InitiatingUserId != null ? Context.InitiatingUserId : Context.UserId);
        }

        public BusinessLogicBase(IOrganizationService service, string pluginName, int messageName)
        {
            PluginName = pluginName;
            MessageName = (ContextMessageName)messageName;
            StageName = ContextStageName.MainOperation;
            Context = null;
            ServiceFactory = null;
            Service = service;
            Utils = new Utils(Service, Context.InitiatingUserId != null ? Context.InitiatingUserId : Context.UserId);
        }

        #endregion

        #region Dynamics Props

        private Entity _target;
        protected Entity Target
        {
            get
            {
                if (_target == null)
                {
                    if (!Context.InputParameters.Contains("Target"))
                        throw new Exception($"Context not contains parameter named Target");
                    if (Context.InputParameters["Target"] == null)
                        throw new Exception($"Parameter Target is null");
                    if (MessageName == ContextMessageName.Delete)
                        _target = new Entity(((EntityReference)Context.InputParameters["Target"]).LogicalName, ((EntityReference)Context.InputParameters["Target"]).Id);
                    else
                        _target = (Entity)Context.InputParameters["Target"];
                }
                return _target;
            }
            private set { _target = value; }
        }

        private Entity _preimage;
        protected Entity PreImage
        {
            get
            {
                if (_preimage == null)
                {
                    string name = "PreImage";
                    if (!Context.PreEntityImages.Contains(name))
                        throw new Exception($"Context not contains PreImage named {name}");
                    if (Context.PreEntityImages[name] == null)
                        throw new Exception($"PreImage {name} is null");
                    _preimage = Context.PreEntityImages[name];
                }
                return _preimage;
            }
            private set { _preimage = value; }
        }

        private Entity _postimage;
        protected Entity PostImage
        {
            get
            {
                if (_postimage == null)
                {
                    string name = "PostImage";
                    if (!Context.PostEntityImages.Contains(name))
                        throw new Exception($"Context not contains PostImage named {name}");
                    if (Context.PostEntityImages[name] == null)
                        throw new Exception($"PostImage {name} is null");
                    _postimage = Context.PostEntityImages[name];
                }
                return _postimage;
            }
            private set { _postimage = value; }
        }

        #endregion

        #region Public Methods

        public abstract void ApplyLogic();

        #endregion

        #region Entity Utilities

        [Obsolete("Use Target prop")]
        protected Entity GetTargetEntity()
        {
            if (Target == null)
            {
                if (!Context.InputParameters.Contains("Target"))
                    throw new Exception($"Context not contains parameter named Target");
                if (Context.InputParameters["Target"] == null)
                    throw new Exception($"Parameter Target is null");
                Target = (Entity)Context.InputParameters["Target"];
            }
            return Target;
        }

        [Obsolete("Use Target prop")]
        protected EntityReference GetDeleteTargetEntity()
        {
            if (!Context.InputParameters.Contains("Target"))
                throw new Exception($"Context not contains parameter named Target");
            if (Context.InputParameters["Target"] == null)
                throw new Exception($"Parameter Target is null");
            return (EntityReference)Context.InputParameters["Target"];
        }

        [Obsolete("Use PreImage props")]
        protected Entity GetPreEntity(string ImageName = "PreImage")
        {
            if (!Context.PreEntityImages.Contains(ImageName))
                throw new Exception($"Context not contains PreImage named {ImageName}");
            if (Context.PreEntityImages[ImageName] == null)
                throw new Exception($"PreImage {ImageName} is null");
            return Context.PreEntityImages[ImageName];
        }

        [Obsolete("Use PostImage props")]
        protected Entity GetPostEntity(string ImageName = "PostImage")
        {
            if (!Context.PostEntityImages.Contains(ImageName))
                throw new Exception($"Context not contains PostImage named {ImageName}");
            if (Context.PostEntityImages[ImageName] == null)
                throw new Exception($"PostImage {ImageName} is null");
            return Context.PostEntityImages[ImageName];
        }

        protected void CheckMandatoryFields(params string[] fields)
        {
            string fieldInError = null;

            if (MessageName == ContextMessageName.Create)
                fieldInError = fields.FirstOrDefault(f => !Target.Contains(f) || Target[f] == null);
            else if (MessageName == ContextMessageName.Update)
                fieldInError = fields.FirstOrDefault(f => (!Target.Contains(f) && !PreImage.Contains(f)) || (Target.Contains(f) && Target[f] == null));

            if (fieldInError != null)
                throw new Exception(RetrieveLocalizedString("error_MissingField", new Dictionary<string, string>() { { "field", GetLocalizatedFied(fieldInError) } }));
        }

        protected void CheckNotUpdatableFields(params string[] fields)
        {
            string fieldInError = null;

            if (MessageName == ContextMessageName.Update)
                fieldInError = fields.FirstOrDefault(f => Target.Contains(f) && PreImage.Contains(f) && !Target[f].Equals(PreImage[f]));

            if (fieldInError != null)
                throw new Exception(RetrieveLocalizedString("error_UpdateField", new Dictionary<string, string>() { { "field", GetLocalizatedFied(fieldInError) } }));
        }

        protected bool EntityHasField(string field)
        {
            if (MessageName == ContextMessageName.Create)
                return Target.Contains(field);
            else if (MessageName == ContextMessageName.Update)
                return Target.Contains(field) || PreImage.Contains(field);
            return false;
        }

        protected T GetFieldInEntity<T>(string field)
        {
            if (Target.Contains(field) && Target[field] != null)
                return Target.GetAttributeValue<T>(field);
            if (MessageName != ContextMessageName.Create && PreImage.Contains(field) && PreImage[field] != null)
                return PreImage.GetAttributeValue<T>(field);

            if (MessageName != ContextMessageName.Create)
                throw new Exception(RetrieveLocalizedString("error_MissingField", new Dictionary<string, string>() { { "field", GetLocalizatedFied(field) } }));
            else
                return default(T);
        }

        #endregion

        #region Utilities

        protected Entity mergeImages(Entity target, Entity image)
        {
            Entity newEnt = target;
            foreach (KeyValuePair<String, Object> attribute in image.Attributes)
            {
                if (!newEnt.Contains(attribute.Key))
                {
                    newEnt[attribute.Key] = attribute.Value;
                }
            }
            return newEnt;
        }

        protected void TraceLog(string log)
        {
            PluginBase.TraceLog(log);
        }

        protected T RetriveConfiguration<T>(string key, string FallbackValue = "keyNotSet")
        {
            return Utils.RetriveConfiguration<T>(key, FallbackValue);
        }

        [Obsolete]
        protected T RetriveConfigurationBasedOnBU<T>(string key, EntityReference userToImpersonate)
        {
            var fetchXml = $@"
            <fetch>
              <entity name='tb_configuration'>
                <attribute name='tb_value' />
                <filter>
                  <condition attribute='tb_name' operator='in'>
                    <value>{key.ToUpper()}</value>
                    <value>{key}</value>
                    <value>{key.ToLower()}</value>
                  </condition>
                </filter>
              </entity>
            </fetch>";
            Entity config = Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            if (config == null)
                throw new Exception(RetrieveLocalizedString("error_NoKey", new Dictionary<string, string>() { { "key", key } }));
            string value = config.GetAttributeValue<string>("tb_value");
            if (string.IsNullOrEmpty(value))
                throw new Exception(RetrieveLocalizedString("error_NullKey", new Dictionary<string, string>() { { "key", key } }));
            return (T)Convert.ChangeType(value, typeof(T));
        }

        protected Entity RetriveWebResource(string name)
        {
            var fetchXml = $@"
            <fetch>
              <entity name='webresource'>
                <attribute name='content' />
                <filter>
                  <condition attribute='name' operator='eq' value='{name}'/>
                </filter>
              </entity>
            </fetch>";
            Entity webResource = Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            if (webResource == null)
                throw new Exception(RetrieveLocalizedString("error_NoWebresource", new Dictionary<string, string>() { { "webresource", name } }));
            return webResource;
        }

        protected void SendEmail(string emailTemplate, EntityReference recipient, bool useThisUserAsSender = false, string sender = null, Dictionary<string, byte[]> attachments = null)
        {
            SendEmail(emailTemplate, new List<EntityReference> { recipient }, useThisUserAsSender, sender, attachments);
        }

        protected void SendEmail(string emailTemplate, IEnumerable<EntityReference> recipients, bool useThisUserAsSender = false, string sender = null, Dictionary<string, byte[]> attachments = null)
        {
            OrganizationRequest req = new OrganizationRequest("tb_SendEmail");
            req["TemplateName"] = emailTemplate;
            req["Recipients"] = JsonConvert.SerializeObject(recipients);
            req["UseUserAsSender"] = useThisUserAsSender;
            req["Sender"] = sender;

            if (attachments != null)
                req["Attachment"] = JsonConvert.SerializeObject(attachments.ToDictionary(a => a.Key, a => Convert.ToBase64String(a.Value)));

            OrganizationResponse response = Service.Execute(req);
        }

        protected string GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            token = Uri.EscapeDataString(token).Replace("%", string.Empty);
            return token;
        }

        protected int GetUserLangCode()
        {
            Entity user = Service.Retrieve("usersettings", Context.InitiatingUserId, new ColumnSet("uilanguageid"));
            return user.GetAttributeValue<int>("uilanguageid");
        }

        protected string RetrieveLocalizedString(string resourceId, Dictionary<string, string> param = null, string ResourceFileName = "common")
        {
            return Utils.RetrieveLocalizedString(resourceId, param, ResourceFileName);
        }

        public DateTime RetrieveLocalTimeFromUTCTime(DateTime UtcTime)
        {
            if (!TimeZoneCode.HasValue)
                TimeZoneCode = RetrieveUsersTimeZoneCode();

            if (!TimeZoneCode.HasValue)
                return UtcTime;

            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = TimeZoneCode.Value,
                UtcTime = UtcTime.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)Service.Execute(request);

            return response.LocalTime;
        }

        public DateTime RetrieveUTCTimeFromLocalTime(DateTime LocalTime)
        {
            if (!TimeZoneCode.HasValue)
                TimeZoneCode = RetrieveUsersTimeZoneCode();

            if (!TimeZoneCode.HasValue)
                return DateTime.Now;

            var request = new UtcTimeFromLocalTimeRequest
            {
                TimeZoneCode = TimeZoneCode.Value,
                LocalTime = LocalTime
            };

            var response = (UtcTimeFromLocalTimeResponse)Service.Execute(request);
            return response.UtcTime;
        }

        protected IEnumerable<Entity> GetConflictingEntities(IEnumerable<Entity> entities, Entity item)
        {
            DateTime validFrom = item.GetAttributeValue<DateTime>("tb_validfrom").Date;
            DateTime validTo = item.Contains("tb_validto") ? item.GetAttributeValue<DateTime>("tb_validto").Date : DateTime.MaxValue.Date;
            return entities.Where(x =>
            {
                DateTime x_validFrom = x.GetAttributeValue<DateTime>("tb_validfrom").Date;
                DateTime x_validTo = x.Contains("tb_validto") ? x.GetAttributeValue<DateTime>("tb_validto").Date : DateTime.MaxValue.Date;
                return validFrom <= x_validTo && x_validFrom <= validTo;
            });
        }

        protected bool IsUnique(bool checkOwner, params string[] fields)
        {
            Entity target = GetTargetEntity();
            string fetchXml = $@"
            <fetch>
              <entity name='{target.LogicalName}'>
                <attribute name='{target.LogicalName}id' />
                <filter>
                  <condition attribute='{target.LogicalName}id' operator='neq' value='{target.Id}'/>";
            foreach (string field in fields)
            {
                object content = target.Contains(field) ? target[field] : GetPreEntity()[field];
                object value = null;
                switch (content)
                {
                    case EntityReference v:
                        value = v.Id;
                        break;
                    case OptionSetValue v:
                        value = v.Value;
                        break;
                    case DateTime v:
                        value = v.ToString("yyyy-MM-dd");
                        break;
                    default:
                        value = content;
                        break;
                }
                fetchXml += $"<condition attribute='{field}' operator='eq' value='{HttpUtility.HtmlEncode(value)}'/>";
            }
            if (checkOwner)
                fetchXml += $"<condition attribute='ownerid' operator='eq' value='{GetFieldInEntity<EntityReference>("ownerid").Id}'/>";
            fetchXml += @"
                </filter>
              </entity>
            </fetch>";
            IEnumerable<Entity> existings = Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities;
            return existings.Count() == 0;
        }

        protected string GetLocalizatedFied(string field)
        {
            TraceLog("GetLocalizatedFied");

            RetrieveEntityRequest entityRequest = new RetrieveEntityRequest();
            entityRequest.LogicalName = Target.LogicalName;
            entityRequest.EntityFilters = EntityFilters.Attributes;
            RetrieveEntityResponse entityResponse = (RetrieveEntityResponse)Service.Execute(entityRequest);

            string label = field;
            try
            {
                AttributeMetadata attribute = entityResponse.EntityMetadata.Attributes.FirstOrDefault(x => x.LogicalName == field);
                if (attribute != null && attribute.DisplayName.UserLocalizedLabel != null)
                    label = attribute.DisplayName.UserLocalizedLabel.Label;
            }
            catch (Exception e)
            {
                TraceLog($"GetLocalizatedFied Exception: {e.Message}");
            }

            return label;
        }

        protected bool CheckAideaRolePermission(string field)
        {
            string fetchXml = $@"
            <fetch>
              <entity name='tb_aidearole'>
                <link-entity name='systemuser' from='tb_aidearole' to='tb_aidearoleid'>
                  <filter>
                    <condition attribute='systemuserid' operator='eq-userid' />
                  </filter>
                </link-entity>
              </entity>
            </fetch>";
            Entity role = Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            return role != null && role.Contains(field) && role[field] != null && (bool)role[field];
        }

        private string GetAuthorizationToken()
        {
            TraceLog("Generate Token request");

            OrganizationRequest req = new OrganizationRequest("tb_GenerateApiToken");
            req["UserId"] = Context.UserId.ToString();
            OrganizationResponse response = Service.Execute(req);
            string token = (string)response.Results.FirstOrDefault().Value;

            TraceLog($"Retrieved Token {token}");

            return token;
        }

        protected void CallWebApi(string controller, string method, HttpMethod httpMethod, Dictionary<string, object> parameters = null, bool hasApi = true)
        {
            string apiPartUrl = hasApi ? "/api" : String.Empty;
            string url = $"{RetriveConfiguration<string>("AiDEAAPIBaseUrl").TrimEnd('/')}{apiPartUrl}/{controller}/{method}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
            request.Headers.Add("tb-access-token", GetAuthorizationToken());

            if (parameters != null)
            {
                if (httpMethod == HttpMethod.Get)
                {
                    url += "?";
                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        url += $"{param.Key}={param.Value}&";
                    }
                    if (url.EndsWith("&"))
                        url = url.Remove(url.Length - 1);
                    request.RequestUri = new Uri(url);
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(parameters), System.Text.Encoding.UTF8, "application/json");
                }
            }

            TraceLog("Calling API url " + url);
            HttpResponseMessage response = client.SendAsync(request).Result;
            client.Dispose();

            if (response != null && !response.IsSuccessStatusCode)
                throw new Exception($"{RetrieveLocalizedString("error_CallApiFail")} - {response.StatusCode}");
        }

        protected void CallWebApi(string controller, string method, HttpMethod httpMethod, List<object> parameters, bool hasApi = true)
        {
            string apiPartUrl = hasApi ? "/api" : String.Empty;
            string url = $"{RetriveConfiguration<string>("AiDEAAPIBaseUrl").TrimEnd('/')}{apiPartUrl}/{controller}/{method}";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage(httpMethod, url);
            request.Headers.Add("tb-access-token", GetAuthorizationToken());

            if (parameters != null)
            {
                if (httpMethod == HttpMethod.Get)
                {
                    if (parameters.Count != 1)
                    {
                        throw new Exception("You can't send arrays on Get methods");
                    }

                    url += "?";

                    object element = parameters.FirstOrDefault();
                    foreach (var prop in element.GetType().GetProperties())
                    {
                        url += $"{prop.Name}={prop.GetValue(element)}&";
                    }

                    if (url.EndsWith("&"))
                        url = url.Remove(url.Length - 1);

                    request.RequestUri = new Uri(url);
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(parameters), System.Text.Encoding.UTF8, "application/json");
                }
            }

            TraceLog("Calling API url " + url);
            HttpResponseMessage response = client.SendAsync(request).Result;
            client.Dispose();

            if (response != null && !response.IsSuccessStatusCode)
                throw new Exception($"{RetrieveLocalizedString("error_CallApiFail")} - {response.StatusCode}");
        }

        protected string GenerateMultiLookupField(IEnumerable<string> values)
        {
            string toReturn = string.Join(", ", values);
            if (toReturn.EndsWith(", "))
                toReturn = toReturn.Remove(toReturn.Length - 2);
            return toReturn;
        }

        #endregion

        private int? RetrieveUsersTimeZoneCode(EntityReference user = null)
        {
            ConditionExpression cd = null;
            if (user == null)
                cd = new ConditionExpression("systemuserid", ConditionOperator.EqualUserId);
            else
                cd = new ConditionExpression("systemuserid", ConditionOperator.Equal, user.Id);
            var currentUserSettings = Service.RetrieveMultiple(
            new QueryExpression("usersettings")
            {
                ColumnSet = new ColumnSet("localeid", "timezonecode"),
                Criteria = new FilterExpression
                {
                    Conditions = { cd }
                }
            }).Entities[0].ToEntity<Entity>();
            return (int?)currentUserSettings.Attributes["timezonecode"];
        }

        protected bool CheckEntityBuOwnerAndActiveOnRelated(EntityReference baseEntity, EntityReference relatedEntity, bool CheckRelatedStatus = true)
        {
            Guid baseOwningBuId = GetOwningBU(baseEntity.LogicalName, baseEntity.Id).Id;
            Entity baseBuData = GetOwningBUData(baseOwningBuId);
            Guid baseParentBuId = baseBuData.Attributes.ContainsKey("parentbusinessunitid") ? baseBuData.GetAttributeValue<EntityReference>("parentbusinessunitid").Id : Guid.Empty;

            Guid relatedOwningBuId = GetOwningBU(relatedEntity.LogicalName, relatedEntity.Id, CheckRelatedStatus).Id;

            return relatedOwningBuId != Guid.Empty && (relatedOwningBuId == baseOwningBuId || relatedOwningBuId == baseParentBuId);
        }

        protected EntityReference GetOwningBU(String entity, Guid id, bool checkStatus = false)
        {
            Entity owningBU = Service.Retrieve(entity, id, new ColumnSet("owningbusinessunit", "statecode"));
            if (checkStatus && owningBU.Attributes.ContainsKey("statecode") && owningBU.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
            {
                return new EntityReference("businessunit", Guid.Empty);
            }
            return owningBU.GetAttributeValue<EntityReference>("owningbusinessunit");
        }

        protected Entity GetOwningBUData(Guid id)
        {
            return Service.Retrieve("businessunit", id, new ColumnSet("businessunitid", "parentbusinessunitid", "tb_butype"));
        }

        protected IEnumerable<Entity> GetUserRoles(EntityReference user = null)
        {

            QueryExpression qe = new QueryExpression("role");
            qe.ColumnSet = new ColumnSet("name");

            LinkEntity link1 = new LinkEntity("role", "systemuserroles", "roleid", "roleid", JoinOperator.Inner);
            LinkEntity link2 = new LinkEntity("systemuserroles", "systemuser", "systemuserid", "systemuserid", JoinOperator.Inner);
            if (user == null)
            {
                link2.LinkCriteria.AddCondition("systemuserid", ConditionOperator.EqualUserId);
            }
            else
            {
                link2.LinkCriteria.AddCondition("systemuserid", ConditionOperator.Equal, user.Id);
            }

            link1.LinkEntities.Add(link2);
            qe.LinkEntities.Add(link1);

            FilterExpression childFilter = qe.Criteria.AddFilter(LogicalOperator.Or);
            childFilter.AddCondition("name", ConditionOperator.Like, "%AiDEA%");
            childFilter.AddCondition("name", ConditionOperator.Equal, "System Administrator");
            qe.Criteria.AddFilter(childFilter);
            return Service.RetrieveMultiple(qe).Entities;
        }

        protected String GetUserRole(EntityReference user = null)
        {
            IEnumerable<Entity> roles = GetUserRoles(user);

            Entity role = roles.First();

            if (!role.Attributes.ContainsKey("name") || role.GetAttributeValue<String>("name") == null || role.GetAttributeValue<String>("name") == "")
            {
                throw new Exception(RetrieveLocalizedString("error_WrongUserRoles"));
            }

            return role.GetAttributeValue<String>("name").Replace("AiDEA - ", "").ToUpper();
        }

        protected Entity getUserData(Guid userId, ColumnSet columns)
        {
            Entity user = Service.Retrieve("systemuser", userId, columns);
            return user;
        }
    }
}