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
    }
}
