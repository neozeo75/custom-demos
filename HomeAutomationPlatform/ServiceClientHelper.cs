using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using System.Diagnostics;

namespace HomeAutomationPlatform
{
    public class ServiceClientHelper
    {
        private ServiceClient _serviceClient;
        private bool _isConnected = false;

        public ServiceClientHelper(string connectionString)
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        }
        public async Task OpenConnectionAsync()
        {
            try
            {
                await _serviceClient.OpenAsync();
                _isConnected = true;
            }
            catch
            {
                Debug.WriteLine("An error occurred while connection to IoT Hub.");
            }
        }

        public async Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload)
        {
            var methodInvokation = new CloudToDeviceMethod(methodName);
            methodInvokation.SetPayloadJson(payload);
            return await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvokation);
        }

        public async Task CloseConnectionAsync()
        {
            if (_isConnected)
            {
                await _serviceClient.CloseAsync();
            }
        }
    }
}
