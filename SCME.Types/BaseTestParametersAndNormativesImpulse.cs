using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types
{
    public class BaseTestParametersAndNormativesImpulse : BaseTestParametersAndNormatives
    {
        public int NumberPosition { get; set; } = 1;
        public override bool IsHasChanges(BaseTestParametersAndNormatives oldParametersBase)
        {
            throw new NotImplementedException();
        }
    }
}
