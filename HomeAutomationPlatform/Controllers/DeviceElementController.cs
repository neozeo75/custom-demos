using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using HomeAutomationPlatform.Model;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;

namespace HomeAutomationPlatform.Controllers
{
    [Produces("application/json")]
    [Route("api/Element")]
    public class DeviceElementController : Controller
    {
        private ServiceClientHelper _serviceClientHelper;

        public DeviceElementController()
        {
            try
            {
                _serviceClientHelper = new ServiceClientHelper("HostName=azureiotworkshophub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=djCNmksyeHvG9MT+ln67Y4e7ghZrGU3iHVLT6SG2ZXQ=");
            }
            catch (Exception execption)
            {
                throw new Exception($"An error occurred while connecting to IoT Hub (inner message: {execption.Message}");
            }
        }

        [HttpGet]
        [Route("GetDeviceStatus")]
        public async Task<object> GetDeviceElementStatusAsync(string deviceId, ElementType elementType)
        {
            var result = new CloudToDeviceMethodResult() { Status = 200 };
            try
            {
                await _serviceClientHelper.OpenConnectionAsync();
                result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                    deviceId,
                    GetMethodName(elementType),
                    null);
                await _serviceClientHelper.CloseConnectionAsync();
            } 
            catch
            {
                Debug.WriteLine("An error occurred while getting the device element status.");
            }
            return result;
        }
        
        [HttpPost]
        [Route("SetDevices")]
        public async Task<object> SetDevicesElementAsync([FromBody] DeviceElementStatus status)
        {
            var result = new CloudToDeviceMethodResult() { Status = 200 };
            try
            {
                await _serviceClientHelper.OpenConnectionAsync();
                result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                    status.DeviceId,
                    GetMethodName(status.DeviceElements[0].ElementType),
                    status.ToString());
                await _serviceClientHelper.CloseConnectionAsync();
            }
            catch
            {
                Debug.WriteLine("An error occurred while getting the device element status.");
            }
            return result;
        }

        private string GetMethodName(ElementType type)
        {
            var directMethodName = "default";
            switch (type)
            {
                case ElementType.LGT: directMethodName = "SwitchLight"; break;
                case ElementType.FAN: directMethodName = "SwitchFan"; break;
                case ElementType.CAM: directMethodName = "SwitchCamera"; break;
                case ElementType.TMP: directMethodName = "ReadTemperature"; break;
                case ElementType.HMD: directMethodName = "ReadHumidity"; break;
                case ElementType.ALL: directMethodName = "DeviceStatus"; break;
            }
            return directMethodName;
        }

    }
}