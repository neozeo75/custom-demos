using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using HomeAutomationPlatform.Model;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace HomeAutomationPlatform.Controllers
{
    [Produces("application/json")]
    [Route("api/Element")]
    public class DeviceElementController : Controller
    {
        private ServiceClientHelper _serviceClientHelper;

        public DeviceElementController()
        {
            _serviceClientHelper = new ServiceClientHelper("HostName=azureiotworkshophub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=djCNmksyeHvG9MT+ln67Y4e7ghZrGU3iHVLT6SG2ZXQ=");
        }

        [HttpGet]
        [Route("GetDeviceStatus")]
        public async Task<object> GetDeviceElementStatusAsync(string deviceId, ElementType elementType)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                deviceId,
                GetMethodName(elementType), 
                null);
            await _serviceClientHelper.CloseConnectionAsync();
            return result;
        }
        
        [HttpPost]
        [Route("SetDevices")]
        public async Task<object> SetDevicesElementAsync([FromBody] DeviceElementStatus status)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                status.DeviceId,
                GetMethodName(status.DeviceElements[0].ElementType),
                status.ToString());
            await _serviceClientHelper.CloseConnectionAsync();
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