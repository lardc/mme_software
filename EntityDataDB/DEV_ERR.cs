using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.EntityDataDB
{
    public class DEV_ERR
    {
        [Key, Column(Order = 0)]
        public int DEV_ID { get; set; }

        public DEVICE DEVICE { get; set; }


        [Key, Column(Order = 1)]
        public int ERR_ID { get; set; }

        public ERROR ERROR { get; set; }
    }
}
