namespace Mwp.Constants
{
    public class SharedResourceSecret
    {
        public const string KeyProperty = "Key";
        public const string ResourceGroupProperty = "ResourceGroup";
        public const string ResourceNameProperty = "Name";
        public const string SubscriptionIdProperty = "SubscriptionId";

        public const string StorageKey1Name = "key1";

        public const string StorageConnectionStringTemplate =
            "DefaultEndpointsProtocol=https;AccountName={name};AccountKey={key};EndpointSuffix=core.windows.net";

        public const int ValidityPeriodDays = 30;
    }
}