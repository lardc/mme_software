using System;
using System.Data;

namespace SCME.InterfaceImplementations.Common
{
    public class TDbCommandParametr
    {
        public TDbCommandParametr(string name, DbType dbType, int? size = null)
        {
            DbType = dbType;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Size = size;
        }

        public DbType DbType { get; set; }
        public string Name { get; set; }
        public int? Size { get; set; }
    }
}
