using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Utilities
{
    public class Utils
    {
        #region Properties
        IOrganizationService Service;
        Guid userId;
        #endregion

        #region Ctor
        public Utils(IOrganizationService Service, Guid userId)
        {
            this.Service = Service;
            this.userId = userId;
        }
        #endregion

        #region Public Methods

        #endregion

        #region Utilities

        public static string JsonSerializer<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }

        public static Object JsonDeserialize(string jsonString)
        {
            return JsonConvert.DeserializeObject(jsonString);
        }

        public static Entity mergeImages(Entity target, Entity image)
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

        public T RetriveConfiguration<T>(string key, string FallbackValue = "keyNotSet")
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
                    <filter>
                      <condition attribute='statecode' operator='eq' value='0'/>
                      <condition attribute='statuscode' operator='eq' value='1'/>
                    </filter>
                    <link-entity name='businessunit' from='businessunitid' to='owningbusinessunit' link-type='inner'>
                      <link-entity name='systemuser' from='businessunitid' to='businessunitid' link-type='inner'>
                        <filter>
                          <condition attribute='systemuserid' operator='eq' value='{userId}' />
                        </filter>
                      </link-entity>
                    </link-entity>
                  </entity>
                </fetch>";
            Entity config = Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities.FirstOrDefault();
            string value = FallbackValue;
            if (FallbackValue == "keyNotSet" && config == null)
            {
                throw new Exception(RetrieveLocalizedString("error_NoKey", new Dictionary<string, string>() { { "key", key } }));
            }
            if (config != null && config.Contains("tb_value"))
                value = config.GetAttributeValue<string>("tb_value");
            if (string.IsNullOrEmpty(value))
                throw new Exception(RetrieveLocalizedString("error_NullKey", new Dictionary<string, string>() { { "key", key } }));
            return (T)Convert.ChangeType(value, typeof(T));
        }

        /*protected Entity RetriveWebResource(string name)
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
        }*/

        /*protected void SendEmail(string emailTemplate, EntityReference recipient, bool useThisUserAsSender = false, string sender = null, Dictionary<string, byte[]> attachments = null)
        {
            SendEmail(emailTemplate, new List<EntityReference> { recipient }, useThisUserAsSender, sender, attachments);
        }*/

        /*protected void SendEmail(string emailTemplate, IEnumerable<EntityReference> recipients, bool useThisUserAsSender = false, string sender = null, Dictionary<string, byte[]> attachments = null)
        {
            OrganizationRequest req = new OrganizationRequest("tb_SendEmail");
            req["TemplateName"] = emailTemplate;
            req["Recipients"] = JsonConvert.SerializeObject(recipients);
            req["UseUserAsSender"] = useThisUserAsSender;
            req["Sender"] = sender;

            if (attachments != null)
                req["Attachment"] = JsonConvert.SerializeObject(attachments.ToDictionary(a => a.Key, a => Convert.ToBase64String(a.Value)));

            OrganizationResponse response = Service.Execute(req);
        }*/

        /*protected string GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            token = Uri.EscapeDataString(token).Replace("%", string.Empty);
            return token;
        }*/

        public int GetUserLangCode()
        {
            Entity user = Service.Retrieve("usersettings", userId, new ColumnSet("uilanguageid"));
            return user.GetAttributeValue<int>("uilanguageid");
        }

        public string RetrieveLocalizedString(string resourceId, Dictionary<string, string> param = null, string ResourceFileName = "common")
        {
            string BASE_LANGUAGE_WEBRESOURCE_NAME = $"tb_common/lang/{ResourceFileName}";

            string localLangName = $"{BASE_LANGUAGE_WEBRESOURCE_NAME}.{GetUserLangCode()}.resx";
            string baseLangName = $"{BASE_LANGUAGE_WEBRESOURCE_NAME}.1033.resx";
            var fetchXml = $@"
                <fetch>
                  <entity name='webresource'>
                    <attribute name='name' />
                    <attribute name='contentjson' />
                    <filter>
                      <condition attribute='name' operator='in'>
                        <value>{localLangName}</value>
                        <value>{baseLangName}</value>
                      </condition>
                    </filter>
                    </entity>
                </fetch>";
            IEnumerable<Entity> languages = Service.RetrieveMultiple(new FetchExpression(fetchXml)).Entities;

            Entity localLang = languages.FirstOrDefault(x => (string)x["name"] == localLangName);
            Entity baseLang = languages.FirstOrDefault(x => (string)x["name"] == baseLangName);
            string escapedString = "";
            if (localLang != null)
                escapedString = localLang.GetAttributeValue<string>("contentjson");
            else if (baseLang != null)
                escapedString = baseLang.GetAttributeValue<string>("contentjson");
            else
                throw new Exception($"Failed to load language file");
            Dictionary<string, string> localizatedstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(escapedString);

            string toReturn = localizatedstring.ContainsKey(resourceId) ? localizatedstring[resourceId] : resourceId;

            if (param != null)
            {
                foreach (KeyValuePair<string, string> p in param)
                {
                    toReturn = toReturn.Replace($"##{p.Key}##", p.Value);
                }
            }
            return toReturn;
        }

        /*public DateTime RetrieveLocalTimeFromUTCTime(DateTime UtcTime)
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
        }*/

        /*public DateTime RetrieveUTCTimeFromLocalTime(DateTime LocalTime)
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
        }*/

        /*protected IEnumerable<Entity> GetConflictingEntities(IEnumerable<Entity> entities, Entity item)
        {
            DateTime validFrom = item.GetAttributeValue<DateTime>("tb_validfrom").Date;
            DateTime validTo = item.Contains("tb_validto") ? item.GetAttributeValue<DateTime>("tb_validto").Date : DateTime.MaxValue.Date;
            return entities.Where(x =>
            {
                DateTime x_validFrom = x.GetAttributeValue<DateTime>("tb_validfrom").Date;
                DateTime x_validTo = x.Contains("tb_validto") ? x.GetAttributeValue<DateTime>("tb_validto").Date : DateTime.MaxValue.Date;
                return validFrom <= x_validTo && x_validFrom <= validTo;
            });
        }*/

        /*protected void CheckMandatoryFields(params string[] fields)
        {
            TraceLog("Check mandatory fields");

            Entity target = GetTargetEntity();
            string fieldInError = null;

            if (MessageName == ContextMessageName.Create)
                fieldInError = fields.FirstOrDefault(f => !target.Contains(f) || target[f] == null);
            else if (MessageName == ContextMessageName.Update)
                fieldInError = fields.FirstOrDefault(f => (!GetPreEntity().Contains(f) && !target.Contains(f)) || (target.Contains(f) && target[f] == null));

            if (fieldInError != null)
                throw new Exception(RetrieveLocalizedString("error_MissingField", new Dictionary<string, string>() { { "field", GetLocalizatedFied(fieldInError) } }));
        }*/

        /*protected void CheckNotUpdatableFields(params string[] fields)
        {
            TraceLog("Check not updatable fields");

            Entity target = GetTargetEntity();
            Entity pre = GetPreEntity();
            string fieldInError = null;

            if (MessageName == ContextMessageName.Update)
            {
                var inErrs = fields.Where(f => target.Contains(f) && (pre == null || !pre.Contains(f) || !pre[f].Equals(target[f])));
                fieldInError = inErrs.FirstOrDefault();
            }


            if (fieldInError != null)
                throw new Exception(RetrieveLocalizedString("error_UpdateField", new Dictionary<string, string>() { { "field", GetLocalizatedFied(fieldInError) } }));
        }*/

        /*protected bool IsUnique(bool checkOwner, params string[] fields)
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
        }*/

        /*protected string GetLocalizatedFied(string field)
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
        }*/

        /*protected bool CheckAideaRolePermission(string field)
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
        }*/

        /*private string GetAuthorizationToken()
        {
            TraceLog("Generate Token request");

            OrganizationRequest req = new OrganizationRequest("tb_GenerateApiToken");
            req["UserId"] = Context.UserId.ToString();
            OrganizationResponse response = Service.Execute(req);
            string token = (string)response.Results.FirstOrDefault().Value;

            TraceLog($"Retrieved Token {token}");

            return token;
        }*/

        /*protected void CallWebApi(string controller, string method, HttpMethod httpMethod, Dictionary<string, object> parameters = null, bool hasApi = true)
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
        }*/

        /*protected void CallWebApi(string controller, string method, HttpMethod httpMethod, List<object> parameters, bool hasApi = true)
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
        }*/

        /*protected string GenerateMultiLookupField(IEnumerable<string> values)
        {
            string toReturn = string.Join(", ", values);
            if (toReturn.EndsWith(", "))
                toReturn = toReturn.Remove(toReturn.Length - 2);
            return toReturn;
        }*/

        #endregion

        /*private int? RetrieveUsersTimeZoneCode(EntityReference user = null)
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
        }*/

        public bool CheckEntityBuOwnerAndActiveOnRelated(EntityReference baseEntity, EntityReference relatedEntity, bool CheckRelatedStatus = true)
        {
            Guid baseOwningBuId = GetOwningBU(baseEntity.LogicalName, baseEntity.Id).Id;
            Entity baseBuData = GetOwningBUData(baseOwningBuId);
            Guid baseParentBuId = baseBuData.Attributes.ContainsKey("parentbusinessunitid") ? baseBuData.GetAttributeValue<EntityReference>("parentbusinessunitid").Id : Guid.Empty;

            Guid relatedOwningBuId = GetOwningBU(relatedEntity.LogicalName, relatedEntity.Id, CheckRelatedStatus).Id;

            return relatedOwningBuId != Guid.Empty && (relatedOwningBuId == baseOwningBuId || relatedOwningBuId == baseParentBuId);
        }

        public EntityReference GetOwningBU(String entity, Guid id, bool checkStatus = false)
        {
            Entity owningBU = Service.Retrieve(entity, id, new ColumnSet("owningbusinessunit", "statecode"));
            if (checkStatus && owningBU.Attributes.ContainsKey("statecode") && owningBU.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
            {
                return new EntityReference("businessunit", Guid.Empty);
            }
            return owningBU.GetAttributeValue<EntityReference>("owningbusinessunit");
        }

        public Entity GetOwningBUData(Guid id)
        {
            return Service.Retrieve("businessunit", id, new ColumnSet("businessunitid", "parentbusinessunitid", "tb_butype"));
        }

        public IEnumerable<Entity> GetUserRoles(EntityReference user = null)
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

        public String GetUserRole(EntityReference user = null)
        {
            if (user == null)
            {
                user = new EntityReference("systemuser", userId);
            }
            IEnumerable<Entity> roles = GetUserRoles(user);

            Entity role = roles.First();

            if (!role.Attributes.ContainsKey("name") || role.GetAttributeValue<String>("name") == null || role.GetAttributeValue<String>("name") == "")
            {
                throw new Exception(RetrieveLocalizedString("error_WrongUserRoles"));
            }

            return role.GetAttributeValue<String>("name").Replace("AiDEA - ", "");
        }

        public Entity getUserData(Guid userId, ColumnSet columns)
        {
            Entity user = Service.Retrieve("systemuser", userId, columns);
            return user;
        }

        public string ConcatenateFetchInValues(EntityReferenceCollection types)
        {
            string concatenation = String.Empty;

            foreach (EntityReference type in types)
            {
                concatenation += "<value >{" + type.Id + "}</value>";
            }

            return concatenation;
        }
    }
}
