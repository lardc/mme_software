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
        public Commutation.ModuleCommutationType ComutationType { get; set; }
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
        public ObservableCollection<MyProfile> Childrens { get; set; } = new ObservableCollection<MyProfile>();
        [DataMember]
        public ProfileDeepData ProfileDeepData { get; set; } = new ProfileDeepData();

        public bool IsTop { get; set; }
        public MyProfile Parent { get; set; }

        public MyProfile GenerateNextVersion(ProfileDeepData profileDeepData)
        {
            MyProfile newProfile = new MyProfile(Id, Name, Guid.NewGuid(), Version+ 1, DateTime.Now)
            {
                ProfileDeepData = profileDeepData,
                IsTop = true,
            };
            newProfile.Childrens.Add(this);
            foreach (var i in Childrens)
                newProfile.Childrens.Add(i);
            return newProfile;
        }

    }
}
