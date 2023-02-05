using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.Models.Devices
{
    public class DeviceProperties
    {
        [Required]
        public string Model { get; set; }
        [Required]
        public string Manufacturer { get; set; }
        [Required]
        public string AndroidVersion { get; set; }
        [Required]
        public int SdkVersion { get; set; }
        [Required]
        public string Locale { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public string Operator { get; set; }
        [Required]
        public string Language { get; set; }

        public List<string> OtherProperties { get; set; }

    }
}
