using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SCME.Types.SQL
{
    [DataContract]
    public class ProfileForSqlSelect
    {
        [DataMember]
        public int  Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Guid Key { get; set; }
        [DataMember]
        public int Version { get; set; }
        [DataMember]
        public DateTime TS { get; set; }
        public ProfileForSqlSelect() { }
        public ProfileForSqlSelect(int id, string name, Guid key, int version, DateTime tS)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Key = key;
            Version = version;
            TS = tS;
        }

        public Profile ToProfile()
        {
            return new Profile()
            {
                Id = Id,
                Key = Key,
                Version = Version,
                Timestamp = TS,
                Name = Name
            };
        }

        public ProfileItem ToProfileItem()
        {
            return new ProfileItem()
            {
                ProfileId = Id,
                ProfileKey = Key,
                Version = Version,
                ProfileTS = TS,
                ProfileName = Name
            };
        }

        public ProfileItem ToProfileItemWithChild(IEnumerable<ProfileForSqlSelect> profileItems)
        {
            var profile = ToProfileItem();
            profile.ChildProfileItems = profileItems.Select(m=> m.ToProfileItem()).ToList();
            return profile;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj.GetType() == GetType())
                return false;
            else
                return Key == ((ProfileForSqlSelect) obj).Key;
        }

    }
}
