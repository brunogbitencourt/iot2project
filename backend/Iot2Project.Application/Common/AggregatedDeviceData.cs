using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Common
{
    public class AggregatedDeviceData
    {
        public DateTime Timestamp { get; init; }   // início do bucket
        public double AvgValue { get; init; }
        public double MinValue { get; init; }
        public double MaxValue { get; init; }
    }
}
