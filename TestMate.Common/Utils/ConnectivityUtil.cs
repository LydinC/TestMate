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
using TestMate.Common.Enums;
using TestMate.Common.Models.Devices;

namespace TestMate.Common.Utils
{
    public class ConnectivityUtil
    {
        public static string adbPath = "C:\\Users\\lydin.camilleri\\AppData\\Local\\Android\\Sdk\\platform-tools\\adb.exe";

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
                string adbTcpipCommand = $"-s {serialNumber} tcpip {port}";
                string tcpipOutput = ExecuteADBCommand(adbTcpipCommand);

                string adbConnectCommand = $"connect {ip}:{port}";
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
                string adbConnectCommand = $"connect {ip}:{port}";
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
                string adbCommand = $"disconnect {ip}:{port}";
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
                string adbCommand = "devices";
                string output = ExecuteADBCommand(adbCommand);

                Regex serialRegexPattern = new Regex("^([a-zA-Z0-9\\-]+)(\\s+)(device)");
                Regex ipRegexPattern = new Regex(@"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\:(\d{1,5})\s+device");
                string[] lines = output.Split(new string[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
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
                throw;
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
                //additional properties are included in adb command (ex: wm size & battery)
            };

            string propertiesList = string.Join("|", properties);

            string adbCcommand = $"-s {ip}:{port} shell 'getprop && wm size && dumpsys battery' | Select-String -Pattern '{propertiesList}|Physical size|level|scale|AC powered|USB powered|Wireless powered'";
            var output = ExecuteADBCommand(adbCcommand);
            
            output = output.Replace("[", "").Replace("]", "");
            
            return MapDeviceProperties(output);
        }

        private static DeviceProperties MapDeviceProperties(string adbOutput)
        {
            string[] properties = adbOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            DeviceProperties deviceProperties = new DeviceProperties();
            Battery battery = new Battery();
            deviceProperties.Battery = battery;
            
            foreach (string property in properties)
            {
                string key = property.Split(':')[0].Trim();
                string value = property.Split(':')[1].Trim();
                switch (key)
                {
                    case "ro.product.model":
                        deviceProperties.Model = value;
                        break;
                    case "ro.product.manufacturer":
                        deviceProperties.Manufacturer = value;
                        break;
                    case "ro.build.version.release":
                        deviceProperties.AndroidVersion = value;
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
                        battery.Level = int.Parse(value);
                        break;
                    case "scale":
                        battery.Scale = int.Parse(value);
                        break;
                    case "AC powered":
                        battery.ACPowered = Boolean.Parse(value);
                        break;
                    case "USB powered":
                        battery.USBPowered = Boolean.Parse(value);
                        break;
                    case "Wireless powered":
                        battery.WirelessPowered = Boolean.Parse(value);
                        break;

                }
            }

            return deviceProperties;
        }

        public static bool ValidateSerialNumberAndIP(string serialNumber, string ip)
        {
            string adbCommand = $"-s {serialNumber} shell ip -f inet addr show wlan0";

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
            string adbCcommand = $"-s {ip} shell getprop ro.serialno";
            var output = ExecuteADBCommand(adbCcommand);
            return output;
        }

        public static string ExecuteADBCommand(string command)
        {
            string output;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"{adbPath} {command}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError= true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit(new TimeSpan(1000));

                output = process.StandardOutput.ReadToEnd().Trim();
                
                if(process.ExitCode != 0){
                    string error = process.StandardError.ReadToEnd().Trim();
                    File.AppendAllText(@"C:\Users\lydin.camilleri\Desktop\Master's Code Repo\TEST.txt", "\n\n Command: " + command + "; Output: " + output + "; Error: " + error);
                    throw new Exception($"Error in Executing ADB Command: {error}");
                }
            }
         
            return output;
        }
    }
}
