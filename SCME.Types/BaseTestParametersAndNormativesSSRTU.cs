using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types
{
    [KnownType(typeof(InputOptions.TestParameters))]
    [KnownType(typeof(OutputLeakageCurrent.TestParameters))]
    [KnownType(typeof(OutputResidualVoltage.TestParameters))]
    [KnownType(typeof(ProhibitionVoltage.TestParameters))]
    [KnownType(typeof(AuxiliaryPower.TestParameters))]
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class BaseTestParametersAndNormativesSSRTU : BaseTestParametersAndNormatives
    {
        [DataMember]
        public int NumberPosition { get; set; } = 1;
        public override bool IsHasChanges(BaseTestParametersAndNormatives oldParametersBase)
        {
            throw new NotImplementedException();
        }
    }
}
