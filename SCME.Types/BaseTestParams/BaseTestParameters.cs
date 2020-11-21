using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Xml.Serialization;

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
        InputOptions = 101,

        [EnumMember]
        OutputLeakageCurrent = 102,

        [EnumMember]
        OutputResidualVoltage = 103,

        [EnumMember]
        ProhibitionVoltage = 104
    }

    [KnownType(typeof(ATU.TestParameters))]
    [KnownType(typeof(BVT.TestParameters))]
    [KnownType(typeof(dVdt.TestParameters))]
    [KnownType(typeof(Gate.TestParameters))]
    [KnownType(typeof(QrrTq.TestParameters))]
    [KnownType(typeof(TOU.TestParameters))]
    [KnownType(typeof(VTM.TestParameters))]
    [KnownType(typeof(InputOptions.TestParameters))]
    [KnownType(typeof(OutputLeakageCurrent.TestParameters))]
    [KnownType(typeof(OutputResidualVoltage.TestParameters))]
    [KnownType(typeof(ProhibitionVoltage.TestParameters))]

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public abstract class BaseTestParametersAndNormatives: INotifyPropertyChanged
    {
        public bool ShowAuxiliaryVoltagePowerSupply1 => DutPackageType == DutPackageType.B5 || DutPackageType == DutPackageType.V108;

        public bool ShowAuxiliaryVoltagePowerSupply2 => DutPackageType == DutPackageType.V108;


        public virtual bool ShowAuxiliarySupplyCurrent1 => (DutPackageType == DutPackageType.B5 || DutPackageType == DutPackageType.V108) && GetType() == typeof(InputOptionsParameters);
        public virtual bool ShowAuxiliarySupplyCurrent2 =>  DutPackageType == DutPackageType.V108 && GetType() == typeof(InputOptionsParameters);


        public bool IsProfileStyle { get; set; } = true;

        [DataMember]
        public int NumberPosition { get; set; } = 1;

        
        [DataMember]
        public DutPackageType DutPackageType { get; set; }  

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

        public event PropertyChangedEventHandler PropertyChanged;

        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public abstract class BaseTestResults
    {
        [DataMember]
        public long TestTypeId { get; set; }
    }
}
