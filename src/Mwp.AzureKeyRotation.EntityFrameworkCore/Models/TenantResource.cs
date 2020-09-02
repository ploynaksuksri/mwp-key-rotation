using System;

namespace Mwp.Models
{
    public class TenantResource
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public int CloudServiceLocationId { get; set; }
        public int CloudServiceOptionId { get; set; }
    }
}