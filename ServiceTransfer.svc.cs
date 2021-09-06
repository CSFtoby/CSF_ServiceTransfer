using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CSF_ServiceTransfer
{
    public class ServiceTransfer : IServiceTransfer
    {
        public string GetServiceResponse(string XmlRequest)
        {
            ClsCore cre = new ClsCore();
            return cre.porcessRequest(XmlRequest);
        }
    }
}
