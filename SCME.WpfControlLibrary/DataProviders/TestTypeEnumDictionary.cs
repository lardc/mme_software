using System;
using System.Collections.Generic;
using SCME.Types.BaseTestParams;
using SCME.Types.BVT;
using SCME.Types.Commutation;
using SCME.Types.dVdt;
using SCME.Types.QrrTq;
using SCME.Types.VTM;
using SCME.WpfControlLibrary.Properties;
// ReSharper disable UnusedMethodReturnValue.Global

namespace SCME.WpfControlLibrary.DataProviders
{
    public static class TestTypeEnumDictionary
    {
        public static Dictionary<string, BVTTestType> GetBVTTestTypes()
        {
            return new Dictionary<string, BVTTestType>()
            {
                {Resources.Both, BVTTestType.Both},
                {Resources.Direct, BVTTestType.Direct},
                {Resources.Reverse, BVTTestType.Reverse},
            };
        }
        
        public static Dictionary<string, VTMTestType> GetVTMTestTypes()
        {
            return new Dictionary<string, VTMTestType>()
            {
                {Resources.Sinus, VTMTestType.Sinus},
                {Resources.Ramp, VTMTestType.Ramp},
                {Resources.Curve, VTMTestType.Curve},
            };
        }
        
        public static Dictionary<string, BVTMeasurementMode> GetBVTMeasurementModes()
        {
            return new Dictionary<string, BVTMeasurementMode>()
            {
                {BVTMeasurementMode.ModeI.ToString(), BVTMeasurementMode.ModeI},
                {BVTMeasurementMode.ModeV.ToString(), BVTMeasurementMode.ModeV},
            };
        }
        
        public static Dictionary<string, DvdtMode> GetDvdtModes()
        {
            return new Dictionary<string, DvdtMode>()
            {
                {Resources.DvdtConfirmation, DvdtMode.Confirmation},
                {Resources.DvdtDetection, DvdtMode.Detection},
            };
        }
        
        public static Dictionary<string, VoltageRate> GetVoltageRates()
        {
            return new Dictionary<string, VoltageRate>()
            {
                {nameof(VoltageRate.V500), VoltageRate.V500},
                {nameof(VoltageRate.V1000), VoltageRate.V1000},
                {nameof(VoltageRate.V1600), VoltageRate.V1600},
                {nameof(VoltageRate.V2000), VoltageRate.V2000},
                {nameof(VoltageRate.V2500), VoltageRate.V2500},
            };
        }

        public static Dictionary<string, TMode> GetTModes()
        {
            return new Dictionary<string, TMode>()
            {
                {nameof(TMode.Qrr), TMode.Qrr},
                {nameof(TMode.QrrTq), TMode.QrrTq}
            };
        }

        public static Dictionary<string, TDcFallRate> GetTDcFallRates()
        {
            return new Dictionary<string, TDcFallRate>()
            {
                {nameof(TDcFallRate.r2), TDcFallRate.r2},
                {nameof(TDcFallRate.r5), TDcFallRate.r5},
                {nameof(TDcFallRate.r10), TDcFallRate.r10},
                {nameof(TDcFallRate.r15), TDcFallRate.r15},
                {nameof(TDcFallRate.r20), TDcFallRate.r20},
                {nameof(TDcFallRate.r30), TDcFallRate.r30},
                {nameof(TDcFallRate.r50), TDcFallRate.r50},
                {nameof(TDcFallRate.r60), TDcFallRate.r60},
                {nameof(TDcFallRate.r100), TDcFallRate.r100},
            };
        }
        
        
        public static Dictionary<string, TOsvRate> GetTOsvRates()
        {
            return new Dictionary<string, TOsvRate>()
            {
                {nameof(TOsvRate.r20), TOsvRate.r20},
                {nameof(TOsvRate.r50), TOsvRate.r50},
                {nameof(TOsvRate.r100), TOsvRate.r100},
                {nameof(TOsvRate.r200), TOsvRate.r200},
            };
        }

        public static Dictionary<string, ModuleCommutationType> GetModuleCommutationTypes()
        {
            return new Dictionary<string, ModuleCommutationType>()
            {
                {nameof(ModuleCommutationType.Direct), ModuleCommutationType.Direct},
                {nameof(ModuleCommutationType.Reverse), ModuleCommutationType.Reverse},
                {nameof(ModuleCommutationType.MD1), ModuleCommutationType.MD1},
                {nameof(ModuleCommutationType.MD3), ModuleCommutationType.MD3},
                {nameof(ModuleCommutationType.MD4), ModuleCommutationType.MD4},
                {nameof(ModuleCommutationType.MD5), ModuleCommutationType.MD5},
                {nameof(ModuleCommutationType.MT1), ModuleCommutationType.MT1},
                {nameof(ModuleCommutationType.MT3), ModuleCommutationType.MT3},
                {nameof(ModuleCommutationType.MT4), ModuleCommutationType.MT4},
                {nameof(ModuleCommutationType.MT5), ModuleCommutationType.MT5},
                {nameof(ModuleCommutationType.MDT3), ModuleCommutationType.MDT3},
                {nameof(ModuleCommutationType.MDT4), ModuleCommutationType.MDT4},
                {nameof(ModuleCommutationType.MDT5), ModuleCommutationType.MDT5},
                {nameof(ModuleCommutationType.MTD3), ModuleCommutationType.MTD3},
                {nameof(ModuleCommutationType.MTD4), ModuleCommutationType.MTD4},
                {nameof(ModuleCommutationType.MTD5), ModuleCommutationType.MTD5},
            };
        }
        
        public static Dictionary<string, TestParametersType> GetTestParametersTypes()
        {
            return new Dictionary<string, TestParametersType>()
            {
               {nameof(TestParametersType.Gate), TestParametersType.Gate},
               {nameof(TestParametersType.StaticLoses), TestParametersType.StaticLoses},
               {nameof(TestParametersType.Bvt), TestParametersType.Bvt},
               {nameof(TestParametersType.Dvdt), TestParametersType.Dvdt},
               {nameof(TestParametersType.ATU), TestParametersType.ATU},
               {nameof(TestParametersType.QrrTq), TestParametersType.QrrTq},
               {nameof(TestParametersType.RAC), TestParametersType.RAC},
               {nameof(TestParametersType.TOU), TestParametersType.TOU},
            };
        }
        
    }
}