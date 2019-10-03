using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using SCME.InterfaceImplementations.Common;
using SCME.InterfaceImplementations.Common.DbService;

namespace SCME.Service
{
    public class MyServiceBehavior : IServiceBehavior  
    {  
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)  
        {  
        }  
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)  
        {  
            foreach (ChannelDispatcher disp in serviceHostBase.ChannelDispatchers)  
            {  
                disp.ErrorHandlers.Add(new MyErrorHandler());  
            }  
        }  
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)  
        {  
            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)  
            {  
                if (endpoint.Contract.ContractType == typeof(IMetadataExchange)) continue;  
                foreach (OperationDescription operation in endpoint.Contract.Operations)  
                {  
                    FaultDescription expectedFault = operation.Faults.Find(DbService<SqlCommand, SqlConnection >.FaultAction);

                }  
            }  
        }  
    }  
}