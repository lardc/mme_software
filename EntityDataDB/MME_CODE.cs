using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.EntityDataDB
{
    public class MME_CODE
    {
        [Key]
        public int MME_CODE_ID { get; set; }

        [Column("MME_CODE")]
        public string Name { get; set; }

        public virtual ICollection<MME_CODES_TO_PROFILES> MME_CODES_TO_PROFILES { get; set; } = new HashSet<MME_CODES_TO_PROFILES>();
    }
}
