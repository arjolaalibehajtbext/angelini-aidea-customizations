using TrueBlue.Aidea.Plugin.AngCp.Customization.Archive.PrivacyConsent.BusinessLogic;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Base;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Archive.PrivacyConsent
{
    public class AideaAngCpCustomPrivacyConsentPlugin : PluginBase
    {
        public AideaAngCpCustomPrivacyConsentPlugin() : base("AideaAngCpCustomPrivacyConsentPlugin") { }

        public override void ExecutePlugin()
        {
            AideaAngCpCustomPrivacyConsentBusinessLogic bl = new AideaAngCpCustomPrivacyConsentBusinessLogic(this);
            bl.ApplyLogic();
        }
    }
}