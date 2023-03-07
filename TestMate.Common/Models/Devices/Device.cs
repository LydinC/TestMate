using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Enums;
using TestMate.Common.Utils;

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

        [Required(ErrorMessage = "Connected Timestamp is required!")]
        public DateTime ConnectedTimestamp { get; set; }

        [Required(ErrorMessage = "Device Status is required!")]
        public DeviceStatus Status  { get; set; }

        [Required(ErrorMessage = "Device Properties are required!")]
        public DeviceProperties DeviceProperties { get; set; } = null!;


        public bool SetBluetooth(bool enable)
        {
            string setStatus = enable ? "enable" : "disable";
            string adbCommand = $"-s {this.IP}:{this.TcpIpPort} shell am broadcast -a io.appium.settings.bluetooth --es setstatus {setStatus}";
            string output = ConnectivityUtil.ExecuteADBCommand(adbCommand);
            return output.Contains("Broadcast completed: result=-1");
        }

        public bool SetAirplaneMode(bool enable)
        {
            string setStatus = enable ? "enable" : "disable";
            string command = $"-s {this.IP}:{this.TcpIpPort} shell cmd connectivity airplane-mode {setStatus}";
            string output = ConnectivityUtil.ExecuteADBCommand(command);
            return output.Equals("");
        }

        public bool SetBrightness(bool max)
        {
            //turn off automatic brightness mode first
            string command = $"-s {this.IP}:{this.TcpIpPort} shell settings put system screen_brightness_mode 0";
            string output = ConnectivityUtil.ExecuteADBCommand(command);

            //currently only catering for dimmest or brightest scenarios
            string setLevel = max ? "50000" : "0";
            command = $"-s {this.IP}:{this.TcpIpPort} shell settings put system screen_brightness {setLevel}";
            output = ConnectivityUtil.ExecuteADBCommand(command);
            
            return output.Equals("");
        }

        public bool SetAutoRotateMode(bool enable)
        {
            string setStatus = enable ? "1" : "0";
            string command = $"-s {this.IP}:{this.TcpIpPort} shell settings put system accelerometer_rotation {setStatus}";
            string output = ConnectivityUtil.ExecuteADBCommand(command);
            return output.Equals("");
        }

        public bool SetLocation(bool enable)
        {
            string setStatus = enable ? LocationMode.On.ToString() : LocationMode.Off.ToString();
            string command = $"-s {this.IP}:{this.TcpIpPort} shell settings put secure location_mode {setStatus}";
            string output = ConnectivityUtil.ExecuteADBCommand(command);
            return output.Equals("");
        }

        public bool SetOrientation(int orientation)
        {
            if(!SetAutoRotateMode(enable: false)) return false; 

            string command = $"-s {this.IP}:{this.TcpIpPort} shell settings put system user_rotation {orientation}";
            string output = ConnectivityUtil.ExecuteADBCommand(command);
            return output.Equals("");
        }

        public bool SetVolume(bool max)
        {
            string keycode = max ? "KEYCODE_VOLUME_UP" : "KEYCODE_VOLUME_DOWN";

            string command = $"-s {this.IP}:{this.TcpIpPort} shell input keyevent {keycode}";
            for(int i =0; i < 20; i++)
            {
                string output = ConnectivityUtil.ExecuteADBCommand(command);
                if(output != "") { return false; }
            }
            return true;
        }



    }
}
