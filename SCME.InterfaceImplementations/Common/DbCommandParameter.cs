using System;
using System.Data;

namespace SCME.InterfaceImplementations.Common
{
    public class DbCommandParameter
    {
        public DbCommandParameter(string name, DbType dbType, int? size = null)
        {
            DbType = dbType;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Size = size;
        }

        public DbType DbType { get; }
        public string Name { get; }
        public int? Size { get; }
    }
}
