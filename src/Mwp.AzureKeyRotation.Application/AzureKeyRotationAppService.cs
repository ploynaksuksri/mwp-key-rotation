using Mwp.AzureKeyRotation.Localization;
using Volo.Abp.Application.Services;

namespace Mwp.AzureKeyRotation
{
    /* Inherit your application services from this class.
     */
    public abstract class AzureKeyRotationAppService : ApplicationService
    {
        protected AzureKeyRotationAppService()
        {
            LocalizationResource = typeof(AzureKeyRotationResource);
        }
    }
}
