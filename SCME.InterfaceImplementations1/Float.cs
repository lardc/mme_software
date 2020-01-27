using System;

namespace SCME.InterfaceImplementations
{
    public static class Float
    {
        public static float ParseInternationally(string str)
        {
            return float.Parse(str.Replace('.', ','));
        }
    }
}