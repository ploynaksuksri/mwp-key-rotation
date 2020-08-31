using Volo.Abp.Modularity;

namespace Mwp.AzureKeyRotation
{
    [DependsOn(
        typeof(AzureKeyRotationApplicationModule),
        typeof(AzureKeyRotationDomainTestModule)
        )]
    public class AzureKeyRotationApplicationTestModule : AbpModule
    {

    }
}