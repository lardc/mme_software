using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using GateTestParameters = SCME.Types.Gate.TestParameters;
using BvtTestParameters = SCME.Types.BVT.TestParameters;
using VtmTestParameters = SCME.Types.VTM.TestParameters;
using DvDtParameters = SCME.Types.dVdt.TestParameters;
using AtuParameters = SCME.Types.ATU.TestParameters;
using QrrTqParameters = SCME.Types.QrrTq.TestParameters;
using RacParameters = SCME.Types.RAC.TestParameters;
using TOUParameters = SCME.Types.TOU.TestParameters;

namespace SCME.Types.BaseTestParams
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum TestParametersType
    {
        [EnumMember]
        Gate = 1,

        [EnumMember]
        StaticLoses = 2,

        [EnumMember]
        Bvt = 3,

        [EnumMember]
        Commutation = 4,

        [EnumMember]
        Clamping = 5,

        [EnumMember]
        Dvdt = 6,

        [EnumMember]
        Sctu = 7,

        [EnumMember]
        ATU = 8,

        [EnumMember]
        QrrTq = 9,

        [EnumMember]
        RAC = 10,

        [EnumMember]
        IH = 11,

        [EnumMember]
        RCC = 12,

        [EnumMember]
        TOU = 13
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public abstract class BaseTestParametersAndNormatives
    {
        [DataMember]
        public TestParametersType TestParametersType { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public long TestTypeId { get; set; }

        public abstract bool IsHasChanges(BaseTestParametersAndNormatives oldParametersBase);

        public static BaseTestParametersAndNormatives CreateParametersByType(TestParametersType type)
        {
            switch (type)
            {
                case TestParametersType.Gate:
                    return new GateTestParameters() { IsEnabled = false };
                case TestParametersType.Bvt:
                    return new BvtTestParameters() { IsEnabled = false };
                case TestParametersType.StaticLoses:
                    return new VtmTestParameters() { IsEnabled = false };
                case TestParametersType.Dvdt:
                    return new DvDtParameters() { IsEnabled = true };
                case TestParametersType.ATU:
                    return new AtuParameters() { IsEnabled = true };
                case TestParametersType.QrrTq:
                    return new QrrTqParameters() { IsEnabled = true };
                case TestParametersType.RAC:
                    return new RacParameters() { IsEnabled = true };
                case TestParametersType.TOU:
                    return new TOUParameters() { IsEnabled = true };
                default:
                    throw new NotImplementedException("CreateParametersByType");
            }
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public abstract class BaseTestResults
    {
        [DataMember]
        public long TestTypeId { get; set; }
    }
}
