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
using TOUParameters = SCME.Types.TOU.TestParameters;
using OutputLeakageCurrentParameters = SCME.Types.OutputLeakageCurrent.TestParameters;
using OutputResidualVoltageParameters = SCME.Types.OutputResidualVoltage.TestParameters;
using InputOptionsParameters = SCME.Types.InputOptions.TestParameters;
using ProhibitionVoltageParameters = SCME.Types.ProhibitionVoltage.TestParameters;


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
        IH = 11,

        [EnumMember]
        RCC = 12,

        [EnumMember]
        TOU = 13,

        [EnumMember]
        OutputLeakageCurrent = 14,

        [EnumMember]
        OutputResidualVoltage = 15,

        [EnumMember]
        InputOptions = 16,

        [EnumMember]
        ProhibitionVoltage = 17
    }

    [KnownType(typeof(ATU.TestParameters))]
    [KnownType(typeof(BVT.TestParameters))]
    [KnownType(typeof(dVdt.TestParameters))]
    [KnownType(typeof(Gate.TestParameters))]
    [KnownType(typeof(QrrTq.TestParameters))]
    [KnownType(typeof(TOU.TestParameters))]
    [KnownType(typeof(VTM.TestParameters))]
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public abstract class BaseTestParametersAndNormatives
    {
        [DataMember]
        public TestParametersType TestParametersType { get; set; }

        [DataMember]
        public int Order { get; set; }
        
        [DataMember] public bool IsEnabled { get; set; }

        [DataMember]
        public long TestTypeId { get; set; }

        public abstract bool IsHasChanges(BaseTestParametersAndNormatives oldParametersBase);

        public static BaseTestParametersAndNormatives CreateParametersByType(TestParametersType type)
        {
            switch (type)
            {
                case TestParametersType.Gate:
                    return new GateTestParameters();
                case TestParametersType.Bvt:
                    return new BvtTestParameters();
                case TestParametersType.StaticLoses:
                    return new VtmTestParameters();
                case TestParametersType.Dvdt:
                    return new DvDtParameters();
                case TestParametersType.ATU:
                    return new AtuParameters();
                case TestParametersType.QrrTq:
                    return new QrrTqParameters();
                case TestParametersType.TOU:
                    return new TOUParameters();
                case TestParametersType.OutputLeakageCurrent:
                    return new OutputLeakageCurrentParameters();
                case TestParametersType.OutputResidualVoltage:
                    return new OutputResidualVoltageParameters();
                case TestParametersType.InputOptions:
                    return new InputOptionsParameters();
                case TestParametersType.ProhibitionVoltage:
                    return new ProhibitionVoltageParameters();
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
