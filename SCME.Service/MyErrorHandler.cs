using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace SCME.Service
{
    public class MyErrorHandler : IErrorHandler  
    {  
        public bool HandleError(Exception error)  
        {  
            return error is FaultException && (error.InnerException as SerializationException != null);  
        }  
 
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)  
        {  
            if (error is FaultException)  
            {  
                SerializationException serException = error.InnerException as SerializationException;  
                if (serException != null)  
                {  
                    string detail = String.Format("{0}: {1}", serException.GetType().FullName, serException.Message);  
                    FaultException<string> faultException = new FaultException<string>(detail, new FaultReason("SerializationException caught while deserializing parameter from the client"));  
                    MessageFault messageFault = faultException.CreateMessageFault();  
                    //fault = Message.CreateMessage(version, messageFault, Dbs.FaultAction);  
                }  
            }  
        }  
    }  
}