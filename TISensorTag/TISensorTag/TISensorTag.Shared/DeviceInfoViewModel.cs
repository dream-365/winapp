using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace TISensorTag
{
    public class DeviceInfoViewModel : INotifyPropertyChanged
    {
        private string _manufacturerName;
        private string _firmwareRevision;
        private string _temperature;
        private string _accelerometer;
        private string _humidity;


        public string ManufacturerName
        {
            get
            {
                return this._manufacturerName;
            }

            set
            {
                if (value != this._manufacturerName)
                {
                    this._manufacturerName = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public string FirmwareRevision
        {
            get
            {
                return this._firmwareRevision;
            }

            set
            {
                if (value != this._firmwareRevision)
                {
                    this._firmwareRevision = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public string Temperature
        {
            get
            {
                return this._temperature;
            }

            set
            {
                if (value != this._temperature)
                {
                    this._temperature = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public string Accelerometer
        {
            get
            {
                return this._accelerometer;
            }

            set
            {
                if (value != this._accelerometer)
                {
                    this._accelerometer = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public string Humidity
        {
            get {
                return this._humidity;
            }

            set
            {
                if (value != this._humidity)
                {
                    this._humidity = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
