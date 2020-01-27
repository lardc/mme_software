using System;
using System.Collections.Generic;
using SCME.Types.BaseTestParams;

namespace SCME.WpfControlLibrary.DataProviders
{
    public class TestParameter
    {
        public TestParameter(TestParametersType testParametersType, Type type, string name = null)
        {
            TestParametersType = testParametersType;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? TestParametersType.ToString();
        }

        public string Name { get; set; }
        public TestParametersType TestParametersType { get; set; }
        public Type Type { get; set; }

    }
  
}
