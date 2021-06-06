using System;
using System.Runtime.Serialization;
using SCME.Types.BaseTestParams;

namespace SCME.Types.dVdt
{
    /// <summary>Состояние оборудования</summary>
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

    /// <summary>Причина ошибки</summary>
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

    /// <summary>Причина предупреждения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWWarningReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        WatchdogReset = 1001
    };

    /// <summary>Причина выключения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDisableReason
    {
        [EnumMember]
        None = 0,
    }

    /// <summary>Результат выполнения</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWOperationResult
    {
        [EnumMember]
        InProcess = 0,
        [EnumMember]
        Success = 1,
        [EnumMember]
        Fail = 2
    };

    [DataContract(Name = "Dvdt.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        /// <summary>Инициализирует новый экземпляр класса TestParameters</summary>
        public TestParameters()
        {
            TestParametersType = TestParametersType.Dvdt;
            IsEnabled = true;
            Voltage = 1000;
            VoltageRate = VoltageRate.V500;
            Mode = DvdtMode.Confirmation;
            ConfirmationCount = 1;
            VoltageRateLimit = 1000;
            VoltageRateOffSet = 100;
        }

        [DataMember]
        public ushort Voltage
        {
            get; set;
        }

        [DataMember]
        public DvdtMode Mode
        {
            get; set;
        }

        [DataMember]
        public VoltageRate VoltageRate
        {
            get; set;
        }

        [DataMember]
        public ushort ConfirmationCount
        {
            get; set;
        }

        [DataMember]
        public ushort VoltageRateLimit
        {
            get; set;
        }

        [DataMember]
        public ushort VoltageRateOffSet
        {
            get; set;
        }


        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool HasChanges(BaseTestParametersAndNormatives oldParameters)
        {
            TestParameters dvdDtOldParameters = (TestParameters)oldParameters;
            if (dvdDtOldParameters == null)
                throw new InvalidCastException("oldParameters must be dvdDtOldParameters");
            if (Mode != dvdDtOldParameters.Mode)
                return true;
            if (VoltageRate != dvdDtOldParameters.VoltageRate)
                return true;
            if (Voltage != dvdDtOldParameters.Voltage)
                return true;
            if (ConfirmationCount != dvdDtOldParameters.ConfirmationCount)
                return true;
            if (VoltageRateLimit != dvdDtOldParameters.VoltageRateLimit)
                return true;
            if (VoltageRateOffSet != dvdDtOldParameters.VoltageRateOffSet)
                return true;
            return false;
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestResults : BaseTestResults
    {
        /// <summary>Инициализирует новый экземпляр класса TestResults</summary>
        public TestResults()
        {
            Passed = false;
            VoltageRate = 0;
        }

        [DataMember]
        public bool Passed
        {
            get; set;
        }

        [DataMember]
        public ushort VoltageRate
        {
            get; set;
        }

        [DataMember]
        public DvdtMode Mode
        {
            get; set;
        }

    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class CalibrationParams
    {
        [DataMember]
        public ushort VFineN
        {
            get; set;
        }

        [DataMember]
        public ushort VFineD
        {
            get; set;
        }

        [DataMember]
        public ushort V500
        {
            get; set;
        }

        [DataMember]
        public ushort V1000
        {
            get; set;
        }

        [DataMember]
        public ushort V1500
        {
            get; set;
        }

        [DataMember]
        public ushort V2000
        {
            get; set;
        }

        [DataMember]
        public ushort V2500
        {
            get; set;
        }
    }
}