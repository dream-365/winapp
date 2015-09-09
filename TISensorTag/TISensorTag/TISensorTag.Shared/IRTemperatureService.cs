using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Linq;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

namespace TISensorTag
{
    public enum TemperatureScale
    {
        Celsius,
        Farenheit
    }

    public delegate void TemperatureChangedHandler(double ambient, double target);

    public class IRTemperatureService : GattNotifyService
    {
        public event TemperatureChangedHandler TemperatureChanged;

        public IRTemperatureService (): base (SensorTagIds.UUID_IRT_SERV, SensorTagIds.UUID_IRT_DATA,  SensorTagIds.UUID_IRT_CONF)
        {

        }

        public async Task<double> ReadValueAsync()
        {
            var characteristic = _service.GetCharacteristics(new Guid(SensorTagIds.UUID_IRT_DATA)).FirstOrDefault();

            var result = await characteristic.ReadValueAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached);

            if (result.Status == GattCommunicationStatus.Unreachable)
            {
                throw new Exception("serivce is unreachable");
            }

            var data = result.Value.ToArray();

            Debug.WriteLine("data: {0}-{1}-{2}-{3}", data[0], data[1], data[2], data[3]);

            return CalculateAmbientTemperature(result.Value.ToArray(), TemperatureScale.Celsius);
        }


        protected override void OnValueChange(byte[] data)
        {
            var ambientTemperature = CalculateAmbientTemperature(data, TemperatureScale.Celsius);

            var targetTemperature = CalculateTargetTemperature(data, TemperatureScale.Celsius);

            TemperatureChanged(ambientTemperature, targetTemperature);
        }

        #region calculation
        /// <summary>
        /// Calculates the target temperature.
        /// </summary>
        /// <param name="sensorData">Complete array of data retrieved from the sensor</param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static double CalculateTargetTemperature(byte[] sensorData, TemperatureScale scale)
        {
            if (scale == TemperatureScale.Celsius)
                return CalculateTargetTemperature(sensorData, (BitConverter.ToUInt16(sensorData, 2) / 128.0));
            else
                return CalculateTargetTemperature(sensorData, (BitConverter.ToUInt16(sensorData, 2) / 128.0)) * 1.8 + 32;
        }

        /// <summary>
        /// Calculates the target temperature of the sensor.
        /// More info about the calculation: http://www.ti.com/lit/ug/sbou107/sbou107.pdf
        /// </summary>
        /// <param name="sensorData"></param>
        /// <param name="ambientTemperature"></param>
        /// <returns></returns>
        private static double CalculateTargetTemperature(byte[] sensorData, double ambientTemperature)
        {
            double Vobj2 = BitConverter.ToInt16(sensorData, 0);

            Vobj2 *= 0.00000015625;

            double Tdie = ambientTemperature + 273.15;

            double S0 = 5.593E-14;
            double a1 = 1.75E-3;
            double a2 = -1.678E-5;
            double b0 = -2.94E-5;
            double b1 = -5.7E-7;
            double b2 = 4.63E-9;
            double c2 = 13.4;
            double Tref = 298.15;
            double S = S0 * (1 + a1 * (Tdie - Tref) + a2 * Math.Pow((Tdie - Tref), 2));
            double Vos = b0 + b1 * (Tdie - Tref) + b2 * Math.Pow((Tdie - Tref), 2);
            double fObj = (Vobj2 - Vos) + c2 * Math.Pow((Vobj2 - Vos), 2);
            double tObj = Math.Pow(Math.Pow(Tdie, 4) + (fObj / S), .25);

            return tObj - 273.15;
        }

        private static double CalculateAmbientTemperature(byte[] sensorData, TemperatureScale scale)
        {
            if (scale == TemperatureScale.Celsius)
                return BitConverter.ToUInt16(sensorData, 2) / 128.0;
            else
                return (BitConverter.ToUInt16(sensorData, 2) / 128.0) * 1.8 + 32;
        }
        #endregion
    }
}
