using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace SCME.Types
{
    public class DatabaseMaintenanceProxy : ClientBase<IDatabaseMaintenanceService>, IDatabaseMaintenanceService
    {
        public DatabaseMaintenanceProxy(string ServerEndpointConfigurationName)
            : base(ServerEndpointConfigurationName)
        {
        }

        public DatabaseMaintenanceProxy(string ServerEndpointConfigurationName, string RemoteAddress)
            : base(ServerEndpointConfigurationName, RemoteAddress)
        {
        }

        public void Check()
        {
            Channel.Check();
        }

        public void RemoveGroup(string GroupName)
        {
            Channel.RemoveGroup(GroupName);
        }

        public List<GeneralTableRecord> GetTableData(string TableName, long LastID, int TransferSize)
        {
            return Channel.GetTableData(TableName, LastID, TransferSize);
        }
    }
}