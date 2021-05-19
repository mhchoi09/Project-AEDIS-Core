using RaspberryPi.Model;
using RaspberryPi;
using System;
using System.Diagnostics;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Newtonsoft.Json;

namespace RaspberryPi
{
    public sealed partial class MainPage : Page
    {
        private I2cDevice _device;
        private Timer _periodicTimer;
        private string iothubConnectionString = "HostName=pleadisaplha.azure-devices.net;DeviceId=AB01;SharedAccessKey=Typo2T1b1WvVkpKPAQIJjwlsw4DlHKmSbBHEKtktxoY=";
        public MainPage()
        {
            this.InitializeComponent();

            InitI2C();
        }

        private async void InitI2C()
        {
            var settings = new I2cConnectionSettings(0x40); // Arduino address
            settings.BusSpeed = I2cBusSpeed.StandardMode;
            string aqs = I2cDevice.GetDeviceSelector("I2C1");
            var dis = await DeviceInformation.FindAllAsync(aqs);
            _device = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            _periodicTimer = new Timer(TimerCallback, null, 0, 200); // Create a timer
        }

        private async void TimerCallback(object state)
        {
            try
            {
                byte[] ReadBuf = new byte[14];
                _device.Read(ReadBuf);

                var temp = (float)ReadBuf[0];
                var pressure = (float)ReadBuf[1];
                var humidity = (float)ReadBuf[2];
                var visible = (float)ReadBuf[3];
                var irlevel = (int)ReadBuf[4];
                var uvindex = (float)ReadBuf[5];
                var task = Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        txtTemperature.Text = temp.ToString();
                        txtHumidity.Text = humidity.ToString();
                    });
                var telemetrydatapoint = new
                {
                    temperature = temp,
                    humidity = humidity,
                    pressure = pressure,
                    visible = visible,
                    irlevel = irlevel,
                    uvindex = uvindex,
                };
                var messagestring = JsonConvert.SerializeObject(telemetrydatapoint);
                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(iothubConnectionString, TransportType.Http1);
                var msg = new Message(Encoding.UTF8.GetBytes(messagestring));
                await deviceClient.SendEventAsync(msg);
            }
            catch (Exception f)
            {
                Debug.WriteLine(f.Message);
            }
        }

        private void textBlock_Copy6_SelectionChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
