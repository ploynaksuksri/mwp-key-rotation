using Volo.Abp.Settings;

namespace Mwp.AzureKeyRotation.Settings
{
    public class AzureKeyRotationSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            //Define your own settings here. Example:
            //context.Add(new SettingDefinition(AzureKeyRotationSettings.MySetting1));
        }
    }
}
