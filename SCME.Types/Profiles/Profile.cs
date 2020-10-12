using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using PropertyChanged;
using SCME.Types.BaseTestParams;
using SCME.Types.SQL;

namespace SCME.Types.Profiles
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME", IsReference = true)]
    [KnownType(typeof(Profile))]
    [KnownType(typeof(ProfileSet))]
    [KnownType(typeof(ProfileFolder))]
    [AddINotifyPropertyChangedInterface]
    public abstract class ProfileDictionaryObject
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid Key { get; set; }

        [DataMember]
        public Guid NextGenerationKey { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public int Id { get; set; }




        public string TimeStampFormated
        {
            get { return Timestamp.ToString("dd.MM.yyyy HH:mm"); }
        }

        [DataMember]
        public ObservableCollection<ProfileDictionaryObject> Childrens { get; set; }
        [DataMember]
        public ProfileDictionaryObject Parent { get; set; }

        public ProfileDictionaryObject()
        {
            Name = "Default";
            Key = new Guid();
            Timestamp = DateTime.Now;
            Version = 1;

            Childrens = new ObservableCollection<ProfileDictionaryObject>();
        }

        public ProfileDictionaryObject(string Name, Guid Key, int version, DateTime Timestamp)
        {
            this.Name = Name;
            this.Key = Key;
            this.Timestamp = Timestamp;
            this.Version = version;

            Childrens = new ObservableCollection<ProfileDictionaryObject>();
        }

        public override string ToString()
        {
            return Name;
        }

    }

    public sealed class ProfileFolder : ProfileDictionaryObject
    {
        public ProfileFolder()
        {

        }

        public ProfileFolder(string Name, Guid Key, DateTime TimeStamp)
           : base(Name, Key, 0, TimeStamp)
        {
            throw new NotImplementedException();
        }

        public ProfileFolder(string Name, Guid Key, int version, DateTime TimeStamp)
            : base(Name, Key, version, TimeStamp)
        {

        }
    }

    public sealed class ProfileSet : ProfileDictionaryObject
    {
        public ProfileSet()
        {

        }

        public ProfileSet(string Name, Guid Key, int version, DateTime TimeStamp)
            : base(Name, Key, version, TimeStamp)
        {

        }

        public ProfileSet(string Name, Guid Key, DateTime TimeStamp)
           : base(Name, Key, 0, TimeStamp)
        {
            throw new NotImplementedException();
        }

        
    }

    [AddINotifyPropertyChangedInterface]
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public sealed class Profile : ProfileDictionaryObject
    {
        [DataMember]
        public bool IsTop { get; set; }

        [DataMember]
        public ObservableCollection<BaseTestParametersAndNormatives> TestParametersAndNormatives { get; set; }

        [DataMember]
        public Gate.TestParameters ParametersGate { get; set; }
        [DataMember]
        public Gate.ResultNormatives NormativesGate { get; set; }
        [DataMember]
        public VTM.TestParameters ParametersVTM { get; set; }
        [DataMember]
        public VTM.ResultNormatives NormativesVTM { get; set; }
        [DataMember]
        public BVT.TestParameters ParametersBVT { get; set; }
        [DataMember]
        public BVT.ResultNormatives NormativesBVT { get; set; }
        [DataMember]
        public Commutation.ModuleCommutationType ParametersComm { get; set; }
        [DataMember]
        public DutPackageType DutPackageType { get; set; }
        [DataMember]
        public float ParametersClamp { get; set; }
        [DataMember]
        public bool IsHeightMeasureEnabled { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Temperature { get; set; }



        
      


        private void ConstructorInit()
        {
            ParametersGate = new Gate.TestParameters();
            ParametersVTM = new VTM.TestParameters();
            ParametersBVT = new BVT.TestParameters();
            ParametersComm = new Commutation.ModuleCommutationType();
            ParametersClamp = 5.0f;
            TestParametersAndNormatives = new ObservableCollection<BaseTestParametersAndNormatives>(new List<BaseTestParametersAndNormatives>());


            NormativesGate = new Gate.ResultNormatives();
            NormativesVTM = new VTM.ResultNormatives();
            NormativesBVT = new BVT.ResultNormatives();
        }

        public ProfileItem ToProfileItem()
        {
            var profileItem = new ProfileItem()
            {
                ProfileId = Id,
                ProfileTS = Timestamp,
                ProfileKey = Key,
                ProfileName = Name,
                Version = Version,
                GateTestParameters = new List<Gate.TestParameters>(),
                VTMTestParameters = new List<VTM.TestParameters>(),
                BVTTestParameters = new List<BVT.TestParameters>(),
                DvDTestParameterses = new List<dVdt.TestParameters>(),
                ATUTestParameters = new List<ATU.TestParameters>(),
                QrrTqTestParameters = new List<QrrTq.TestParameters>(),
                TOUTestParameters = new List<TOU.TestParameters>(),
                CommTestParameters = ParametersComm,
                IsHeightMeasureEnabled = IsHeightMeasureEnabled,
                ParametersClamp = ParametersClamp,
                Height = Height,
                Temperature = Temperature,
            };

            foreach (var baseTestParametersAndNormativese in TestParametersAndNormatives)
            {
                switch (baseTestParametersAndNormativese)
                {
                    case Gate.TestParameters gate:
                        profileItem.GateTestParameters.Add(gate);
                        break;
                    case VTM.TestParameters sl:
                        profileItem.VTMTestParameters.Add(sl);
                        break;
                    case BVT.TestParameters bvt:
                        profileItem.BVTTestParameters.Add(bvt);
                        break;
                    case dVdt.TestParameters dvdt:
                        profileItem.DvDTestParameterses.Add(dvdt);
                        break;
                    case ATU.TestParameters atu:
                        profileItem.ATUTestParameters.Add(atu);
                        break;
                    case QrrTq.TestParameters qrrTq:
                        profileItem.QrrTqTestParameters.Add(qrrTq);
                        break;
                    case TOU.TestParameters tou:
                        profileItem.TOUTestParameters.Add(tou);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return profileItem;
        }

        public Profile()
        {
            ConstructorInit();
        }

        public Profile(ProfileForSqlSelect prof)
           : base(prof.Name, prof.Key, Convert.ToInt32(prof.Version), prof.TS)
        {
            ConstructorInit();
        }

        public Profile(string Name, Guid Key, long version, DateTime TS)
            : base(Name, Key, Convert.ToInt32(version), TS)
        {
            ConstructorInit();
        }

        public Profile(string Name, Guid Key, DateTime TS)
          : base(Name, Key, 0, TS)
        {
            ConstructorInit();
        }
    }
}
