using System;
using System.Collections.Generic;
using System.Text;

namespace TISensorTag
{
    public class HumidityService : GattNotifyService
    {
        public HumidityService() : base (SensorTagIds.UUID_HUM_SERV, SensorTagIds.UUID_HUM_DATA, SensorTagIds.UUID_HUM_CONF)
        {

        }

        public event Action<double> ValueChanged;

        protected override void OnValueChange(byte[] data)
        {
            ValueChanged(CalculateHumidityInPercent(data));
        }

        /// <summary>
        /// Calculates the humidity in percent.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <returns></returns>
        private static double CalculateHumidityInPercent(byte[] sensorData)
        {
            // more info http://www.sensirion.com/nc/en/products/humidity-temperature/download-center/?cid=880&did=102&sechash=c204b9cc
            int hum = BitConverter.ToUInt16(sensorData, 2);

            //cut first two statusbits
            hum = hum - (hum % 4);

            // calculate in percent
            return (-6f) + 125f * (hum / 65535f);
        }
    }
}
