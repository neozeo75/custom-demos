using System;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using HomeAutomationDevice.Models;
using Windows.Devices.Enumeration;
using HomeAutomationDevice.Helpers;
using System.Diagnostics;

namespace HomeAutomationDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DeviceClient _deviceClient;
        private string _deviceId = "gbb-kickoff-demo";
        private bool _lightStatus = false;
        private bool _fanStatus = false;
        private string _cameraStatus;
        private double _lightDimValue;
        private double _fanSpeedValue;
        private Random _random = new Random();
        private SolidColorBrush _yellow = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private SolidColorBrush _red = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush _grey = new SolidColorBrush(Windows.UI.Colors.LightGray);
        private DispatcherTimer _timer = new DispatcherTimer();
        private double _temperature = 0.00;
        private double _humidity = 0.00;
        private CameraHelper _cameraHelper;
        private BlobHelper _blobHepler = new BlobHelper();
        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeAppSettingsAsync();
        }

        public async void InitializeAppSettingsAsync()
        {
            _cameraHelper = new CameraHelper(_captureElement);
            _deviceClient = DeviceClient.CreateFromConnectionString("HostName=azureiotworkshophub.azure-devices.net;DeviceId=gbb-kickoff-demo;SharedAccessKey=5KcIgKhUwK5N0owlzJVjnJvAVTRqlH3V/TQ8E1C6DDY=", TransportType.Mqtt);
            _deviceClient.SetConnectionStatusChangesHandler(OnConnectionStatusChanged);

            await _deviceClient.SetMethodHandlerAsync("SwitchLight", OnLightSwitched, null);
            await _deviceClient.SetMethodHandlerAsync("SwitchFan", OnFanSwitched, null);
            await _deviceClient.SetMethodHandlerAsync("SwitchCamera", OnCameraSwitched, null);
            await _deviceClient.SetMethodHandlerAsync("ReadTemperature", OnTemperatureRead, null);
            await _deviceClient.SetMethodHandlerAsync("ReadHumidity", OnHumidityRead, null);
            await _deviceClient.SetMethodHandlerAsync("DeviceStatus", OnDeviceStatus, null);
            await _deviceClient.OpenAsync();
            await _cameraHelper.StartCameraPreviewAsync();

            _timer.Interval = new TimeSpan(0, 0, 5);
            _timer.Tick += _timer_Tick;
            _timer.Start();
            Light.Fill = _grey;
            Fan.Fill = _grey;
        }

        private async Task<DeviceInformation> GetCameraId(Windows.Devices.Enumeration.Panel desired)
        {
            var deviceId = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desired);
            if (deviceId != null) return deviceId;
            else throw new Exception($"camera type of {desired} does not exist.");
        }

        private void _timer_Tick(object sender, object e)
        {
            _temperature = (25 + _random.NextDouble() * 23 - 2);
            _humidity    = (25 + _random.NextDouble() * 23 - 2);
            Temperature.Text = string.Format("{0:0.00}", Math.Truncate(_temperature * 10) / 10);
            Humidity.Text = string.Format("{0:0.00}", Math.Truncate(_humidity * 10) / 10);
        }

        public void OnConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            
        }

        public Task<MethodResponse> OnDeviceStatus(MethodRequest request, object context)
        {
            DeviceElementStatus status = new DeviceElementStatus
            {
                DeviceId = _deviceId,
                DeviceElements = new  DeviceElement []{
                 // NOT IMPLEMENTED
                }
            };
            var response = new MethodResponse(Encoding.UTF8.GetBytes(status.ToString()), 500);
            return Task.FromResult(response);
        }

        public async Task<MethodResponse> OnLightSwitched(MethodRequest request, object context)
        {
            var command = JsonConvert.DeserializeObject<DeviceElementStatus>(request.DataAsJson);
            if (command.DeviceElements[0].ElementState == ElementState.ON)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Light.Fill = _yellow; });
                _lightStatus = true;
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Light.Fill = _grey; });
                _lightStatus = false;
            }
            var response = new MethodResponse(Encoding.UTF8.GetBytes(command.ToString()), 500);
            return response;
        }
        

        public async Task<MethodResponse> OnFanSwitched(MethodRequest request, object context)
        {
            var command = JsonConvert.DeserializeObject<DeviceElementStatus>(request.DataAsJson);
            if (command.DeviceElements[0].ElementState == ElementState.ON)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Fan.Fill = _yellow; });
                _fanStatus = true;
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { Fan.Fill = _grey; });
                _fanStatus = false;
            }
            var response = new MethodResponse(Encoding.UTF8.GetBytes(command.ToString()), 500);
            return response;
        }

        public async Task<MethodResponse> OnCameraSwitched(MethodRequest request, object context)
        {
            var command = JsonConvert.DeserializeObject<DeviceElementStatus>(request.DataAsJson);

            int status = 500;
            try
            {
                var path = await _cameraHelper.TakePhotoAsync();
                var bloblocation = await _blobHepler.SaveImageToBlobAsync(path);
                command.DeviceElements[0].Value = bloblocation;
            }
            catch
            {
                command.DeviceElements[0].Value = "http://localhost";
                status = 200;
            }

            var response = new MethodResponse(Encoding.UTF8.GetBytes(command.ToString()), status);
            Debug.WriteLine(JsonConvert.SerializeObject(response));
            return response;
        }

        public Task<MethodResponse> OnTemperatureRead(MethodRequest request, object context)
        {
            var command = JsonConvert.DeserializeObject<DeviceElementStatus>(request.DataAsJson);
            command.DeviceElements[0].Value = _temperature.ToString();
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command)), 500));
        }

        public Task<MethodResponse> OnHumidityRead(MethodRequest request, object context)
        {
            var command = JsonConvert.DeserializeObject<DeviceElementStatus>(request.DataAsJson);
            command.DeviceElements[0].Value = _humidity.ToString();
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(command)), 500));
        }
    }
}
