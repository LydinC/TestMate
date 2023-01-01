using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.Models.Devices
{
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string IMEI { get; set; } = null!;

        [Required]
        public DateTime RegistrationTimestamp { get; set; }


        //TO CHECK VALIDITY?
        [Required]
        public DateTime ExpirationTimestamp { get; set; }

        [Required]
        public DeviceProperties DeviceProperties { get; set; } = null!;

        //[Required]
        //public Dictionary<string, string> capabilities { get; set; } = null!;

        //[Required]
        //public Dictionary<string, string> additionalCapabilities { get; set; } = null!;


    }
}
