using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types
{
    [DataContract]
    public class AttestationParameterResponse
    {
        public AttestationParameterResponse(double voltage, double current)
        {
            Voltage = voltage;
            Current = current;
        }
        [DataMember]
        public double Voltage { get; set; }
        [DataMember]
        public double Current { get; set; }
    }
}
