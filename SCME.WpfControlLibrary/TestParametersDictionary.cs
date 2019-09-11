using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.WpfControlLibrary
{
    public class TestParametrContainer
    {
        public TestParametrContainer(string name, TestParametersType testParametersType, Type type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TestParametersType = testParametersType;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public TestParametrContainer(TestParametersType testParametersType, Type type)
        {
            TestParametersType = testParametersType;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = TestParametersType.ToString();
        }

        public string Name { get; set; }
        public TestParametersType TestParametersType { get; set; }
        public Type Type { get; set; }

    }

    public static class TestParametersDictionary
    {
        public static List<TestParametrContainer> TestParametersList { get; set; } = new List<TestParametrContainer>()
        {
            new TestParametrContainer(TestParametersType.Gate, typeof(Types.Gate.TestParameters)),
            new TestParametrContainer(TestParametersType.StaticLoses, typeof(Types.VTM.TestParameters)),
            new TestParametrContainer(TestParametersType.Bvt, typeof(Types.BVT.TestParameters)),
            new TestParametrContainer(TestParametersType.Dvdt, typeof(Types.dVdt.TestParameters)),
            new TestParametrContainer(TestParametersType.ATU, typeof(Types.ATU.TestParameters)),
            new TestParametrContainer(TestParametersType.QrrTq, typeof(Types.QrrTq.TestParameters)),
            new TestParametrContainer(TestParametersType.RAC, typeof(Types.RAC.TestParameters)),
            new TestParametrContainer(TestParametersType.TOU, typeof(Types.TOU.TestParameters)),

            //new TestParametrContainer(TestParametersType.Clamping, typeof(Types.Clamping.TestParameters)),
            //new TestParametrContainer(TestParametersType.Commutation, typeof(Types.Commutation.TestParameters)),
            //new TestParametrContainer(TestParametersType.IH, typeof(Types.IH.TestParameters)),
            //new TestParametrContainer(TestParametersType.RCC, typeof(Types.RCC.TestParameters)),
            //new TestParametrContainer(TestParametersType.Sctu, typeof(Types.SCTU.SctuTestParameters)),
        };

    }
}
