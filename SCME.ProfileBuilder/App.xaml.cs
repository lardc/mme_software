using MahApps.Metro;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using Application = System.Windows.Application;
using Point = System.Drawing.Point;

namespace SCME.ProfileBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var (_, DPIY) = GetSystemDpi();

            //Полтора сантиметра высота кнопки
            //WpfControlLibrary.ViewModels.ResourceBinding.HeightButton = DPIY / 2.54 * 4;
            ////Сантиметр высота шрифта
            //WpfControlLibrary.ViewModels.ResourceBinding.FontSize = DPIY / 2.54;
        }
    }
}
