using System;
using System.ServiceModel;
using System.Windows;
using SCME.Types;

namespace SCME.ProfileManager
{
    internal static class SystemHost
    {
        private const string PROFILE_SERVER_ENDPOINT_NAME = "SCME.ProfileService";

        internal static ProfileServiceProxy ProfileClient { get; private set; }

        internal static bool Initialize()
        {
            try
            {
                ProfileClient = new ProfileServiceProxy(PROFILE_SERVER_ENDPOINT_NAME);
                ProfileClient.Open();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Попытка подключения к серверу профилей завершилась неудачей: {0}", ex.Message),
                    "Ошибка соединения",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ProfileClient.Abort();
                return false;
            }
        }

        public static void Close()
        {
            if (ProfileClient != null)
            {
                try
                {
                    if (ProfileClient.State == CommunicationState.Opened)
                        ProfileClient.Close();
                    else
                        ProfileClient.Abort();
                }
                catch 
                {
                }
            }
        }
    }
}
