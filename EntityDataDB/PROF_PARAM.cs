//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SCME.EntityDataDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROF_PARAM
    {
        
        
        
        public Nullable<double> MIN_VAL { get; set; }
        public Nullable<double> MAX_VAL { get; set; }


        public int PARAM_ID { get; set; }
        [ForeignKey(nameof(PARAM_ID))]
        public virtual PARAM PARAM { get; set; }

        public int PROF_ID { get; set; }
        [ForeignKey(nameof(PROF_ID))]
        public virtual PROFILE PROFILE { get; set; }

        public int PROF_TESTTYPE_ID { get; set; }
        [ForeignKey(nameof(PROF_TESTTYPE_ID))]
        public virtual PROF_TEST_TYPE PROF_TEST_TYPE { get; set; }
    }
}
