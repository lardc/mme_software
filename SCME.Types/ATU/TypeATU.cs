using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SCME.Types.ATU
{
    /// <summary>Состояние оборудования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDeviceState
    {
        /// <summary>Неопределенное состояние (после включения питания)</summary>
        [EnumMember]
        DS_None = 0,
        /// <summary>Состояние ошибки</summary>
        [EnumMember]
        DS_Fault = 1,
        /// <summary>Выключен</summary>
        [EnumMember]
        DS_Disabled = 2,
        /// <summary>Ожидание заряда батареи</summary>
        [EnumMember]
        DS_BatteryCharge = 3,
        /// <summary>Состояние готовности</summary>
        [EnumMember]
        DS_Ready = 4,
        /// <summary>В процессе работы</summary>
        [EnumMember]
        DS_InProcess = 5,
    };

    /// <summary>Причина ошибки</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWFaultReason
    {
        [EnumMember]
        None = 0,
        /// <summary>Ошибка заряда батареи</summary>
        [EnumMember]
        ChargeError = 1,
        /// <summary>Ошибка оцифровки значений напряжения/тока</summary>
        [EnumMember]
        ADCError = 2
    };

    /// <summary>Причина предупреждения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWWarningReason
    {
        [EnumMember]
        None = 0,
        /// <summary>ХХ на выходе</summary>
        [EnumMember]
        Idle = 1,
        /// <summary>КP на выходе</summary>
        [EnumMember]
        Short = 2,
        /// <summary>Погрешность полученной мощности велика</summary>
        [EnumMember]
        PowerAccuracy = 3,
        /// <summary>Пробой прибора</summary>
        [EnumMember]
        BreakDUT = 4,
        /// <summary>Краевой пробой прибора</summary>
        [EnumMember]
        FacetBreak = 5
    };

    /// <summary>Причина выключения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDisableReason
    {
        [EnumMember]
        None = 0
    }

    /// <summary>Результат выполнения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWOperationResult
    {
        /// <summary>В процессе</summary>
        [EnumMember]
        InProcess = 0,
        /// <summary>Успешно</summary>
        [EnumMember]
        Success = 1,
        /// <summary>Неудачно</summary>
        [EnumMember]
        Fail = 2
    };

    [DataContract(Name = "Atu.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        /// <summary>Инициализирует новый экземпляр класса TestParameters</summary>
        public TestParameters()
        {
            TestParametersType = TestParametersType.ATU;
            PrePulseValue = 100;
            PowerValue = 16;
            IsEnabled = true;
        }

        [DataMember]
        public ushort PrePulseValue
        {
            get; set;
        }

        [DataMember]
        public float PowerValue
        {
            get; set;
        }

        [DataMember]
        public short UBR
        {
            get; set;
        }

        [DataMember]
        public short UPRSM
        {
            get; set;
        }

        [DataMember]
        public float IPRSM
        {
            get; set;
        }

        [DataMember] 
        public float PRSM_Min
        {
            get; set;
        } = 0;

        [DataMember]
        public float PRSM_Max
        {
            get; set;
        } = 70;

        public override bool IsHasChanges(BaseTestParametersAndNormatives oldParameters)
        {
            if (oldParameters == null)
                throw new ArgumentNullException("Метод '" + System.Reflection.MethodBase.GetCurrentMethod().Name + "' получил на вход параметр 'oldParameters' равный Null.");
            if (GetHashCode() == oldParameters.GetHashCode())
                return false;
            string typeName = oldParameters.GetType().Name;
            if (typeName != "TestParameters")
                throw new InvalidCastException("Method '" + System.Reflection.MethodBase.GetCurrentMethod().Name + "' получил на вход параметр 'oldParameters' тип которого '" + typeName + "'. Ожидался тип параметра 'TestParameters'.");
            TestParameters aTUOldParameters = (TestParameters)oldParameters;
            if (PrePulseValue != aTUOldParameters.PrePulseValue)
                return true;
            if (PowerValue != aTUOldParameters.PowerValue)
                return true;
            return false;
        }
        
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>Результаты тестирования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestResults : BaseTestResults
    {
        /// <summary>Инициализирует новый экземпляр класса TestResults</summary>
        public TestResults()
        {
            ArrayVDUT = new List<short>();
            ArrayIDUT = new List<short>();
        }

        [DataMember]
        public short UBR
        {
            get; set;
        }

        [DataMember]
        public short UPRSM
        {
            get; set;
        }

        [DataMember]
        public float IPRSM
        {
            get; set;
        }

        [DataMember]
        public float PRSM
        {
            get; set;
        }

        [DataMember]
        public IList<short> ArrayVDUT
        {
            get; set;
        }

        [DataMember]
        public IList<short> ArrayIDUT
        {
            get; set;
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class CalibrationParams
    {

    }
}