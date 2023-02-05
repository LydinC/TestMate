using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using TestMate.Common.Models.Devices;

namespace TestMate.Common.DataTransferObjects.Devices
{
    public class DevicesConnectDTO
    {

        //COULD NOT GET SERIAL NUMBER FROM DEVICE FOLLOWING ANDROID 10 DUE TO SECURITY 
        //[Required(ErrorMessage = "Serial Number is required!")]
        //public string SerialNumber { get; set; } = null!;

        [Required(ErrorMessage = "IP is required!")]
        public string IP { get; set; } = null!;

    }
}
