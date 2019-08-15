using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.EntityDataDB
{
    public class MME_CODES_TO_PROFILES
    {
        public int MME_CODE_ID { get; set; }
        [ForeignKey(nameof(MME_CODE_ID))]
        public virtual MME_CODE MME_CODE { get; set; }

        public int PROFILE_ID { get; set; }
        [ForeignKey(nameof(PROFILE_ID))]
        public virtual PROFILE PROFILE { get; set; }
    }
}
