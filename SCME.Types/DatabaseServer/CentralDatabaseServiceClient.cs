using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using SCME.Types.DataContracts;
using SCME.Types.SQL;

namespace SCME.Types.DatabaseServer
{
    public class CentralDatabaseServiceClient : ClientBase<ICentralDatabaseService>, ICentralDatabaseService, IErrorHandler
    {
        public void Check()
        {
            Channel.Check();
        }

        public void SaveResults(ResultItem results, List<string> errors)
        {
            Channel.SaveResults(results, errors);
        }

        public List<ProfileItem> GetProfileItems()
        {
            return Channel.GetProfileItems();
        }

        public List<ProfileItem> GetProfileItemsByMme(string mmeCode)
        {
            return Channel.GetProfileItemsByMme(mmeCode);
        }

        public ProfileItem GetProfileByProfName(string profName, string mmmeCode, ref bool Found)
        {
            return Channel.GetProfileByProfName(profName, mmmeCode, ref Found);
        }

        public List<ProfileForSqlSelect> SaveProfiles(List<ProfileItem> profileItems)
        {
            return Channel.SaveProfiles(profileItems);
        }

        public List<ProfileForSqlSelect> SaveProfilesFromMme(List<ProfileItem> profileItems, string mmeCode)
        {
            return Channel.SaveProfilesFromMme(profileItems, mmeCode);
        }

        public List<string> GetGroups(DateTime? @from, DateTime? to)
        {
            return Channel.GetGroups(from, to);
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {

        }

        public bool HandleError(Exception error)
        {
            return true;
        }

        public List<DeviceItem> GetDevices(string @group)
        {
            return Channel.GetDevices(@group);
        }

        public List<int> ReadDeviceErrors(long internalId)
        {
            return Channel.ReadDeviceErrors(internalId);
        }

        public List<ParameterItem> ReadDeviceParameters(long internalId)
        {
            return Channel.ReadDeviceParameters(internalId);
        }

        public List<ConditionItem> ReadDeviceConditions(long internalId)
        {
            return Channel.ReadDeviceConditions(internalId);
        }

        public List<ParameterNormativeItem> ReadDeviceNormatives(long internalId)
        {
            return Channel.ReadDeviceNormatives(internalId);
        }

        public bool SendResultToServer(DeviceLocalItem localDevice)
        {
            return Channel.SendResultToServer(localDevice);
        }

        public IEnumerable<MmeCode> GetMmeCodes()
        {
            return Channel.GetMmeCodes();
        }

        public IEnumerable<ProfileMme> GetMmeProfiles(long mmeCodeId)
        {
            return Channel.GetMmeProfiles(mmeCodeId);
        }

        public void SaveConnections(List<MmeCode> mmeCodes)
        {
            Channel.SaveConnections(mmeCodes);
        }

    }
}
