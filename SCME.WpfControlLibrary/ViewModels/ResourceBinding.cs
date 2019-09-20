using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro;
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
        }

        public static void Scaling(double factor = 1)
        {
            var (_, DPIY) = GetSystemDpi();
            double cmFactor = DPIY / 2.54 * factor;
            
            Application.Current.Resources["SCME.BaseFontSize"] = cmFactor;
            Application.Current.Resources["SCME.ScrollBarWidth"] = cmFactor;
            Application.Current.Resources["SCME.SizeButtonWithIcon"] = 1/.5 * cmFactor;
            
            Application.Current.Resources["SCME.CheckBoxBorderSize"] = cmFactor;
            Application.Current.Resources["SCME.CheckBoxPathWidth"] = cmFactor / 18 * 12;
            Application.Current.Resources["SCME.CheckBoxPathHeight"] = cmFactor / 18 * 10;
            
            Application.Current.Resources["SCME.RepeatButtonUpDownSize"] = cmFactor * 1.5;
            Application.Current.Resources["SCME.PathUpDownWidth"] = (double)Application.Current.Resources["SCME.RepeatButtonUpDownSize"] * 14 / 22;
            Application.Current.Resources["SCME.PathDownHeight"] = (double)Application.Current.Resources["SCME.RepeatButtonUpDownSize"] * 3 / 22;
            //Application.Current.Resources["SCME.RepeatButtonUpDownMarginLeft"] = new Thickness(cmFactor * 0.1, 0, 0, 0);
            //Application.Current.Resources["SCME.RepeatButtonDownSize"] = (double)Application.Current.Resources["SCME.RepeatButtonUpSize"] / 14 * 3;
            
            Application.Current.Resources["SCME.RadioButtonEllipseNormalSize"] = cmFactor;
            Application.Current.Resources["SCME.RadioButtonEllipseCheckedSize"] = cmFactor * 10 / 18;
            
            Application.Current.Resources["SCME.EllipseSize"] = cmFactor * 2;
            Application.Current.Resources["SCME.MarginLeftTreeViewItemDataProfile"] = new Thickness(0.5 * cmFactor, 0, 0, 0);
        }



    }
}
