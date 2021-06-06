using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SCME.Types.Gate
{
    /// <summary>Состояние оборудования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDeviceState
    {
        /// <summary>Неопределенное состояние (после включения питания)</summary>
        [EnumMember]
        None = 0,
        /// <summary>Состояние ошибки</summary>
        [EnumMember]
        Fault = 1,
        /// <summary>Выключен</summary>
        [EnumMember]
        Disabled = 2,
        /// <summary>Прозвонка</summary>
        [EnumMember]
        Kelvin = 3,
        [EnumMember]
        Gate = 4,
        [EnumMember]
        IH = 5,
        [EnumMember]
        IL = 6,
        [EnumMember]
        Resistance = 7,
        [EnumMember]
        CalGate = 8,
        [EnumMember]
        CalHolding = 9
    };

    /// <summary>Причина ошибки</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWFaultReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        HoldProcessError = 101,
        [EnumMember]
        GateProcessError = 112,
        [EnumMember]
        LatchProcessError = 121,
    };

    /// <summary>Причина предупреждения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWWarningReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        HoldingCurrentSmall = 101,
        [EnumMember]
        WatchdogReset = 1001
    };

    /// <summary>Причина выключения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDisableReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        BadClock = 1001
    };

    /// <summary>Причина проблемы</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWProblemReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        HoldReachTimeout = 101,
        [EnumMember]
        GateCurrentHigh = 111,
        [EnumMember]
        GateFollowingError = 112,
        [EnumMember]
        GateIgtOverload = 113,
        [EnumMember]
        LatchCurrentHigh = 121,
        [EnumMember]
        LatchFollowingError = 122,
        [EnumMember]
        RGateFollowingError = 141,
        [EnumMember]
        RGateOverload = 142
    };

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
    [DataContract(Name = "Gate.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        private int useIhGost;

        /// <summary>Инициализирует новый экземпляр класса TestParameters</summary>
        public TestParameters()
        {
            TestParametersType = TestParametersType.Gate;
            Resistance = 100;
            IGT = 500;
            VGT = 2.5f;
            IH = 150;
            IL = 1000;
            VGNT = 100;
            IGNT = 25;
            Itm = 0;
            CurrentLimit = 5;
            VoltageLimitD = 1000;
            PlateTime = 1000;
            RampUpVoltage = 2;
            StartVoltage = 500;
            GateLimitV = 100;
            GateLimitI = 25;
            IsEnabled = true;
        }

        [DataMember]
        public bool IsCurrentEnabled
        {
            get; set;
        }
        
        [DataMember]
        public ushort Itm
        {
            get; set;
        }

        [DataMember]
        public bool IsIhEnabled
        {
            get; set;
        }

        [DataMember]
        public bool IsIhStrikeCurrentEnabled
        {
            get; set;
        }

        [DataMember]
        public bool IsIlEnabled
        {
            get; set;
        }

        [DataMember]
        public float Resistance
        {
            get; set;
        }

        [DataMember]
        public float IGT
        {
            get; set;
        }

        [DataMember]
        public float MinIGT
        {
            get; set;
        }

        [DataMember]
        public float VGT
        {
            get; set;
        }

        [DataMember]
        public bool UseIhGost
        {
            get => Convert.ToBoolean(useIhGost);
            set => useIhGost = value ? 1 : 0;
        }

        [DataMember]
        public float IH
        {
            get; set;
        }

        [DataMember]
        public float IL
        {
            get; set;
        }

        [DataMember]
        public float VGNT
        {
            get; set;
        }

        [DataMember]
        public ushort IGNT
        {
            get; set;
        }

        [DataMember]
        public bool UseVgnt
        {
            get; set;
        }

        [DataMember]
        public float CurrentLimit
        {
            get; set;
        }

        [DataMember]
        public ushort VoltageLimitD
        {
            get; set;
        }

        [DataMember]
        public ushort PlateTime
        {
            get; set;
        }

        [DataMember]
        public float RampUpVoltage
        {
            get; set;
        }

        [DataMember]
        public ushort StartVoltage
        {
            get; set;
        }

        [DataMember]
        public ushort GateLimitV
        {
            get; set;
        }

        [DataMember]
        public ushort GateLimitI
        {
            get; set;
        }

        public override bool HasChanges(BaseTestParametersAndNormatives oldParametersBase)
        {
            TestParameters oldParameters = (TestParameters)oldParametersBase;
            if (oldParameters == null)
                throw new InvalidCastException("oldParameters must be gateOldParameters");
            if (IsCurrentEnabled != oldParameters.IsCurrentEnabled)
                return true;
            if (IsIhEnabled != oldParameters.IsIhEnabled)
                return true;
            if (IsIhStrikeCurrentEnabled != oldParameters.IsIhStrikeCurrentEnabled)
                return true;
            if (IsIlEnabled != oldParameters.IsIlEnabled)
                return true;
            if (Resistance.CompareTo(oldParameters.Resistance) != 0)
                return true;
            if (IGT.CompareTo(oldParameters.IGT) != 0)
                return true;
            if (VGT.CompareTo(oldParameters.VGT) != 0)
                return true;
            if (IH.CompareTo(oldParameters.IH) != 0)
                return true;
            if (IL.CompareTo(oldParameters.IL) != 0)
                return true;
            if (Itm.CompareTo(oldParameters.Itm) != 0)
                return true;
            if (UseVgnt != oldParameters.UseVgnt)
                return true;
            if (CurrentLimit.CompareTo(oldParameters.CurrentLimit) != 0)
                return true;
            if (VoltageLimitD.CompareTo(oldParameters.VoltageLimitD) != 0)
                return true;
            if (PlateTime.CompareTo(oldParameters.PlateTime) != 0)
                return true;
            if (RampUpVoltage.CompareTo(oldParameters.RampUpVoltage) != 0)
                return true;
            if (StartVoltage.CompareTo(oldParameters.StartVoltage) != 0)
                return true;
            if (GateLimitV.CompareTo(oldParameters.GateLimitV) != 0)
                return true;
            if (GateLimitI.CompareTo(oldParameters.GateLimitI) != 0)
                return true;
            return false;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>Нормативы тестирования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class ResultNormatives
    {
        /// <summary>Инициализирует новый экземпляр класса ResultNormatives</summary>
        public ResultNormatives()
        {
            Resistance = 100;
            IGT = 500;
            VGT = 2.5f;
            IH = 150;
            IL = 1000;
            VGNT = 100;
            IGNT = 100;
        }

        [DataMember]
        public float Resistance
        {
            get; set;
        }

        [DataMember]
        public float IGT
        {
            get; set;
        }

        [DataMember]
        public float VGT
        {
            get; set;
        }

        [DataMember]
        public float IH
        {
            get; set;
        }

        [DataMember]
        public float IL
        {
            get; set;
        }

        [DataMember]
        public float VGNT
        {
            get; set;
        }

        [DataMember]
        public ushort IGNT
        {
            get; set;
        }
    }

    /// <summary>Результаты тестирования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestResults : BaseTestResults
    {
        /// <summary>Инициализирует новый экземпляр класса TestResults</summary>
        public TestResults()
        {
            ArrayVGT = new List<short>();
            ArrayIGT = new List<short>();
            ArrayIH = new List<short>();
            ArrayKelvin = new List<short>();
        }

        [DataMember]
        public float Resistance
        {
            get; set;
        }

        [DataMember]
        public float IGT
        {
            get; set;
        }

        [DataMember]
        public float VGT
        {
            get; set;
        }

        [DataMember]
        public float IH
        {
            get; set;
        }

        [DataMember]
        public float IL
        {
            get; set;
        }

        [DataMember]
        public bool IsKelvinOk
        {
            get; set;
        }

        [DataMember]
        public float VGNT
        {
            get; set;
        }

        [DataMember]
        public ushort IGNT
        {
            get; set;
        }

        [DataMember]
        public IList<short> ArrayVGT
        {
            get; set;
        }

        [DataMember]
        public IList<short> ArrayIGT
        {
            get; set;
        }

        [DataMember]
        public IList<short> ArrayIH
        {
            get; set;
        }

        [DataMember]
        public IList<short> ArrayKelvin
        {
            get; set;
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class CalibrationResultGate
    {
        [DataMember]
        public ushort Current
        {
            get; set;
        }

        [DataMember]
        public ushort Voltage
        {
            get; set;
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class CalibrationParameters
    {
        [DataMember]
        public short GateIGTOffset
        {
            get; set;
        }

        [DataMember]
        public short GateVGTOffset
        {
            get; set;
        }

        [DataMember]
        public short GateIHLOffset
        {
            get; set;
        }

        [DataMember]
        public ushort RgCurrent
        {
            get; set;
        }

        [DataMember]
        public ushort GateFineIGT_N
        {
            get; set;
        }

        [DataMember]
        public ushort GateFineIGT_D
        {
            get; set;
        }

        [DataMember]
        public ushort GateFineVGT_N
        {
            get; set;
        }

        [DataMember]
        public ushort GateFineVGT_D
        {
            get; set;
        }

        [DataMember]
        public ushort GateFineIHL_N
        {
            get; set;
        }

        [DataMember]
        public ushort GateFineIHL_D
        {
            get; set;
        }
    }
}