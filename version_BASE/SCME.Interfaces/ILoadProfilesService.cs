using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SCME.Types;

namespace SCME.Interfaces
{
    public interface ILoadProfilesService
    {
        List<ProfileItem> GetProfileItems();
        List<ProfileItem> GetProfileItems(string mmeCode);
    }
}
