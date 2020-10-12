using System.ComponentModel.DataAnnotations.Schema;

namespace SCME.MEFADB.Tables
{
    [Table("MME_CODES")]
    public class MmeCode
    {
        [Column("MME_CODE_ID")]
        public int MmeCodeId { get; set; }
        
        [Column("MME_CODE")]
        public string Name { get; set; }
    }
}