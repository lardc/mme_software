using SCME.ProfileBuilder.CommonPages;
using SCME.ProfileBuilder.PagesTech;

namespace SCME.ProfileBuilder
{
    public static class Cache
    {
        public static ProfilesPage ProfilesPage { get; set; }
        internal static MainWindow Main { get; set; }

        internal static ProfilePage ProfileEdit { get; set; }

        internal static ConnectPage ConnectPage { get; set; }

        internal static Connections ConnectionsPage { get; set; }
    }
}
