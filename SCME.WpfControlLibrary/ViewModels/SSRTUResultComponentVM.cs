using PropertyChanged;
using SCME.Types;
using SCME.Types.SSRTU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class SSRTUResultComponentVM
    {
        /*public bool IsEmpty => LeakageCurrent == null && InputAmperage == null && InputVoltage == null && ResidualVoltage == null && ProhibitionVoltage == null
            && AuxiliaryCurrentPowerSupply1 == null && AuxiliaryCurrentPowerSupply2 == null && OpenResistance == null;

        public bool IsGood => (LeakageCurrentIsOk ?? true) && (InputAmperageIsOk ?? true) && (InputVoltageIsOk ?? true) && (ResidualVoltageIsOk ?? true) && (ProhibitionVoltageIsOk ?? true)
            && (AuxiliaryCurrentPowerSupply1IsOk ?? true) && (AuxiliaryCurrentPowerSupply1IsOk ?? true) && (OpenResistanceIsOk ?? true);*/

        public string ErrorCode { get; set; }
        public bool IsEmpty => LeakageCurrentsIsEmpty && InputAmperagesIsEmpty && InputVoltagesIsEmpty && ResidualVoltagesIsEmpty && AuxiliaryCurrentPowerSupply1 == null && AuxiliaryCurrentPowerSupply2 == null;
        public bool IsGood => LeakageCurrentsIsGood && InputAmperagesIsGood && InputVoltagesIsGood && ResidualVoltagesIsGood && (AuxiliaryCurrentPowerSupply1IsOk ?? true) && (AuxiliaryCurrentPowerSupply1IsOk ?? true);


        //public bool IsGood => (LeakageCurrentMin == null || (LeakageCurrentMin != null && LeakageCurrentIsOk.Value)) && 
        //    (InputAmperageMin != null && InputAmperageIsOk.Value) && 
        //    (InputVoltageMin != null && InputVoltageIsOk.Value) && 
        //    (ResidualVoltageMin != null && ResidualVoltageIsOk.Value) && 
        //    (ProhibitionVoltageMin != null && ProhibitionVoltageIsOk.Value) &&
        //    (AuxiliaryCurrentPowerSupplyMin1 != null && AuxiliaryCurrentPowerSupply1IsOk.Value) && 
        //    (AuxiliaryCurrentPowerSupplyMin2 != null && AuxiliaryCurrentPowerSupply2IsOk.Value) && 
        //    (OpenResistanceMin != null && OpenResistanceIsOk.Value);

        //public CommonResult ToCommonResult()
        //{
        //    return CommonResult()
        //}


        public SSRTUResultComponentVM()
        {
            LeakageCurrents = new List<Result>()
            {
                LeakageCurrent1,
                LeakageCurrent2,
                LeakageCurrent3
            };
            ResidualVoltages = new List<ResultResidualVoltage>()
            {
                ResidualVoltage1,
                ResidualVoltage2
            };
            InputAmperages = new List<Result>()
            {
                InputAmperage1,
                InputAmperage2,
                InputAmperage3,
                InputAmperage4,
            };
            InputVoltages = new List<Result>()
            {
                InputVoltage1,
                InputVoltage2,
                InputVoltage3,
                InputVoltage4
            };
        }

        public int Positition { get; set; }
        public DutPackageType DutPackageType { get; set; }
        public int SerialNumber { get; set; }

        public bool ShowAuxiliaryCurrentPowerSupply1 => DutPackageType == DutPackageType.B5 || DutPackageType == DutPackageType.V108;

        public bool ShowAuxiliaryCurrentPowerSupply2 => DutPackageType == DutPackageType.V108;

        public bool ShowInputAmperage { get; set; }
        [DependsOn(nameof(ShowInputAmperage))]
        public bool ShowInputVoltage => !ShowInputAmperage;


        [AddINotifyPropertyChangedInterface]
        public class Result
        {
            public int Index { get; set; }
            public Result(int index)
            {
                Index = index;
            }

            public double? Value { get; set; }
            public double? Min { get; set; }
            public double? Max { get; set; }

            [DependsOn(nameof(Value))]
            public bool IsEmpty => Value == null;
            [DependsOn(nameof(Value))]
            public bool IsGood => (IsOk ?? true);


            [DependsOn(nameof(Value), nameof(Min), nameof(Max))]
            public bool? IsOk => Min == null || Value?.CompareTo(double.Epsilon) == 0 ? (bool?)null : (Min <= Value && Value < Max);
        }

        [AddINotifyPropertyChangedInterface]
        public class ResultResidualVoltage : Result
        {
            public ResultResidualVoltage(int index) : base(index)
            {
            }

            public double? ValueEx { get; set; }
            public double? MinEx { get; set; }
            public double? MaxEx { get; set; }

            [DependsOn(nameof(MinEx))]
            public bool UseEx => MinEx != null;
            [DependsOn(nameof(ValueEx))]
            public bool IsEmptyEx => ValueEx == null;

            [DependsOn(nameof(ValueEx), nameof(MinEx), nameof(MaxEx))]
            public bool? IsOkEx => MinEx == null || ValueEx?.CompareTo(double.Epsilon) == 0 ? (bool?)null : (MinEx < ValueEx && ValueEx < MaxEx);
        }



        public Result LeakageCurrent1 { get; set; } = new Result(1);
        public Result LeakageCurrent2 { get; set; } = new Result(2);
        public Result LeakageCurrent3 { get; set; } = new Result(3);
        public List<Result> LeakageCurrents { get; set; }


        public ResultResidualVoltage ResidualVoltage1 { get; set; } = new ResultResidualVoltage(1);
        public ResultResidualVoltage ResidualVoltage2 { get; set; } = new ResultResidualVoltage(2);
        public List<ResultResidualVoltage> ResidualVoltages { get; set; }



        public Result InputAmperage1 { get; set; } = new Result(1);
        public Result InputAmperage2 { get; set; } = new Result(2);
        public Result InputAmperage3 { get; set; } = new Result(3);
        public Result InputAmperage4 { get; set; } = new Result(4);
        public List<Result> InputAmperages { get; set; }



        public Result InputVoltage1 { get; set; } = new Result(1);
        public Result InputVoltage2 { get; set; } = new Result(2);
        public Result InputVoltage3 { get; set; } = new Result(3);
        public Result InputVoltage4 { get; set; } = new Result(4);
        public List<Result> InputVoltages { get; set; }



        public bool LeakageCurrentsIsEmpty => LeakageCurrents.FirstOrDefault(m => !m.IsEmpty) == null;
        public bool ResidualVoltagesIsEmpty => ResidualVoltages.FirstOrDefault(m => !m.IsEmpty) == null && ResidualVoltages.FirstOrDefault(m => !m.IsEmpty) == null;
        public bool InputAmperagesIsEmpty => InputAmperages.FirstOrDefault(m => !m.IsEmpty) == null;
        public bool InputVoltagesIsEmpty => InputVoltages.FirstOrDefault(m => !m.IsEmpty) == null;


        public bool LeakageCurrentsIsGood => LeakageCurrents.FirstOrDefault(m => !m.IsGood) == null;
        public bool ResidualVoltagesIsGood => ResidualVoltages.FirstOrDefault(m => !m.IsGood) == null && ResidualVoltages.FirstOrDefault(m => !m.IsGood) == null;
        public bool InputAmperagesIsGood => InputAmperages.FirstOrDefault(m => !m.IsGood) == null;
        public bool InputVoltagesIsGood => InputVoltages.FirstOrDefault(m => !m.IsGood) == null;




        public double? AuxiliaryCurrentPowerSupply1 { get; set; }
        public double? AuxiliaryCurrentPowerSupply2 { get; set; }


        public double? AuxiliaryCurrentPowerSupplyMin1 { get; set; }
        public double? AuxiliaryCurrentPowerSupplyMin2 { get; set; }



        public double? AuxiliaryCurrentPowerSupplyMax1 { get; set; }
        public double? AuxiliaryCurrentPowerSupplyMax2 { get; set; }



        [DependsOn(nameof(AuxiliaryCurrentPowerSupply1), nameof(AuxiliaryCurrentPowerSupplyMin1), nameof(AuxiliaryCurrentPowerSupplyMax1))]
        public bool? AuxiliaryCurrentPowerSupply1IsOk => AuxiliaryCurrentPowerSupplyMin1 == null || AuxiliaryCurrentPowerSupply1?.CompareTo(double.Epsilon) == 0 ? (bool?)null : AuxiliaryCurrentPowerSupplyMin1 < AuxiliaryCurrentPowerSupply1 && AuxiliaryCurrentPowerSupply1 < AuxiliaryCurrentPowerSupplyMax1;


        [DependsOn(nameof(AuxiliaryCurrentPowerSupply2), nameof(AuxiliaryCurrentPowerSupplyMin2), nameof(AuxiliaryCurrentPowerSupplyMax2))]
        public bool? AuxiliaryCurrentPowerSupply2IsOk => AuxiliaryCurrentPowerSupplyMin2 == null || AuxiliaryCurrentPowerSupply2?.CompareTo(double.Epsilon) == 0 ? (bool?)null : AuxiliaryCurrentPowerSupplyMin2 < AuxiliaryCurrentPowerSupply2 && AuxiliaryCurrentPowerSupply2 < AuxiliaryCurrentPowerSupplyMax2;

        public bool? HaveError
        {
            get
            {
                if (ErrorCode == null)
                    return null;
                else
                    return string.IsNullOrEmpty(ErrorCode);
            }
        }
    }
}
