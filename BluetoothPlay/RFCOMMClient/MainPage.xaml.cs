using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.Rfcomm;
using System.Diagnostics;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace RFCOMMClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DeviceInformationCollection rfcommServiceInfoCollection;

        private StreamSocket streamSocket;

        private RfcommDeviceService rfcommDeviceService;

        private DataWriter dataWriter;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            rfcommServiceInfoCollection = await DeviceInformation.FindAllAsync(
                RfcommDeviceService.GetDeviceSelector(RfcommServiceId.ObexObjectPush));

            var count = rfcommServiceInfoCollection.Count;

            Debug.WriteLine("Count of RFCOMM Service: " + count);

            if(count > 0)
            {
                lock (this)
                {
                    streamSocket = new StreamSocket();
                }

                var defaultSvcInfo = rfcommServiceInfoCollection.FirstOrDefault();

                rfcommDeviceService = await RfcommDeviceService.FromIdAsync(defaultSvcInfo.Id);

                if(rfcommDeviceService == null)
                {
                    Debug.WriteLine("Rfcomm Device Service is NULL, ID = {0}", defaultSvcInfo.Id);

                    return;
                }

                Debug.WriteLine("ConnectionHostName: {0}, ConnectionServiceName: {1}", rfcommDeviceService.ConnectionHostName, rfcommDeviceService.ConnectionServiceName);

                await streamSocket.ConnectAsync(rfcommDeviceService.ConnectionHostName, rfcommDeviceService.ConnectionServiceName);

                dataWriter = new DataWriter(streamSocket.OutputStream);

                connectButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void SendMessage(string message)
        {
            dataWriter.WriteString(message);

            await dataWriter.FlushAsync();

            await dataWriter.StoreAsync();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage(messageBox.Text);

            messageBox.Text = string.Empty;
        }
    }
}
