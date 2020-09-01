using Mwp.SharedResource;

namespace Mwp.Storage
{
    public class StorageAccessKey
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }

        public string ConnectionString =>
            SharedResourceSecret.StorageConnectionStringTemplate
            .Replace("{name}", AccountName)
            .Replace("{key}", AccountKey);
    }
}