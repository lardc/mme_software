using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using SCME.Types.Clamping;
using SCME.Types.Commutation;
using SCME.Types.Gate;
using TestParameters = SCME.Types.Gate.TestParameters;

namespace SCME.Types
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class LogItem
    {
        [DataMember]
        public Int64 ID { get; set; }

        [DataMember]
        public ComplexParts Source { get; set; }

        [DataMember]
        public LogMessageType MessageType { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class ResultItem
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Party { get; set; }

        [DataMember]
        public string StructureOrd { get; set; }

        [DataMember]
        public string StructureName { get; set; }

        [DataMember]
        public Guid ProfileKey { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string MmeCode { get; set; }

        [DataMember]
        public TestResults[] Gate { get; set; }

        [DataMember]
        public SL.TestResults[] VTM { get; set; }

        [DataMember]
        public BVT.TestResults[] BVT { get; set; }

        [DataMember]
        public TestParameters[] GateTestParameters { get; set; }

        [DataMember]
        public SL.TestParameters[] VTMTestParameters { get; set; }

        [DataMember]
        public BVT.TestParameters[] BVTTestParameters { get; set; }

        [DataMember]
        public dVdt.TestParameters[] DvdTestParameterses { get; set; }

        [DataMember]
        public bool IsHeightMeasureEnabled { get; set; }

        [DataMember]
        public bool IsHeightOk { get; set; }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class ProfileItem
    {
        [DataMember]
        public long ProfileId { get; set; }

        [DataMember]
        public string ProfileName { get; set; }

        [DataMember]
        public Guid ProfileKey { get; set; }

        [DataMember]
        public DateTime ProfileTS { get; set; }

        [DataMember]
        public List<TestParameters> GateTestParameters { get; set; }

        [DataMember]
        public List<SL.TestParameters> VTMTestParameters { get; set; }

        [DataMember]
        public List<BVT.TestParameters> BVTTestParameters { get; set; }

        [DataMember]
        public List<dVdt.TestParameters> DvDTestParameterses { get; set; }
            
        [DataMember]
        public ModuleCommutationType CommTestParameters { get; set; }

        [DataMember]
        public ClampingForce ClampingForce { get; set; }

        [DataMember]
        public float ParametersClamp { get; set; }

        [DataMember]
        public bool IsHeightMeasureEnabled { get; set; }
        
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int Temperature { get; set; }

        [DataMember]
        public List<ProfileItem> ChildProfileItems { get; set; }

        public override bool Equals(object obj)
        {
            var compare = obj as ProfileItem;
            if (compare == null)
                return false;

            return ProfileKey == compare.ProfileKey;
        }

        public override int GetHashCode()
        {
            return ProfileKey.GetHashCode();
        }

        public bool HasChanges(ProfileItem oldItem)
        {
            if (oldItem == null)
                return false;

            if (ProfileName != oldItem.ProfileName)
                return true;

            if (GateTestParameters.Count != oldItem.GateTestParameters.Count)
                return true;
            
            if (VTMTestParameters.Count != oldItem.VTMTestParameters.Count)
                return true;

            if (BVTTestParameters.Count != oldItem.BVTTestParameters.Count)
                return true;

            if (DvDTestParameterses.Count != oldItem.DvDTestParameterses.Count)
                return true;

            if (CommTestParameters != oldItem.CommTestParameters)
                return true;
            if (ParametersClamp.CompareTo(oldItem.ParametersClamp) != 0)
                return true;

            if (IsHeightMeasureEnabled != oldItem.IsHeightMeasureEnabled)
                return true;

            if (Height != oldItem.Height)
                return true;

            if (Temperature != oldItem.Temperature)
                return true;

            for (var i = 0; i < GateTestParameters.Count; i++)
            {
                if (GateTestParameters[i].IsHasChanges(oldItem.GateTestParameters[i]))
                    return true;
            }

            for (var i = 0; i < VTMTestParameters.Count; i++)
            {
                if (VTMTestParameters[i].IsHasChanges(oldItem.VTMTestParameters[i]))
                    return true;
            }

            for (var i = 0; i < BVTTestParameters.Count; i++)
            {
                if (BVTTestParameters[i].IsHasChanges(oldItem.BVTTestParameters[i]))
                    return true;
            }

            for (var i = 0; i < DvDTestParameterses.Count; i++)
            {
                if (DvDTestParameterses[i].IsHasChanges(oldItem.DvDTestParameterses[i]))
                    return true;
            }

            return false;
        }

    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class DeviceItem
    {
        [DataMember]
        public long InternalID { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string StructureOrd { get; set; }

        [DataMember]
        public string StructureID { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string ProfileName { get; set; }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class ParameterItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string NameLocal { get; set; }

        [DataMember]
        public float Value { get; set; }

        [DataMember]
        public Boolean IsHide { get; set; }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class ParameterNormativeItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public float? Min { get; set; }

        [DataMember]
        public float? Max { get; set; }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class ConditionItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string NameLocal { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public Boolean IsTech { get; set; }
    }

    [ServiceContract(Namespace = "http://proton-electrotex.com/SCME", SessionMode = SessionMode.Required)]
    public interface IDatabaseCommunicationService
    {
        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void Check();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<LogItem> ReadLogs(long Tail, long Count);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<string> ReadGroups(DateTime? From, DateTime? To);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<string> ReadProfiles();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<DeviceItem> ReadDevices(string Group);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<ParameterItem> ReadDeviceParameters(long InternalID);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<ConditionItem> ReadDeviceConditions(long InternalID);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<ParameterNormativeItem> ReadDeviceNormatives(long InternalID);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<int> ReadDeviceErrors(long InternalID);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<ProfileItem> GetProfileItems();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<ProfileItem> GetProfileItemsByMmeCode(string mmeCode);

    }
}