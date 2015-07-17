using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Linq;

namespace TISensorTag
{
    public class GattService
    {
        protected GattDeviceService _service;

        protected Guid _servcieUuid;

        public GattService(string uuid)
        {
            _servcieUuid = Guid.Parse(uuid);
        }

        public async Task InitAsync()
        {
            var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(_servcieUuid), new string[] { "System.Devices.ContainerId" });

            var defaultDevice = devices.FirstOrDefault();

            if (defaultDevice == null)
            {
                throw new Exception("Device not found");
            }

            _service = await GattDeviceService.FromIdAsync(defaultDevice.Id);
        }
    }
}
