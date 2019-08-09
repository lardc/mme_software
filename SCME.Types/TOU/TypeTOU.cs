using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SCME.Types.TOU
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDeviceState
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Fault = 1,
        [EnumMember]
        Disabled = 2,
        [EnumMember]
        PowerReady = 3,
        [EnumMember]
        InProcess = 4
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestResults : BaseTestResults
    {
        [DataMember]
        public float ITM { get; set; }

        [DataMember]
        public float TGD { get; set; }

        [DataMember]
        public float TGT { get; set; }

        public TestResults(){}

        public override string ToString()
        {
            return $"{ITM} {TGD} {TGT}";
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        public static ushort CurrentAmplitudeMin { get; private set; } = 160;
        public static ushort CurrentAmplitudeMax = 1250;

        private ushort m_CurrentAmplitude;

        [DataMember]
        public ushort ITM
        {
            get
            {
                return m_CurrentAmplitude;
            }
            set
            {
                if (value >= CurrentAmplitudeMin && value <= CurrentAmplitudeMax)
                    m_CurrentAmplitude = value;
                else
                    throw new Exception("Недопустимое значение амплитуды тока(диапазон 160-1250)");
            }
        }

        [DataMember]
        public bool IsEnabled { get; set; }

        public TestParameters()
        {
            IsEnabled = true;
            TestParametersType = TestParametersType.TOU;
            ITM = (ushort)CurrentAmplitudeMin;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool IsHasChanges(BaseTestParametersAndNormatives oldParameters)
        {
            var tOUOldParameters = oldParameters as TestParameters;

            if (tOUOldParameters == null)
                throw new InvalidCastException("oldParameters must be tOUOldParameters");

            if (ITM != tOUOldParameters.ITM)
                return true;

            return false;
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWFaultReason
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        NoBatteryCharge = 1
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDisableReason
    {
        [EnumMember]
        None = 0,
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWWarningReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Unknown = 1
    };


}
