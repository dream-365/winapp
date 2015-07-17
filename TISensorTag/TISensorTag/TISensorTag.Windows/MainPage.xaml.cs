using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TISensorTag
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public DeviceInfoViewModel DeviceInfo { get; set; }

        private CoreDispatcher _dispatcher { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            DeviceInfo = new DeviceInfoViewModel();

            deviceInfoPanel.DataContext = DeviceInfo;
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            connectButton.IsEnabled = false;

            var servcie = new GattDeviceInfoService();

            await servcie.InitAsync();

            var data = await servcie.ReadDeviceInfoAsync();

            DeviceInfo.ManufacturerName = data.ManufacturerName;

            DeviceInfo.FirmwareRevision = data.FirmwareRevision;

            deviceInfoPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;

            RunAccelerometerService();

            RunIRTemperatureService();

            RunHumidityService();
        }

        private async void RunHumidityService()
        {
            var service = new HumidityService();

            await service.InitAsync();

            await service.EnableSensorAsync();

            await service.EnableNotificationAsync();

            service.ValueChanged += service_HumidityChanged;
        }

        private async void RunAccelerometerService()
        {
            var service = new AccelerometerService();

            await service.InitAsync();

            await service.EnableSensorAsync();

            await service.EnableNotificationAsync();

            service.AxisValueChanged += service_AxisValueChanged;
        }

        private async void RunIRTemperatureService()
        {
            var service = new IRTemperatureService();

            await service.InitAsync();

            await service.EnableSensorAsync();

            await service.EnableNotificationAsync();

            service.TemperatureChanged += service_TemperatureChanged;
        }

        public void service_HumidityChanged(double percentage)
        {
            UpdateData(() =>
            {
                DeviceInfo.Humidity = String.Format("{0:00.00}%", percentage);
            });
        }

        public void service_TemperatureChanged(double ambient, double target)
        {
            UpdateData(() =>
            {
                DeviceInfo.Temperature = String.Format("{0:00.00} ℃", ambient);
            });
        }

        public void service_AxisValueChanged(sbyte x, sbyte y, sbyte z)
        {
            UpdateData(() =>
            {
                DeviceInfo.Accelerometer = String.Format("[X: {0}, Y: {1}, Z: {2}]", x, y, z);
            });
        }

        #region UI Helper
        private void UpdateData(DispatchedHandler updateHandler)
        {
            _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, updateHandler).AsTask().Wait();
        }
        #endregion
    }
}
