using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using System.Diagnostics;

namespace HomeAutomationPlatform
{
    public class ServiceClientHelper
    {
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
            var result = new CloudToDeviceMethodResult()
            {
                Status = 200
            };

            methodInvokation.SetPayloadJson(payload);
            try
            {
                result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvokation);
            }
            catch
            {
                Debug.WriteLine($"An error occured while invoking device method on: {deviceId}");
            }
            return await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvokation);
        }

        public async Task CloseConnectionAsync()
        {
            if (_isConnected)
            {
                await _serviceClient.CloseAsync();
            }
        }

        private ServiceClient _serviceClient;
        private bool _isConnected = false;
    }
}
