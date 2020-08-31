using Mwp.AzureKeyRotation.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Mwp.AzureKeyRotation
{
    [DependsOn(
        typeof(AzureKeyRotationEntityFrameworkCoreTestModule)
        )]
    public class AzureKeyRotationDomainTestModule : AbpModule
    {

    }
}