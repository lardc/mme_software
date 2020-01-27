using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace SCME.Types
{
    public static class WcfClientBindings
    {
        public static NetTcpBinding DefaultNetTcpBinding => new NetTcpBinding()
        {
            ReceiveTimeout = new TimeSpan(0, 8, 0, 0),
            SendTimeout = new TimeSpan(0, 8, 0, 0),
            MaxBufferPoolSize = 2147483647,
            MaxBufferSize = 2147483647,
            MaxReceivedMessageSize = 2147483647,
            Security = new NetTcpSecurity()
            {
                Mode = SecurityMode.None,
                Transport = new TcpTransportSecurity()
                {
                    ClientCredentialType = TcpClientCredentialType.None
                },
                Message = new MessageSecurityOverTcp()
                {
                    ClientCredentialType = MessageCredentialType.None
                }
            }
        };
    }
}
