using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestMate.Common.Models.Devices;

namespace TestMate.Common.Models.TestRequests
{
    public class DesiredDeviceProperties
    {
        public List<string>? Model { get; set; }
        public List<string>? Manufacturer { get; set; }
        public string? AndroidVersion { get; set; }
        public string? SdkVersion { get; set; }
        public string? Locale { get; set; }
        public List<string>? Brand { get; set; }
        public List<string>? Operator { get; set; }
        public List<string>? ProcessorType { get; set; }
        public List<ScreenResolution>? ScreenResolution { get; set; }
        public List<string>? TimeZoneInfo { get; set; }
    }

}
