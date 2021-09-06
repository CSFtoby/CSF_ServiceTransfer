using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Data;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Oracle.ManagedDataAccess;

namespace CSF_ServiceTransfer
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract(Namespace = "CoreTransfer", Name = "IServiceTransfer")]

    public interface IServiceTransfer
    {

        [OperationContract]
        string GetServiceResponse(string XmlRequest);

    }
}
