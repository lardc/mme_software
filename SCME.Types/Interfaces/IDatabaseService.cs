using System.Data;

namespace SCME.Types.Interfaces
{
    public interface IDatabaseService
    {
        void ImportProfiles(string filePath);

        bool Migrate();

        void Open();
        void Close();
        void ResetContent();
        ConnectionState State { get; set; }
    }
}
