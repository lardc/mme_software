using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using PropertyChanged;
using SCME.Types.BaseTestParams;

namespace SCME.Types.BVT
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
        Charging = 3,
        [EnumMember]
        PowerReady = 4,
        [EnumMember]
        InProcess = 5
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWFaultReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        BridgeOverload = 400,
        [EnumMember]
        FolowingError = 204,
        [EnumMember]
        TemperatureOverload = 401
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDisableReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        NoShSignal = 400,
        [EnumMember]
        NoStSignal = 401,
        [EnumMember]
        NoTempSignal = 402
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWWarningReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        CurrentNotReached = 401,
        [EnumMember]
        WatchdogReset = 1001
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWProblemReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Stopped = 401
    };

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
    
    [DataContract(Name = "Bvt.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    [AddINotifyPropertyChangedInterface]
//    [KnownType(typeof(BaseTestParametersAndNormatives))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
     
        [DataMember]
        public BVTTestType UdsmUrsmTestType { get; set; }
        
        [DataMember]
        public ushort UdsmUrsmPulseFrequency{ get; set; }
        
        [DataMember]
        public float UdsmUrsmCurrentLimit{ get; set; }
        
        [DataMember]
        public float UdsmUrsmRampUpVoltage{ get; set; }
        
        [DataMember]
        public ushort UdsmUrsmStartVoltage { get; set; }
        
        [DataMember]
        public ushort UdsmUrsmVoltageFrequency { get; set; }
        
        [DataMember]
        public ushort UdsmUrsmFrequencyDivisor { get; set; }
        
        [DataMember]
        public ushort UdsmUrsmVoltageLimitR { get; set; }
        
        [DataMember]
        public ushort UdsmUrsmVoltageLimitD { get; set; }
        
        [DataMember]
        public ushort UdsmUrsmPlateTime { get; set; }


        [DataMember]
        public ushort VDSM { get; set; }

        [DataMember]
        public ushort VRSM { get; set; }
        [DataMember]
        public float IDSM { get; set; }

        [DataMember]
        public float IRSM { get; set; }

        [DataMember]
        public bool UseUdsmUrsm { get; set; }
        
        [DataMember]
        public int? ClassByProfileName { get; set; }
        
        [DataMember]
        public ushort PulseFrequency { get; set; }
        
        [DataMember]
        public BVTMeasurementMode MeasurementMode { get; set; }

        [DataMember]
        public ushort VoltageLimitD { get; set; }

        [DataMember]
        public ushort VoltageLimitR { get; set; }

        [DataMember]
        public float CurrentLimit { get; set; }

        [DataMember]
        public ushort PlateTime { get; set; }

        [DataMember]
        public float RampUpVoltage { get; set; }

        [DataMember]
        public ushort StartVoltage { get; set; }

        [DataMember]
        public ushort VoltageFrequency { get; set; }

        [DataMember]
        public ushort FrequencyDivisor { get; set; }

        [DataMember]
        public BVTTestType TestType { get; set; }

        [DataMember]
        public ushort VDRM { get; set; }

        [DataMember]
        public ushort VRRM { get; set; }

        [DataMember]
        public float IDRM { get; set; }

        [DataMember]
        public float IRRM { get; set; }

        public override bool HasChanges(BaseTestParametersAndNormatives oldParametersBase)
        {
            var oldParameters = oldParametersBase as TestParameters;
            if (oldParameters == null)
                throw new InvalidCastException("oldParametersBase must be bvtOldParameters");

            if (UseUdsmUrsm != oldParameters.UseUdsmUrsm)
                return true;
            if (PulseFrequency != oldParameters.PulseFrequency)
                return true;
            if (MeasurementMode != oldParameters.MeasurementMode)
                return true;
            if (VoltageLimitD != oldParameters.VoltageLimitD)
                return true;
            if (VoltageLimitR != oldParameters.VoltageLimitR)
                return true;
            if (CurrentLimit.CompareTo(oldParameters.CurrentLimit) != 0)
                return true;
            if (PlateTime != oldParameters.PlateTime)
                return true;
            if (RampUpVoltage.CompareTo(oldParameters.RampUpVoltage) != 0)
                return true;
            if (StartVoltage != oldParameters.StartVoltage)
                return true;
            if (VoltageFrequency != oldParameters.VoltageFrequency)
                return true;
            if (FrequencyDivisor != oldParameters.FrequencyDivisor)
                return true;
            if (TestType != oldParameters.TestType)
                return true;
            if (VDRM != oldParameters.VDRM)
                return true;
            if (VRRM != oldParameters.VRRM)
                return true;
            if (IDRM.CompareTo(oldParameters.IDRM) != 0)
                return true;
            if (IRRM.CompareTo(oldParameters.IRRM) != 0)
                return true;
            
            if (UdsmUrsmPulseFrequency != oldParameters.UdsmUrsmPulseFrequency)
                return true;    
            if (UdsmUrsmVoltageLimitD != oldParameters.VoltageLimitD)
                return true;
            if (UdsmUrsmVoltageLimitR != oldParameters.VoltageLimitR)
                return true;
            if (UdsmUrsmCurrentLimit.CompareTo(oldParameters.CurrentLimit) != 0)
                return true;
            if (UdsmUrsmPlateTime != oldParameters.PlateTime)
                return true;
            if (UdsmUrsmRampUpVoltage.CompareTo(oldParameters.RampUpVoltage) != 0)
                return true;
            if (UdsmUrsmStartVoltage != oldParameters.StartVoltage)
                return true;
            if (UdsmUrsmVoltageFrequency != oldParameters.VoltageFrequency)
                return true;
            if (UdsmUrsmFrequencyDivisor != oldParameters.FrequencyDivisor)
                return true;
            if (UdsmUrsmTestType != oldParameters.TestType)
                return true;
            if (VDSM != oldParameters.VDSM)
                return true;
            if (VRSM != oldParameters.VRSM)
                return true;
            if (IDSM.CompareTo(oldParameters.IDSM) != 0)
                return true;
            if (IRSM.CompareTo(oldParameters.IRSM) != 0)
                return true;

            return false;
        }

        public TestParameters()
        {
            IsEnabled = false;
            UdsmUrsmTestType = TestType = BVTTestType.Reverse;
            MeasurementMode = BVTMeasurementMode.ModeV;
            UdsmUrsmVoltageLimitD =VoltageLimitD = 1000;
            UdsmUrsmVoltageLimitR =VoltageLimitR = 1000;
            UdsmUrsmCurrentLimit =CurrentLimit = 5;
            UdsmUrsmPlateTime =PlateTime = 1000;
            UdsmUrsmRampUpVoltage =RampUpVoltage = 2;
            UdsmUrsmStartVoltage =StartVoltage = 500;
            UdsmUrsmVoltageFrequency =VoltageFrequency = 50;
            UdsmUrsmFrequencyDivisor = FrequencyDivisor = 1;
            VDRM = 1400;
            VRRM = 1400;
            IDSM =IDRM = 5;
            IRSM = IRRM = 5;
            

            TestParametersType = TestParametersType.Bvt;



        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class ResultNormatives
    {
        [DataMember]
        public ushort VDRM { get; set; }

        [DataMember]
        public ushort VRRM { get; set; }

        [DataMember]
        public float IDRM { get; set; }

        [DataMember]
        public float IRRM { get; set; }

        
        [DataMember]
        public float UdsmUrsmIDRM { get; set; }

        [DataMember]
        public float UdsmUrsmIRRM { get; set; }
        
        
        public ResultNormatives()
        {
            VDRM = 1400;
            VRRM = 1400;
            UdsmUrsmIDRM = IDRM = 5;
            UdsmUrsmIRRM = IRRM = 5;
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestResults : BaseTestResults
    {
        [DataMember]
        public ushort VDSM { get; set; }

        [DataMember]
        public ushort VRSM { get; set; }

        [DataMember]
        public float IDSM { get; set; }

        [DataMember]
        public float IRSM { get; set; }

        [DataMember]
        public ushort VDRM { get; set; }

        [DataMember]
        public ushort VRRM { get; set; }

        [DataMember]
        public float IDRM { get; set; }

        [DataMember]
        public float IRRM { get; set; }

        [DataMember]
        public List<short> VoltageData { get; set; }

        [DataMember]
        public List<short> CurrentData { get; set; }

        public TestResults()
        {
            VoltageData = new List<short>();
            CurrentData = new List<short>();
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class CalibrationParams
    {
        [DataMember]
        public ushort S1Current1FineN { get; set; }

        [DataMember]
        public ushort S1Current1FineD { get; set; }

        [DataMember]
        public ushort S1Current2FineN { get; set; }

        [DataMember]
        public ushort S1Current2FineD { get; set; }

        [DataMember]
        public ushort S1Voltage1FineN { get; set; }

        [DataMember]
        public ushort S1Voltage1FineD { get; set; }

        [DataMember]
        public ushort S1Voltage2FineN { get; set; }

        [DataMember]
        public ushort S1Voltage2FineD { get; set; }
    }
}