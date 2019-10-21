using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SCME.Types.Profiles
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    [AddINotifyPropertyChangedInterface]
    public class ProfileDeepData
    {
        [DataMember]
        public ObservableCollection<BaseTestParametersAndNormatives> TestParametersAndNormatives { get; set; } = new ObservableCollection<BaseTestParametersAndNormatives>();

        #region Comutation
        [DataMember]
        public Commutation.ModuleCommutationType CommutationType { get; set; }
        #endregion
        #region Clamping
        [DataMember]
        public Clamping.ClampingForce ClampingForce { get; set; }
        [DataMember]
        public float ParameterClamp { get; set; }
        [DataMember]
        public bool IsHeightMeasureEnabled { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Temperature { get; set; }
        #endregion
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    [AddINotifyPropertyChangedInterface]
    public class MyProfile
    {
        public MyProfile(int id, string name, Guid key, int version, DateTime timestamp)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Key = key;
            Version = version;
            Timestamp = timestamp;
            Id = id;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid Key { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public ObservableCollection<MyProfile> Children { get; set; } = new ObservableCollection<MyProfile>();

        [DataMember]
        public ProfileDeepData DeepData { get; set; } = new ProfileDeepData();

        public bool IsTop { get; set; }
        public MyProfile Parent { get; set; }

        public MyProfile GenerateNextVersion(ProfileDeepData profileDeepData, string newName)
        {
            var newProfile = new MyProfile(Id, newName, Guid.NewGuid(), Version+ 1, DateTime.Now)
            {
                DeepData = profileDeepData,
                IsTop = true,
            };
            
            return newProfile;
        }

        public Profile ToProfile() => new Profile()
        {
            Name = Name,
            Key = Key,
            
            IsHeightMeasureEnabled = DeepData.IsHeightMeasureEnabled,
            ParametersClamp = DeepData.ParameterClamp,
            Height = DeepData.Height,
            Temperature = DeepData.Temperature,
            ParametersComm = DeepData.CommutationType,
            
            TestParametersAndNormatives = DeepData.TestParametersAndNormatives
        };
        
        public override bool Equals(object obj)
        {
            if (!(obj is MyProfile profile))
                return false;
            return Id == profile.Id;
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Id;

//        public class ProfileBuIdEqualityComparer : IEqualityComparer<MyProfile>
//        {
//            public bool Equals(MyProfile p1, MyProfile p2)
//            {
//                if (p2 == null && p1 == null)
//                    return true;
//                if (p1 == null || p2 == null)
//                    return false;
//                return p1.Id == p2.Id;
//            }
//
//            public int GetHashCode(MyProfile p)
//            {
//                return p.Id;
//            }
//        }
    }
}
