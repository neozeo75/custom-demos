using Newtonsoft.Json;

namespace HomeAutomationDevice.Models
{
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
        VARIANT = 3,
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
}
