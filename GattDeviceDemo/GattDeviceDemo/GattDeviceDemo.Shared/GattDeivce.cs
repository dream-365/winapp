using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace GattDeviceDemo
{
    public class GattDeivce
    {
        private static Guid DEVICEINFO_SERVICE_ID = Guid.Parse("0000180A-0000-1000-8000-00805f9b34fb");
        private static Guid BATTERY_SERVICE_ID = Guid.Parse("0000180F-0000-1000-8000-00805f9b34fb");
        private static Guid MANUFACTURER_CHARACTERISTIC_ID = Guid.Parse("00002A29-0000-1000-8000-00805f9b34fb");
        private static Guid DEVICENAME_CHARACTERISTIC_ID = Guid.Parse("00002A00-0000-1000-8000-00805f9b34fb");
        private static Guid BATTERY_LEVEL_CHARACTERISTIC_ID = Guid.Parse("00002A19-0000-1000-8000-00805f9b34fb");

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
                GattDeviceService.GetDeviceSelectorFromUuid(BATTERY_SERVICE_ID),
                new string[] { "System.Devices.ContainerId" });

            var defaultDevice = devices.FirstOrDefault();

            if (defaultDevice != null)
            {
                var service = await GattDeviceService.FromIdAsync(defaultDevice.Id);

                var Characteristics = service.GetCharacteristics(BATTERY_LEVEL_CHARACTERISTIC_ID);

                GattCharacteristic characteristic = Characteristics.FirstOrDefault();

                if (characteristic != null)
                {
                    characteristic.ProtectionLevel = GattProtectionLevel.EncryptionRequired;

                    characteristic.ValueChanged += characteristic_ValueChanged;

                    var currentDescriptorValue = await characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                    var CHARACTERISTIC_NOTIFY_TYPE = GattClientCharacteristicConfigurationDescriptorValue.Notify;

                    if (currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != CHARACTERISTIC_NOTIFY_TYPE)
                    {
                        // Set the Client Characteristic Configuration Descriptor to enable the device to indicate
                        // when the Characteristic value changes
                        GattCommunicationStatus status =
                            await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            CHARACTERISTIC_NOTIFY_TYPE);
                    }

                    //GattReadResult result = await characteristic.ReadValueAsync();

                    //if (result.Status == GattCommunicationStatus.Success)
                    //{
                    //    var reader = DataReader.FromBuffer(result.Value);

                    //    byte[] bytes = new byte[result.Value.Length];

                    //    reader.ReadBytes(bytes);

                    //    _deviceManufacturer = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    //}
                }
            }
        }

        private void characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {

        }
    }
}
