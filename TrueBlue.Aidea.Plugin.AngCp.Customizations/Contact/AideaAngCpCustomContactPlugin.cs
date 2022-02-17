using TrueBlue.Aidea.Plugin.AngCp.Customization.Archive.Contact.BusinessLogic;
using TrueBlue.Aidea.Plugin.AngCp.Customization.Common.Base;

namespace TrueBlue.Aidea.Plugin.AngCp.Customization.Archive.Contact
{
    public class AideaAngCpCustomContactPlugin : PluginBase
    {
        public AideaAngCpCustomContactPlugin() : base("AideaAngCpCustomContactPlugin") { }

        public override void ExecutePlugin()
        {
            AideaAngCpCustomContactBusinessLogic bl = new AideaAngCpCustomContactBusinessLogic(this);
            bl.ApplyLogic();
        }
    }
}