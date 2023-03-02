using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestMate.Common.Enums;

namespace TestMate.Common.Models.TestRequests
{
    public class TestRequestConfiguration
    {
        [Required]
        public string ApkPath { get; set; } = null!;

        [Required]
        public string TestExecutablePath { get; set; } = null!;

        [Required]
        public DesiredDeviceProperties DesiredDeviceProperties { get; set; } = null!;
        
        public List<DesiredContextConfiguration>? DesiredContextConfiguration { get; set; }

        //[Required]
        //public TestRequestConstraints Constraints { get; set; }

        public TestRequestConfiguration(string apkPath, string testExecutablePath, DesiredDeviceProperties desiredDeviceProperties, List<DesiredContextConfiguration>? desiredContextConfiguration) {
            ApkPath = apkPath;
            TestExecutablePath = testExecutablePath;
            DesiredDeviceProperties = desiredDeviceProperties;
            DesiredContextConfiguration = desiredContextConfiguration;
            //Constraints = constraints;
        }
    }

    public class DesiredContextConfiguration
    {
        public List<bool>? BluetoothOn { get; set; }
        //adb shell am broadcast -a io.appium.settings.bluetooth --es setstatus disable

        public List<bool>? AirplaneModeOn { get; set; }
        //adb shell settings put global airplane_mode_on 1
        //adb shell am broadcast -a android.intent.action.AIRPLANE_MODE

        public List<bool>? BrightnessMode{ get; set; }
        //adb shell settings put system screen_brightness [0-255]

        public List<bool>? AutoRotateMode { get; set; }
        //adb shell settings put system accelerometer_rotation 0  #disable auto-rotate

        public List<DeviceScreenOrientation>? Orientation { get; set; }
        //user_rotation: actual rotation, clockwise, 0 0°, 1 90°, 2 180°, 3 270°

        public List<bool>? LocationOn { get; set; }

        public List<int>? MediaVolume { get; set; }
        //adb shell media volume --set 15 --show //0-15


       

        public List<bool>? FlashlightOn { get; set; }

       

        //TODO: STILL TO CHECK IF MANIPULATING THESE OPTIONS IS VIABLE OR NOT
        //public List<DeviceRingMode> RingMode { get; set; }
        //public bool BatteryPowerSavingOn { get; set; }
        //public bool AutoRotateOn { get; set; }
        //adb shell settings put system accelerometer_rotation 0
        //
        //public bool DoNotDisturbOn { get; set; }

    }

    public class TestRequestConstraints
    {
        public int maxNumberOfDevices { get; set; }
        public TimeOnly totalRunDuration { get; set; }
        public int maxNumberOfContexts { get; set; }
    }

}
