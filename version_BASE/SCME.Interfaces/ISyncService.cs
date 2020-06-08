using System;

namespace SCME.Interfaces
{
    public interface ISyncService : IDisposable
    {
        /// <summary>
        /// Send local results to central db
        /// </summary>
        void SyncResults();

        /// <summary>
        /// Refresh local profiles from central db
        /// </summary>
        void SyncProfiles();
    }
}
