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
        //[EnumMember]
        //Disabled = 2,
        [EnumMember]
        Charging = 3,
        [EnumMember]
        Ready = 4,
        [EnumMember]
        InProcess = 5
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

        public TestResults() { }

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

        private ushort _CurrentAmplitude;

        [DataMember]
        public ushort CurrentAmplitude
        {
            get
            {
                return _CurrentAmplitude;
            }
            set
            {
                if (value >= CurrentAmplitudeMin && value <= CurrentAmplitudeMax)
                    _CurrentAmplitude = value;
                else
                    throw new Exception("Недопустимое значение амплитуды тока(диапазон 160-1250)");
            }
        }

        public ushort ITM { get; set; }
        public ushort TGD { get; set; }
        public ushort TGT { get; set; }

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

            if (ITM != tOUOldParameters.ITM)
                return true;

            if (TGD != tOUOldParameters.TGD)
                return true;

            if (TGT != tOUOldParameters.TGT)
                return true;

            return false;
        }
    }

    public enum HWProblemReason
    {
        [EnumMember]
        None = 0,
        StateIsNoGood = 1000 //в описании блока не было такого значения, зарезервировал его себе для случая не корректного состояния блока
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWFaultReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        NoControlNoPower = 1, // Отсутствие тока управления и силового тока
        [EnumMember]
        NoPower = 2, // Отсутствие силового тока
        [EnumMember]
        Short = 3, // КЗ на выходе
        [EnumMember]
        NoPotensialSignal = 4,// Нет сигнала с потенциальной линии
        [EnumMember]
        Overflow90 = 5, // Переполнение счётчика уровня 90%
        [EnumMember]
        Overflow10 = 6, // Переполнение счётчика уровня 10%
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
        CurrentOutOfRange = 1 // Измеренное значение тока вне диапазона
    };


}
