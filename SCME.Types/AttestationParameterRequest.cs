using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types
{
    [DataContract]
    public class AttestationParameterRequest
    {
        public AttestationParameterRequest(int parameter, uint current, uint voltage, int numberPosition, AttestationType attestationType)
        {
            AttestationType = attestationType;
            Parameter = parameter;
            Current = current;
            Voltage = voltage;
            NumberPosition = numberPosition;
        }
        [DataMember]
        public AttestationType AttestationType { get; set; }
        [DataMember]
        public int Parameter { get; set; }
        [DataMember]
        public uint Current { get; set; }
        [DataMember]
        public uint Voltage { get; set; }
        [DataMember]
        public int NumberPosition { get; set; } = 1;

        
    }
}
