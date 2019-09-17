using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Drawing.Point;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public static class ResourceBinding
    {
        private static readonly int LOGPIXELSX = 88;    // Used for GetDeviceCaps().
        private static readonly int LOGPIXELSY = 90;    // Used for GetDeviceCaps().

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        public static (int DPIX, int DPIY) GetSystemDpi()
        {
            Point result = new Point();

            IntPtr hDC = GetDC(IntPtr.Zero);

            result.X = GetDeviceCaps(hDC, LOGPIXELSX);
            result.Y = GetDeviceCaps(hDC, LOGPIXELSY);

            ReleaseDC(IntPtr.Zero, hDC);

            return (result.X, result.Y);
        }

        static ResourceBinding()
        {
            var (_, DPIY) = GetSystemDpi();
            double cm = DPIY / 2.54;

            TopBottomMargin = new Thickness(0, cm * 0.3, 0, cm * 0.3);
            BottomMargin = new Thickness(0, 0, 0, cm * 0.3);
            TopMargin = new Thickness(0, cm * 0.3, 0, 0);

            ScrollBarWidth = cm;
            SizeButtonWithIcon = cm * 1.5;


            RepeatButtonUpSize = cm * 1.5;
            RepeatButtonUpDownMarginLeft = cm * 0.1;
            //Изначальное соотношение высоты ширины для минуса в стилях
            RepeatButtonDownSize = RepeatButtonUpSize / 14 * 3;

            CheckBoxBorderSize = cm;
            CheckBoxPathWidth = cm / 18 * 12;
            CheckBoxPathHeight = cm / 18 * 10;


            FontSize = cm;
            EllipseSize = cm * 2;

            MarginLeft05 = new Thickness(0.5 * cm, 0, 0, 0);
        }

        public static double FontSize { get; set; }
        public static double SizeButtonWithIcon { get; set; }


        //CheckBox


        //NumericUpDown
        public static double RepeatButtonUpSize { get; set; }
        public static double RepeatButtonDownSize { get; set; }
        public static double RepeatButtonUpDownMarginLeft { get; set; }

        public static double CheckBoxBorderSize { get; set; }
        public static double CheckBoxPathWidth { get; set; }
        public static double CheckBoxPathHeight { get; set; }


        public static double ScrollBarWidth { get; set; }
        public static double EllipseSize { get; set; }

        public static Thickness TopBottomMargin { get; set; }
        public static Thickness BottomMargin { get; set; }
        public static Thickness TopMargin { get; set; }

        public static Thickness MarginLeft05 { get; set; }


    }
}
