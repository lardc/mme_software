using System;
using System.Collections.Generic;
using SCME.Types.BaseTestParams;

namespace SCME.WpfControlLibrary.DataProviders
{
    public class TestParameter
    {
        public TestParameter(string name, TestParametersType testParametersType, Type type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TestParametersType = testParametersType;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public TestParameter(TestParametersType testParametersType, Type type)
        {
            TestParametersType = testParametersType;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = TestParametersType.ToString();
        }

        public string Name { get; set; }
        public TestParametersType TestParametersType { get; set; }
        public Type Type { get; set; }

    }

    public static class TestParameterDictionary
    {
        public static List<TestParameter> TestParametersList { get; set; } = new List<TestParameter>()
        {
            new TestParameter(TestParametersType.Gate, typeof(Types.Gate.TestParameters)),
            new TestParameter(TestParametersType.StaticLoses, typeof(Types.VTM.TestParameters)),
            new TestParameter(TestParametersType.Bvt, typeof(Types.BVT.TestParameters)),
            new TestParameter(TestParametersType.Dvdt, typeof(Types.dVdt.TestParameters)),
            new TestParameter(TestParametersType.ATU, typeof(Types.ATU.TestParameters)),
            new TestParameter(TestParametersType.QrrTq, typeof(Types.QrrTq.TestParameters)),
            new TestParameter(TestParametersType.RAC, typeof(Types.RAC.TestParameters)),
            new TestParameter(TestParametersType.TOU, typeof(Types.TOU.TestParameters)),

            //new TestParametrContainer(TestParametersType.Clamping, typeof(Types.Clamping.TestParameters)),
            //new TestParametrContainer(TestParametersType.Commutation, typeof(Types.Commutation.TestParameters)),
            //new TestParametrContainer(TestParametersType.IH, typeof(Types.IH.TestParameters)),
            //new TestParametrContainer(TestParametersType.RCC, typeof(Types.RCC.TestParameters)),
            //new TestParametrContainer(TestParametersType.Sctu, typeof(Types.SCTU.SctuTestParameters)),
        };

    }
}
