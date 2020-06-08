using System.Data;
using SCME.Types;

namespace SCME.Interfaces
{
    public interface IDatabaseService
    {
        void ImportProfiles(string filePath);

        void Open();
        void Close();
        void ResetContent();
        ConnectionState State { get; set; }
    }
}
