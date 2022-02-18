using Microsoft.Xrm.Sdk;
using System;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Base;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Model;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Archive.PrivacyConsent.BusinessLogic
{
    public class AideaAngCpCustomPrivacyConsentBusinessLogic : BusinessLogicBase
    {
        #region Ctor

        public AideaAngCpCustomPrivacyConsentBusinessLogic(PluginBase plugin) : base(plugin) { }

        #endregion

        #region Public Methods

        public override void ApplyLogic()
        {
            if (MessageName == ContextMessageName.Create)
                ApplyCreateLogic();
            else if (MessageName == ContextMessageName.Update)
                ApplyUpdateLogic();
        }

        #endregion

        #region Create Logic

        private void ApplyCreateLogic()
        {
            TraceLog("Apply create business logic");
            Entity target = GetTargetEntity();
            EntityReference hcpER;

            if (!target.Contains("tb_hcp"))
                throw new Exception(RetrieveLocalizedString("error_NoContact"));
            else 
                hcpER =  target.GetAttributeValue<EntityReference>("tb_hcp");

            if (hcpER != null)
            {
                Entity contact = new Entity("contact");
                contact["contactid"] = hcpER.Id;
                contact["tbc_externalprivacydatetime"] = null;
                contact["tbc_externalprivacysource"] = null;
                Service.Update(contact);
            }
        }

        #endregion

        #region Update Logic

        public void ApplyUpdateLogic()
        {
        }

        #endregion

        #region Common
        #endregion
    }
}
