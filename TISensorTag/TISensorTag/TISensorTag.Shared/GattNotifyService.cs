using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Linq;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TISensorTag
{
    public abstract class GattNotifyService : GattService
    {
        private Guid _dataUuid;
        private Guid _confUuid;

        public GattNotifyService(string servcieUuid, string dataUuid, string confUuid) : base (servcieUuid)
        {
            _dataUuid = Guid.Parse(dataUuid);
            _confUuid = Guid.Parse(confUuid);
        }

        public async Task EnableNotificationAsync()
        {
            var dataCharacteristic = _service.GetCharacteristics(_dataUuid).FirstOrDefault();

            dataCharacteristic.ValueChanged += dataCharacteristic_ValueChanged;

            GattCommunicationStatus status =
                    await dataCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);

            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new Exception("Service is not reachable");
            }
        }

        public virtual async Task EnableSensorAsync()
        {
            GattCharacteristic configCharacteristic = _service.GetCharacteristics(_confUuid).FirstOrDefault();

            GattCommunicationStatus status = await configCharacteristic.WriteValueAsync((new byte[] { 1 }).AsBuffer());

            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new Exception("Service is not reachable");
            }
        }

        public async Task DisableNotificationAsync()
        {
            throw new NotImplementedException();
        }

        private void dataCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            var data = new byte[args.CharacteristicValue.Length];

            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(data);

            OnValueChange(data);
        }

        protected abstract void OnValueChange(byte[] data);
    }
}
