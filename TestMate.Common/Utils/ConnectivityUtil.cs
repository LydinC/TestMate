using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestMate.Common.Models.Devices;

namespace TestMate.Common.Utils
{
    public class ConnectivityUtil
    {
        public static bool IsIpReachable(string ip, int timeoutInMs)
        {
            var options = new PingOptions
            {
                DontFragment = true
            };
            var buffer = new byte[32];
            var ping = new Ping();
            var reply = ping.Send(ip, timeoutInMs, buffer, options);

            return reply.Status == IPStatus.Success;
        }

        public static int GetAvailableTcpIpPort()
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static bool TryADBConnect(string serialNumber, string ip, int port) {
            try
            {
                string adbTcpipCommand = $"adb -s {serialNumber} tcpip {port}";
                string tcpipOutput = ExecuteADBCommand(adbTcpipCommand);

                string adbConnectCommand = $"adb connect {ip}:{port}";
                string connectOutput = ExecuteADBCommand(adbConnectCommand);

                if (connectOutput.Contains($"connected to {ip}:{port}"))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message + " - STACK: " + ex.StackTrace);
                return false;
            }
        }

        public static bool TryADBConnect(string ip, int port)
        {
            try
            {
                string adbConnectCommand = $"adb connect {ip}:{port}";
                string connectOutput = ExecuteADBCommand(adbConnectCommand);

                if (connectOutput.Contains($"connected to {ip}:{port}"))
                {
                    return true;
                } else 
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message + " - STACK: " + ex.StackTrace);
                return false;
            }
        }


        public static bool DisconnectADBDevice(string ip, int port)
        {
            try
            {
                string adbCommand = $"adb disconnect {ip}:{port}";
                string output = ExecuteADBCommand(adbCommand);

                if (output.Contains($"disconected {ip}:{port}"))
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message + " - STACK: " + ex.StackTrace);
                return false;
            }
        }


        public static List<string> GetADBDevices()
        {
            List<string> deviceSerials = new List<string>();
         
            try
            {
                string adbCommand = "adb devices";
                string output = ExecuteADBCommand(adbCommand);
                
                var serialRegexPattern = new Regex("^([a-zA-Z0-9\\-]+)(\\s+)(device)");
                var ipRegexPattern = new Regex(@"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\:(\d{1,5})\s+device");
                var lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (serialRegexPattern.IsMatch(line))
                    {
                        var match = serialRegexPattern.Match(line);
                        deviceSerials.Add(match.Groups[1].Value);
                    }
                    else if (ipRegexPattern.IsMatch(line)) {
                        var match = ipRegexPattern.Match(line);
                        deviceSerials.Add(match.Groups[1].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
            }
            return deviceSerials;
        }

        public static DeviceProperties GetDeviceProperties(string ip, int port)
        {
            string[] properties = {
                "ro.product.model",
                "ro.product.manufacturer",
                "ro.build.version.release",
                "ro.product.locale",
                "ro.product.brand",
                "ro.build.version.sdk",
                "gsm.operator.alpha",
                "ro.product.cpu.abi",
                "persist.sys.timezone",
                //wm size is embedded in command
            };

            string propertiesList = string.Join("|", properties);

            string adbCcommand = $"adb -s {ip}:{port} shell 'getprop && wm size && dumpsys battery' | Select-String -Pattern '{propertiesList}|Physical size|level|scale|AC powered|USB powered|Wireless powered'";
            var output = ExecuteADBCommand(adbCcommand);
            
            output = output.Replace("[", "").Replace("]", "");
            
            return MapDeviceProperties(output);
        }
        private static DeviceProperties MapDeviceProperties(string adbOutput)
        {
            var properties = adbOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            DeviceProperties deviceProperties = new DeviceProperties();
            BatteryInfo batteryInfo = new BatteryInfo();
            deviceProperties.Battery = batteryInfo;
            
            foreach (var property in properties)
            {
                var key = property.Split(':')[0].Trim();
                var value = property.Split(':')[1].Trim();
                switch (key)
                {
                    case "ro.product.model":
                        deviceProperties.Model = value;
                        break;
                    case "ro.product.manufacturer":
                        deviceProperties.Manufacturer = value;
                        break;
                    case "ro.build.version.release":
                        deviceProperties.AndroidVersion = int.Parse(value);
                        break;
                    case "ro.product.locale":
                        deviceProperties.Locale = value;
                        break;
                    case "ro.product.brand":
                        deviceProperties.Brand = value;
                        break;
                    case "ro.build.version.sdk":
                        deviceProperties.SdkVersion = int.Parse(value);
                        break;
                    case "gsm.operator.alpha":
                        deviceProperties.Operator = value;
                        break;
                    case "Physical size":
                        string[] resolution = value.Split("x");
                        deviceProperties.ScreenResolution = new ScreenResolution(int.Parse(resolution[0]), int.Parse(resolution[1]));
                        break;
                    case "ro.product.cpu.abi":
                        deviceProperties.ProcessorType = value;
                        break;
                    case "persist.sys.timezone":
                        deviceProperties.TimeZone = value;
                        break;

                    //Battery Related
                    case "level":
                        batteryInfo.Level = int.Parse(value);
                        break;
                    case "scale":
                        batteryInfo.Scale = int.Parse(value);
                        break;
                    case "AC powered":
                        batteryInfo.ACPowered = Boolean.Parse(value);
                        break;
                    case "USB powered":
                        batteryInfo.USBPowered = Boolean.Parse(value);
                        break;
                    case "Wireless powered":
                        batteryInfo.WirelessPowered = Boolean.Parse(value);
                        break;

                }
            }

            return deviceProperties;
        }

        public static bool ValidateSerialNumberAndIP(string serialNumber, string ip)
        {
            string adbCommand = $"adb -s {serialNumber} shell ip -f inet addr show wlan0";

            string output = ExecuteADBCommand(adbCommand);

            if (string.IsNullOrEmpty(output))
            {
                Console.WriteLine("Unable to retrieve device IP.");
                return false;
            }

            string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                if (line.Contains("inet "))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string deviceIp = parts[1].Split("/")[0];

                    if (deviceIp == ip)
                    {
                        return true;
                    }

                    Console.WriteLine($"Expected IP: {ip}, but got: {deviceIp}");
                    return false;
                }
            }

            Console.WriteLine("Unable to retrieve device IP.");
            return false;
            }

        public static string GetSerialNumberByIp(string ip) {
            string adbCcommand = $"adb -s {ip} shell getprop ro.serialno";
            var output = ExecuteADBCommand(adbCcommand);
            return output;
        }

        public static string ExecuteADBCommand(string command)
        {
            string output;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"{command}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit(new TimeSpan(1000));
                
                output = process.StandardOutput.ReadToEnd().Trim();
            }

            return output;
        }
    }
}
