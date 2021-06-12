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
        /// <summary>Неопределенное состояние</summary>
        [EnumMember]
        DS_None = 0,
        /// <summary>Ошибка</summary>
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
        DS_InProcess = 5
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
        Accuracy = 3,
        /// <summary>Пробой прибора</summary>
        [EnumMember]
        Break = 4,
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

    /// <summary>Параметры произведения тестов</summary>
    [DataContract(Name = "ATU.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        /// <summary>Инициализирует новый экземпляр класса TestParameters</summary>
        public TestParameters()
        {
            TestParametersType = TestParametersType.ATU;
            IsEnabled = true;
            PrePulseValue = 100;
            PowerValue = 16;
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

        /// <summary>Проверка изменений в параметрах</summary>
        /// <param name="oldParameters">Старые параметры</param>
        /// <returns>Возвращает True, если параметры были изменены</returns>
        public override bool HasChanges(BaseTestParametersAndNormatives oldParameters)
        {
            //Старые параметры
            TestParameters OldTestParameters = (TestParameters)oldParameters;
            if (oldParameters == null)
                throw new InvalidCastException("OldParameters must be ATUOldParameters");
            if (PrePulseValue != OldTestParameters.PrePulseValue)
                return true;
            if (PowerValue != OldTestParameters.PowerValue)
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
            ArrayIDUT = new List<short>();
            ArrayVDUT = new List<short>();
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
        public IList<short> ArrayIDUT
        {
            get; set;
        }

        [DataMember]
        public IList<short> ArrayVDUT
        {
            get; set;
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class CalibrationParameters
    {

    }
}