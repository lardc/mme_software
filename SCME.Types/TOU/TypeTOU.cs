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
        public ushort CurrentAmplitude
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
            CurrentAmplitude = (ushort)CurrentAmplitudeMin;
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

            if (CurrentAmplitude != tOUOldParameters.CurrentAmplitude)
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
        LinkCell1 = 601,
        [EnumMember]
        LinkCell2 = 602,
        [EnumMember]
        LinkCell3 = 603,
        [EnumMember]
        LinkCell4 = 604,
        [EnumMember]
        LinkCell5 = 605,
        [EnumMember]
        LinkCell6 = 606,
        [EnumMember]
        NotReadyCell1 = 611,
        [EnumMember]
        NotReadyCell2 = 612,
        [EnumMember]
        NotReadyCell3 = 613,
        [EnumMember]
        NotReadyCell4 = 614,
        [EnumMember]
        NotReadyCell5 = 615,
        [EnumMember]
        NotReadyCell6 = 616
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
        WatchdogReset = 1001
    };


}
