﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace SCME.Types.DataContracts
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestTypeLocalItem
    {
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class DeviceParametersLocalItem
    {
        [DataMember]
        public float Value { get; set; }

        [DataMember]
        public string Name { get; set; }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class DeviceLocalItem
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public Guid ProfileKey { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string MmeCode { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string StructureOrd { get; set; }

        [DataMember]
        public string StructureId { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public IEnumerable<long> ErrorCodes { get; set; }

        [DataMember]
        public Dictionary<TestTypeLocalItem, List<DeviceParametersLocalItem>> DeviceParameters { get; set; } 
        
    }
}