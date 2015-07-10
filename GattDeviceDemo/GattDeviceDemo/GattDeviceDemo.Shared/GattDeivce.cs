using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace GattDeviceDemo
{
    public class GattDeivce
    {
        private static Guid DEVICEINFO_SERVICE_ID = Guid.Parse("0000180A-0000-1000-8000-00805f9b34fb");
        private static Guid MANUFACTURER_CHARACTERISTIC_ID = Guid.Parse("00002A29-0000-1000-8000-00805f9b34fb");

        private string _deviceManufacturer;

        public String DeviceManufacturer
        {
            get
            {
                return _deviceManufacturer;
            }
        }

        public async Task ConnectToServcie()
        {
            var devices = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(DEVICEINFO_SERVICE_ID),
                new string[] { "System.Devices.ContainerId" });

            var defaultDevice = devices.FirstOrDefault();

            if (defaultDevice != null)
            {
                var service = await GattDeviceService.FromIdAsync(defaultDevice.Id);

                var manufacturerName = service.GetCharacteristics(MANUFACTURER_CHARACTERISTIC_ID);

                GattCharacteristic characteristic = manufacturerName.FirstOrDefault();

                if (characteristic != null)
                {
                    GattReadResult result = await characteristic.ReadValueAsync();

                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        var bytes = result.Value.ToArray();

                        _deviceManufacturer = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    }
                }
            }
        }
    }
}
