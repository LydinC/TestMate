using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestMate.Common.Enums;

namespace TestMate.Common.Models.TestRequests
{
    public class TestRunConfiguration
    {
        [Required]
        public string ApplicationUnderTest { get; set; }

        [Required]
        public string TestSolutionPath { get; set; }


        [Required]
        public string DesiredDeviceProperties { get; set; } = null!;

        [Required]
        public TestRunConstraints Constraints { get; set; }

        public ContextConifguration ContextConifguration { get; set; } = null!;


        public TestRunConfiguration(string applicationUnderTest, string testSolutionPath, string desiredDeviceProperties, TestRunConstraints constraints, ContextConifguration contextConifguration) {
            ApplicationUnderTest = applicationUnderTest;
            TestSolutionPath = testSolutionPath;
            DesiredDeviceProperties = desiredDeviceProperties;
            Constraints = constraints;
            ContextConifguration = contextConifguration;
        }
    
    }


    public class TestRunConstraints 
    {
        public int maxNumberOfDevices { get; set; }
        public TimeOnly totalRunDuration { get; set; }
        public int maxNumberOfContexts { get; set; }
    }

    public class ContextConifguration
    {

        //STILL TO CHECK IF MANIPULATING THESE OPTIONS IS VIABLE OR NOT

        public DeviceRingMode RingMode { get; set; }

        public int MediaVolume { get; set; }
        //adb shell media volume --set 15 --show //0-15

        public int BrightnessLevel { get; set; }
        //adb shell settings put system screen_brightness [0-255]

        public bool BatteryPowerSavingOn { get; set; }

        public bool AirplaneModeOn { get; set; }
        //adb shell settings put global airplane_mode_on 1
        //adb shell am broadcast -a android.intent.action.AIRPLANE_MODE

        public bool FlashlightOn { get; set; }

        public bool AutoRotateOn { get; set; }
        //adb shell settings put system accelerometer_rotation 0

        public bool LocationOn { get; set; }

        public bool DoNotDisturbOn { get; set; }

        public bool BluetoothOn { get; set; }
        //adb shell am broadcast -a io.appium.settings.bluetooth --es setstatus disable 

        public DeviceScreenOrientation Orientation { get; set; }
        //adb shell settings put system accelerometer_rotation 0  #disable auto-rotate
        //adb shell settings put system user_rotation 3  #270° clockwise
        //accelerometer_rotation: auto-rotation, 0 disable, 1 enable
        //user_rotation: actual rotation, clockwise, 0 0°, 1 90°, 2 180°, 3 270°


    }




}
