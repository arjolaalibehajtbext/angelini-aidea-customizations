using Microsoft.Xrm.Sdk;
using System.Net.Http;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Base;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Model;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Archive.Contact.BusinessLogic
{
    public class AideaAngCpCustomContactBusinessLogic : BusinessLogicBase
    {
        #region Ctor

        public AideaAngCpCustomContactBusinessLogic(PluginBase plugin) : base(plugin) { }

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
