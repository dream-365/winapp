using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace TISensorTag
{
    public delegate void OnAxisValueChangedHandler(sbyte x, sbyte y, sbyte z);


    public class AccelerometerService : GattNotifyService
    {
        public event OnAxisValueChangedHandler AxisValueChanged;

        public AccelerometerService()
            : base(SensorTagIds.UUID_ACC_SERV, SensorTagIds.UUID_ACC_DATA, SensorTagIds.UUID_ACC_CONF)
        {

        }

        // 1: 2G, 2: 4G, 3: 8G, 0: disable
        public override async Task EnableSensorAsync()
        {
            GattCharacteristic configCharacteristic = _service.GetCharacteristics(Guid.Parse(SensorTagIds.UUID_ACC_CONF)).FirstOrDefault();

            GattCommunicationStatus status = await configCharacteristic.WriteValueAsync((new byte[] { 1 }).AsBuffer());

            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new Exception("Service is not reachable");
            }
        }

        protected override void OnValueChange(byte[] data)
        {
            AxisValueChanged(ConvertToSbyte(data[0]), ConvertToSbyte(data[1]), ConvertToSbyte(data[2]));
        }


        private static sbyte ConvertToSbyte(byte value)
        {
            byte high = 128;

            byte low = 127;

            sbyte temp = (sbyte)(((byte)(high & value) == high) ? -1 : 1);

            sbyte result = (sbyte)((sbyte)(low & value) * temp);

            return result;
        }


        /// <summary>
        /// Sets the period the sensor reads data. Default is 1s. Lower limit is 100ms, the default value is 1000ms
        /// </summary>
        /// <param name="time">Period in 10 ms.</param>
        /// <exception cref="DeviceUnreachableException">Thrown if it wasn't possible to communicate with the device.</exception>
        /// <exception cref="DeviceNotInitializedException">Thrown if sensor has not been initialized successfully.</exception>
        public async Task SetReadPeriod(byte time)
        {
            if (time < 10)
                throw new ArgumentOutOfRangeException("time", "Period can't be lower than 100ms");

            GattCharacteristic dataCharacteristic = _service.GetCharacteristics(new Guid(SensorTagIds.UUID_ACC_PERI)).FirstOrDefault();

            byte[] data = new byte[] { time };

            GattCommunicationStatus status = await dataCharacteristic.WriteValueAsync(data.AsBuffer());

            if (status == GattCommunicationStatus.Unreachable)
            {
                throw new Exception("Service is not reachable");
            }
        }
    }
}
