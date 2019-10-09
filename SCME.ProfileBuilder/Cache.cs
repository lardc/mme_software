using SCME.ProfileBuilder.Pages;
using SCME.WpfControlLibrary.CustomControls;

namespace SCME.ProfileBuilder
{
    public static class Cache
    {
        public static ProfilesPage ProfilesPage { get; set; }
        internal static MainWindow Main { get; set; }

        internal static ConnectPage ConnectPage { get; set; }

    }
}
