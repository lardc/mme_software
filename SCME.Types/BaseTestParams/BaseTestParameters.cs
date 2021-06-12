using System;
using System.Runtime.Serialization;
using ATUParameters = SCME.Types.ATU.TestParameters;
using BVTTestParameters = SCME.Types.BVT.TestParameters;
using dVdtParameters = SCME.Types.dVdt.TestParameters;
using GTUTestParameters = SCME.Types.GTU.TestParameters;
using QrrTqParameters = SCME.Types.QrrTq.TestParameters;
using SLTestParameters = SCME.Types.VTM.TestParameters;
using TOUParameters = SCME.Types.TOU.TestParameters;

namespace SCME.Types.BaseTestParams
{
    /// <summary>Тип параметра</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum TestParametersType
    {
        /// <summary>GTU</summary>
        [EnumMember]
        GTU = 1,
        /// <summary>SL</summary>
        [EnumMember]
        SL = 2,
        /// <summary>BVT</summary>
        [EnumMember]
        BVT = 3,
        /// <summary>Коммутация</summary>
        [EnumMember]
        Commutation = 4,
        /// <summary>Пресс</summary>
        [EnumMember]
        Clamping = 5,
        /// <summary>dVdt</summary>
        [EnumMember]
        dVdt = 6,
        /// <summary>ATU</summary>
        [EnumMember]
        ATU = 8,
        /// <summary>QrrTq</summary>
        [EnumMember]
        QrrTq = 9,
        /// <summary>TOU</summary>
        [EnumMember]
        TOU = 13,

        [EnumMember]
        Sctu = 7,
        [EnumMember]
        IH = 11,
        [EnumMember]
        RCC = 12
    }

    [KnownType(typeof(GTUTestParameters))]
    [KnownType(typeof(SLTestParameters))]
    [KnownType(typeof(BVTTestParameters))]
    [KnownType(typeof(dVdtParameters))]
    [KnownType(typeof(ATUParameters))]
    [KnownType(typeof(QrrTqParameters))]
    [KnownType(typeof(TOUParameters))]
    /// <summary>Данные о параметрах тестирования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public abstract class BaseTestParametersAndNormatives
    {
        /// <summary>Тип параметра</summary>
        [DataMember]
        public TestParametersType TestParametersType
        {
            get; set;
        }

        /// <summary>Порядок</summary>
        [DataMember]
        public int Order
        {
            get; set;
        }

        /// <summary>Активация</summary>
        [DataMember]
        public bool IsEnabled
        {
            get; set;
        }

        /// <summary>Id тестирования</summary>
        [DataMember]
        public long TestTypeId
        {
            get; set;
        }

        /// <summary>Проверка изменений в параметрах</summary>
        /// <param name="oldParameters">Старые параметры</param>
        /// <returns>Возвращает True, если параметры были изменены</returns>
        public abstract bool HasChanges(BaseTestParametersAndNormatives oldParameters);

        /// <summary>Создание параметра по типу</summary>
        /// <param name="type">Тип параметра</param>
        /// <returns>Новый параметр с заданным типом</returns>
        public static BaseTestParametersAndNormatives CreateParametersByType(TestParametersType type)
        {
            switch (type)
            {
                case TestParametersType.GTU:
                    return new GTUTestParameters();
                case TestParametersType.BVT:
                    return new BVTTestParameters();
                case TestParametersType.SL:
                    return new SLTestParameters();
                case TestParametersType.dVdt:
                    return new dVdtParameters();
                case TestParametersType.ATU:
                    return new ATUParameters();
                case TestParametersType.QrrTq:
                    return new QrrTqParameters();
                case TestParametersType.TOU:
                    return new TOUParameters();
                default:
                    throw new NotImplementedException("CreateParametersByType");
            }
        }
    }

    /// <summary>Результат тестирования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public abstract class BaseTestResults
    {
        /// <summary>Id тестирования</summary>
        [DataMember]
        public long TestTypeId
        {
            get; set;
        }
    }
}
