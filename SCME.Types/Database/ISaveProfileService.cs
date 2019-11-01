using SCME.Types.Profiles;

namespace SCME.Types.Database
{
    public interface ISaveProfileService
    {
        int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode);
        void RemoveProfile(MyProfile profile);
    }
}
