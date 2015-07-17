using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Linq;
using System.Threading.Tasks;

namespace TISensorTag
{
    public class GattDeviceInfoService : GattService
    {
        public GattDeviceInfoService () : base (SensorTagIds.UUID_INF_SERV)
        {

        }

        private async Task<string> ReadCharacteristicStringAsync(string uuid)
        {
            var characteristics = _service.GetCharacteristics(Guid.Parse(uuid));

            var defaultCharacteristic = characteristics.FirstOrDefault();

            if (defaultCharacteristic == null)
            {
                throw new Exception("characteristic " + uuid + " does not exist");
            }

            GattReadResult result = await defaultCharacteristic.ReadValueAsync();

            return (result.Status == GattCommunicationStatus.Success)
                   ? ValueReadHelper.ReadString(result.Value)
                   : string.Empty;
        }

        public async Task<DeviceInfoViewModel> ReadDeviceInfoAsync()
        {
            var model = new DeviceInfoViewModel();

            model.ManufacturerName = await ReadCharacteristicStringAsync(SensorTagIds.UUID_INF_MANUF_NR);

            model.FirmwareRevision = await ReadCharacteristicStringAsync(SensorTagIds.UUID_INF_FW_NR);

            return model;
        }
    }
}
