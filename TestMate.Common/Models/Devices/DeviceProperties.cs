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
        public string ProcessorType { get; set; }
        [Required]
        public ScreenResolution ScreenResolution{ get; set; }
        [Required]
        public Battery Battery { get; set; }
        [Required]
        public string TimeZone { get; set; }
    }

    public class ScreenResolution {
        public int Height { get; set; }
        public int Width { get; set; }

        public ScreenResolution(int height, int width) { 
            this.Height = height;
            this.Width = width;
        }
    }

    public class Battery
    {
        public int Level { get; set; }
        public int Scale { get; set; }
        public bool ACPowered { get; set; }
        public bool WirelessPowered { get; set; }
        public bool USBPowered { get; set; }
        
        public Battery(int level = 0, int scale = 0, bool acPowered = false, bool usbPowered = false, bool wirelessPowered = false)
        {
            this.Level = Level;
            this.Scale = Scale;
            this.ACPowered = acPowered;
            this.USBPowered = usbPowered;
            this.WirelessPowered = wirelessPowered;
        }
    }
}
