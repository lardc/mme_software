using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types
{
    public class TestParameterTypeMeasurement
    {
        public TestParameterTypeMeasurement(TestParametersType testParametersType, Type type, string name = null)
        {
            TestParametersType = testParametersType;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? TestParametersType.ToString();
        }

        public string Name { get; set; }
        public TestParametersType TestParametersType { get; set; }
        public Type Type { get; set; }

        public static List<TestParameterTypeMeasurement> GetDefaultList() =>
            new List<TestParameterTypeMeasurement>()
                {
                    new TestParameterTypeMeasurement(TestParametersType.OutputLeakageCurrent, typeof(Types.OutputLeakageCurrent.TestParameters), "Ток утечки на выходе"),
                    new TestParameterTypeMeasurement(TestParametersType.OutputResidualVoltage, typeof(Types.OutputResidualVoltage.TestParameters), "Выходное остаточное напряжение"),
                    new TestParameterTypeMeasurement(TestParametersType.InputOptions, typeof(Types.InputOptions.TestParameters), "Параметры входа"),
                };

        public static List<TestParameterTypeMeasurement> GetAllList() =>
          new List<TestParameterTypeMeasurement>()
              {
                    new TestParameterTypeMeasurement(TestParametersType.OutputLeakageCurrent, typeof(Types.OutputLeakageCurrent.TestParameters), "Ток утечки на выходе"),
                    new TestParameterTypeMeasurement(TestParametersType.OutputResidualVoltage, typeof(Types.OutputResidualVoltage.TestParameters), "Выходное остаточное напряжение"),
                    new TestParameterTypeMeasurement(TestParametersType.InputOptions, typeof(Types.InputOptions.TestParameters), "Параметры входа"),
                    new TestParameterTypeMeasurement(TestParametersType.AuxiliaryPower, typeof(Types.AuxiliaryPower.TestParameters), "Вспомогательное пиотание")
              };

    }
}
