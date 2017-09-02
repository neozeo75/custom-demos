using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace HttpExample
{
    /*
     * Data Models used in API / Device Communication 
     */
    public enum ElementType
    {
        LGT = 0,
        FAN = 1,
        CAM = 2,
        TMP = 3,
        HMD = 4,
        ALL = 5
    }
    public enum ElementState
    {
        OFF = 0,
        ON = 1,
        VARIANT = 2,
    }

    public class DeviceElement
    {
        public ElementType ElementType { get; set; }
        public ElementState ElementState { get; set; }
        public string Value { get; set; }
    }
    public class DeviceElementStatus
    {
        public string DeviceId { get; set; }
        public DeviceElement[] DeviceElements { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    class Program
    {
        static HttpClient httpclient;
        static void Main(string[] args)
        {
            httpclient = new HttpClient();
            httpclient.BaseAddress = new Uri("http://homeautomationplatform.azurewebsites.net/");
            var deviceElementStatus = new DeviceElementStatus();
            deviceElementStatus.DeviceId = "gbb-kickoff-demo";
            deviceElementStatus.DeviceElements = new DeviceElement[]{
                new DeviceElement()
                {
                    ElementType = ElementType.CAM,
                    ElementState = ElementState.OFF,
                    Value = "0.00"
                }
            };

            PostAsync("api/element/setdevices", deviceElementStatus);
            Console.WriteLine("Press Any Key to Close...");
            Console.ReadKey();
        }
        static void PostAsync(string requestUri, DeviceElementStatus control)
        {
            var content = JsonConvert.SerializeObject(control);
            var response = httpclient
                .PostAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"))
                .ContinueWith(responseTask =>
                {
                    var result = responseTask.Result;
                    var json = result.Content.ReadAsStringAsync();
                    Console.WriteLine(json.Result);
                });
            response.Wait();
        }
    }
}