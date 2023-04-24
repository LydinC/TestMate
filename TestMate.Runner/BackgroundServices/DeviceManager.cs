using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TestMate.Common.Models.Devices;

namespace TestMate.Runner.BackgroundServices
{
    public class DeviceManager
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        //Declared as Lazy to initialise singleton instance only when it is first used and be thread-safe
        private static readonly Lazy<DeviceManager> _instance = new Lazy<DeviceManager>(() => new DeviceManager());
        public static DeviceManager Instance => _instance.Value;

        public DeviceManager()
        { 
        }

        public bool LockDevice(Device device)
        {
            lock (_semaphores)
            {
                if (!_semaphores.TryGetValue(device.SerialNumber, out SemaphoreSlim semaphore))
                {
                    semaphore = new SemaphoreSlim(1); // only 1 thread can use the device at one point in time
                    _semaphores.TryAdd(device.SerialNumber, semaphore);
                }
                // returns true if semaphore is available, otherwise false
                return semaphore.Wait(0);
            }
        }


        public bool ReleaseDevice(Device device) {
            lock (_semaphores)
            {
                if (_semaphores.TryGetValue(device.SerialNumber, out SemaphoreSlim semaphore))
                {
                    semaphore.Release();
                    if (semaphore.CurrentCount == 1) // if no other threads are waiting on the semaphore
                    {
                        //Removing from semaphore dictionary to prevent it from growing indefinitely and consuming too much memory
                        _semaphores.TryRemove(device.SerialNumber, out SemaphoreSlim removedSemaphore);
                    }
                    return true;
                } else
                {
                    throw new Exception("Should never reach a case where releasing a device lock, which isn't released successfully");//Nothing to actually release? 
                }

            }
        }
    }
}
