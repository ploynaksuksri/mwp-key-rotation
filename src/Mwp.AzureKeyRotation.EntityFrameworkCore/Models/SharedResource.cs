using System;

namespace Mwp.Models
{
    public class SharedResource
    {
        public Guid Id { get; set; }

        public int CloudServiceLocationId { get; set; }
        public int CloudServiceOptionId { get; set; }
        public string SecretName { get; set; }
    }
}