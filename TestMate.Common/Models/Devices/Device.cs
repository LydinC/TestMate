using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Enums;

namespace TestMate.Common.Models.Devices
{
    public class Device
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Serial Number is required!")]
        public string SerialNumber { get; set; } = null!;

        [Required(ErrorMessage = "IP is required!")]
        public string IP { get; set; } = null!;

        [Required(ErrorMessage = "IP is required!")]
        public int TcpIpPort { get; set; }

        [Required(ErrorMessage = "Registration Timestamp is required!")]
        public DateTime ConnectedTimestamp { get; set; }

        [Required(ErrorMessage = "Device Status is required!")]
        public DeviceStatus Status  { get; set; }

        [Required(ErrorMessage = "Device Properties are required!")]
        public DeviceProperties DeviceProperties { get; set; } = null!;

    }
}
