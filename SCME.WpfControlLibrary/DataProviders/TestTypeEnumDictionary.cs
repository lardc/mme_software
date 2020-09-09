﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using SCME.Types.BaseTestParams;
using SCME.Types.BVT;
using SCME.Types.Clamping;
using SCME.Types.Commutation;
using SCME.Types.dVdt;
using SCME.Types.QrrTq;
using SCME.Types.VTM;
using SCME.WpfControlLibrary.Properties;
using Settings = SCME.UIServiceConfig.Properties.Settings;

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

        public static Dictionary<string, ModuleCommutationType> GetDataFromCommutationModeEnum()
        {
            return UIServiceConfig.Properties.Settings.Default.ClampingSystemType switch
            {
                ClampingSystemType.kN100 => new Dictionary<string, ModuleCommutationType>() {{nameof(ModuleCommutationType.Direct), ModuleCommutationType.Direct},},
                ClampingSystemType.Studs => new Dictionary<string, ModuleCommutationType>() {{nameof(ModuleCommutationType.Direct), ModuleCommutationType.Direct}, {nameof(ModuleCommutationType.Reverse), ModuleCommutationType.Reverse},},
                ClampingSystemType.Modules => new Dictionary<string, ModuleCommutationType>()
                {
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
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static Visibility GetVisibilityHeightForce()
        {
            return Settings.Default.ClampingSystemType == ClampingSystemType.kN100 ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility GetVisibilityTopTemp()
        {
            return Settings.Default.ClampingSystemType == ClampingSystemType.Modules ? Visibility.Collapsed: Visibility.Visible;
        }

       

        public static Dictionary<string, ClampingForce> GetClampingForceTypes()
        {
            return new Dictionary<string, ClampingForce>()
            {
                {nameof(ClampingForce.Contact), ClampingForce.Contact},
                {nameof(ClampingForce.Custom), ClampingForce.Custom},
            };
        }

        public static List<TestParameter> GetTestParametersTypes()
        {
            return new List<TestParameter>()
            {
                new TestParameter(TestParametersType.Gate, typeof(Types.Gate.TestParameters), "GTU"),
                new TestParameter(TestParametersType.StaticLoses, typeof(Types.VTM.TestParameters), "SL"),
                new TestParameter(TestParametersType.Bvt, typeof(Types.BVT.TestParameters), "BVT"),
                new TestParameter(TestParametersType.Dvdt, typeof(Types.dVdt.TestParameters), "dVdt"),
                new TestParameter(TestParametersType.ATU, typeof(Types.ATU.TestParameters), "ATU"),
                new TestParameter(TestParametersType.QrrTq, typeof(Types.QrrTq.TestParameters), "QrrTq"),
                new TestParameter(TestParametersType.TOU, typeof(Types.TOU.TestParameters), "TOU"),
            };
        }

        public static double MeasureString(string candidate)
        {
            var fontFamily = ((FontFamily) Application.Current.Resources["SCME.DefaultFont"]).FamilyNames.First().Value;

            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily),
                (double) Application.Current.Resources["SCME.BaseFontSize"],
                Brushes.Black,
                new NumberSubstitution());

            return formattedText.Width;
        }

        public static Dictionary<ushort, ushort> GetFrequencyDivisors()
        {
            return new Dictionary<ushort, ushort>()
            {
                {50, 1},
                {25, 2},
                {10, 5},
                {5, 10},
                {2, 25},
                {1, 50},
            };
        }
        
        public static List<ushort> GetVoltageFrequency()
        {
            return new List<ushort>()
            {
                50,60
            };
        }

        public static string GetTopTempName()
        {
            return Settings.Default.ClampingSystemType == ClampingSystemType.Studs ? Resources.Housing : Resources.TopTempName;
        }
        
        public static string GetBottomTempName()
        {
            return Settings.Default.ClampingSystemType == ClampingSystemType.Studs ? Resources.Output : Resources.BotTempName;
        }
        
    }
}