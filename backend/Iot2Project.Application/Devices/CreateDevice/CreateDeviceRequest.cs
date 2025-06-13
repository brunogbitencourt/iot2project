using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Devices.CreateDevice
{
    public record class CreateDeviceRequest
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }    
        public string Unit { get; set; }
        public string ConnectedPort { get; set; }
        public string KafkaTopic { get; set; }
        public string MqttTopic { get; set; }
        public int UserId { get; set; }


    }
}
