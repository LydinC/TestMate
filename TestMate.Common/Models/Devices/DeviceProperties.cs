using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.Models.Devices
{
    public class DeviceProperties
    {
        [Required]
        public string Manufacturer { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public string Brand { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Locale { get; set; }

        [Required]
        public string DeviceModel { get; set; }

        [Required]
        public string VendorBrand { get; set; }

        [Required]
        public List<string> OtherProperties { get; set; }

    }
}
