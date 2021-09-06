using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Web.Hosting;
using System.Data;
using System.Web.Services.Protocols;
using System.Net;

namespace CSF_ServiceTransfer
{
    public class ClsCore
    {
        bool estado = false;
        bool error_interno = false;

        DataAccess da = new DataAccess();

        #region valida la transaccion
        private void Validate(ClsTransaction transaction, ref ClsStatus status)
        {
            try
            {
                XDocument xDocument = XDocument.Load(HostingEnvironment.MapPath("~/App_Data/Accounts.xml"));

                var Accouts = from iAccount in xDocument.Descendants("account_config").Descendants("account")
                              where iAccount.Element("number").Value.CompareTo(transaction.account_number) == 0
                              select new
                              {
                                  number = iAccount.Element("number").Value.ToString(),
                                  limit = iAccount.Element("limit").Value.ToString(),
                                  status = iAccount.Element("status").Value.ToString(),
                                  substatus_code = iAccount.Element("substatus_code").Value.ToString(),
                                  substatus_text = iAccount.Element("substatus_text").Value.ToString()
                              };

                if (Accouts.Count() > 0)
                {
                    var idat = Accouts.First();
                    status.StatusCode = idat.status;
                    status.SubStatusCode = idat.substatus_code;
                    status.SubStatusMessage = idat.substatus_text;
                }
            }
            catch (Exception ex)
            {
                status.StatusCode = "01000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "01001";
                status.SubStatusMessage = ex.Message;
            }
        }
        #endregion

        #region response
        void Response(ClsTransaction transaction, ClsStatus status, ref string xmlResponse, string transactionType = " ")
        {

            string oErr = string.Empty;
            string name = string.Empty;
            string last_name = string.Empty;
            string Account_Type = string.Empty;
            string state = string.Empty;
            string id = string.Empty;
            string id_type = string.Empty;
            string address = string.Empty;
            string country = string.Empty;
            string cucurrency = string.Empty;
            string city = string.Empty;
            string state_ = string.Empty;
            string phone = string.Empty;
            string birthday = string.Empty;
            string ocupation = string.Empty;
            string marital = string.Empty;
            char sex;
            string country_code = string.Empty;
            string fecha_nacimiento = string.Empty;
            string country_birth = string.Empty;
            string nationality = string.Empty;
            string status_account = string.Empty;

            try
            {
                if (transactionType.CompareTo("account-search-request") == 0)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(transaction.currency_code))
                        {
                            transaction.currency_code = System.Configuration.ConfigurationManager.AppSettings["CurrencyDefault"];
                        }
                    }
                    catch (Exception ex)
                    {
                        oErr = ex.Message;
                    }

                    if (estado == true)
                    {

                        DataTable daper = da.Personal_data(transaction.account_number);

                        foreach (DataRow personal in daper.Rows)
                        {
                            name = personal["FIRST_NAME"].ToString();
                        }

                        if (String.IsNullOrEmpty(name))
                        {
                            status.StatusCode = "01000";
                            status.StatusMessage = "Internal Error";
                            status.SubStatusCode = "00008";
                            status.SubStatusMessage = "Rejected by internal validation";

                            xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                <soapenv:Header/>
                                                <soapenv:Body>
                                                    <account-search-reply>
	                                                <account_details>
		                                                <account_number></account_number>
                                                        <first_name></first_name>
                                                        <last_name></last_name>
                                                        <account_type>SAVINGS</account_type>
		                                                <currency></currency>
                                                        <status_account></status_account>
	                                                </account_details>
                                                    <Personal_data>
                                                        <id_details>
                                                            <id_number></id_number>
                                                            <id_type></id_type>
                                                        </id_details>
                                                        <address>
                                                        <addr></addr>
                                                            <city></city>
                                                            <state></state>
                                                            <country_name></country_name>
                                                            <country_code></country_code>
                                                        </address>
                                                        <phone_number></phone_number>
                                                        <sex>M</sex>
                                                        <date_of_birth></date_of_birth> 
                                                        <country_of_birth></country_of_birth>
                                                        <nationality></nationality>
                                                        <occupation></occupation>
                                                        <marital_status></marital_status>
                                                    </Personal_data>
	                                                <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                <status>
		                                                <status_code>" + status.StatusCode + @"</status_code>
		                                                <status_description>" + status.StatusMessage + @"</status_description>
		                                                <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                </status>
                                                </account-search-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope> ";
                        }
                        else
                        {

                            foreach (DataRow personal in daper.Rows)
                            {
                                name = personal["FIRST_NAME"].ToString();
                                last_name = personal["LAST_NAME"].ToString();
                                if (personal["CUNCURRECY"].ToString().Equals("HN"))
                                {
                                    cucurrency = "HNL";
                                }
                                Account_Type = personal["ACCOUNT_TYPE"].ToString();
                                id = personal["ID_NUMBER"].ToString();
                                if (personal["ID_TYPE"].ToString() == "1")
                                {
                                    id_type = "B";
                                }
                                else
                                {
                                    id_type = "A";
                                }
                                address = personal["ADDR"].ToString();
                                city = personal["CITY"].ToString();
                                state_ = personal["STATE"].ToString();
                                country = personal["COUNTRY_NAME"].ToString();
                                country_code = personal["COUNTRY_CODE"].ToString();
                                phone = personal["PHONE_NUMBER"].ToString();
                                sex = Convert.ToChar(personal["SEX"].ToString());
                                fecha_nacimiento = personal["DATE_OF_BIRTHDAY"].ToString();
                                country_birth = personal["COUNTRY_OF_BIRTH"].ToString();
                                nationality = personal["NATIONALITY"].ToString();
                                ocupation = personal["OCCUPATION"].ToString();
                                marital = Convert.ToString(personal["MARITAL_STATUS"].ToString());

                                if (personal["STATUS_ACCOUN"].ToString() == "N")
                                {
                                    status_account = "ACTIVA";
                                }
                                else {
                                    status_account = "INACTIVO";
                                }

                            }

                            if (phone== "N/A" || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(ocupation)) {

                                status.StatusCode = "00000";
                                status.StatusMessage = "Rejected Request";
                                status.SubStatusCode = "00008";
                                status.SubStatusMessage = "Rejected by internal validation";

                                da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                da.insert_Error_bitacora(status.SubStatusMessage + " Falta numero de telefono", transaction.account_number, Convert.ToDecimal(transaction.destination_amount));

                                xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                            <soapenv:Header/>
                                            <soapenv:Body>
                                                <account-search-reply>
	                                                <account_details>
		                                                <account_number></account_number>
                                                        <first_name></first_name>
                                                        <last_name></last_name>
                                                        <account_type>SAVINGS</account_type>
		                                                <currency></currency>
                                                        <status_account></status_account>
	                                                </account_details>
                                                    <Personal_data>
                                                        <id_details>
                                                            <id_number></id_number>
                                                            <id_type></id_type>
                                                        </id_details>
                                                        <address>
                                                        <addr></addr>
                                                            <city></city>
                                                            <state></state>
                                                            <country_name></country_name>
                                                            <country_code></country_code>
                                                        </address>
                                                        <phone_number></phone_number>
                                                        <sex>M</sex>
                                                        <date_of_birth></date_of_birth> 
                                                        <country_of_birth></country_of_birth>
                                                        <nationality></nationality>
                                                        <occupation></occupation>
                                                        <marital_status></marital_status>
                                                    </Personal_data>
	                                                <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                <status>
		                                                <status_code>" + status.StatusCode + @"</status_code>
		                                                <status_description>" + status.StatusMessage + @"</status_description>
		                                                <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                </status>
                                                </account-search-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope>";

                            }else
                            {
                                xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                              <soapenv:Header/>
                                              <soapenv:Body>
                                                <account-search-reply>
	                                                <account_details>
		                                                <account_number>" + transaction.account_number + @"</account_number>
                                                        <first_name>" + name + @"</first_name>
                                                        <last_name>" + last_name + @"</last_name>
                                                        <account_type>SAVINGS</account_type>
		                                                <currency>" + cucurrency + @"</currency>
                                                        <status_account>" + status_account + @"</status_account>
	                                                </account_details>
                                                    <Personal_data>
                                                        <id_details>
                                                            <id_number>" + id + @"</id_number>
                                                            <id_type>" + id_type + @"</id_type>
                                                        </id_details>
                                                        <address>
                                                        <addr>" + address + @"</addr>
                                                            <city>" + city + @"</city>
                                                            <state>" + state_ + @"</state>
                                                            <country_name>" + country + @"</country_name>
                                                            <country_code>" + country_code + @"</country_code>
                                                        </address>
                                                        <phone_number>" + phone + @"</phone_number>
                                                        <sex>M</sex>
                                                        <date_of_birth>" + fecha_nacimiento + @"</date_of_birth> 
                                                        <country_of_birth>" + country_birth + @"</country_of_birth>
                                                        <nationality>" + country_code + @"</nationality>
                                                        <occupation>" + ocupation + @"</occupation>
                                                        <marital_status>" + marital + @"</marital_status>
                                                    </Personal_data>
	                                                <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                <status>
		                                                <status_code>" + status.StatusCode + @"</status_code>
		                                                <status_description>" + status.StatusMessage + @"</status_description>
		                                                <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                </status>
                                                </account-search-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope> ";
                            }
                        }
                    }
                    else {

                        xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                            <soapenv:Header/>
                                            <soapenv:Body>
                                                <account-search-reply>
	                                                <account_details>
		                                                <account_number></account_number>
                                                        <first_name></first_name>
                                                        <last_name></last_name>
                                                        <account_type>SAVINGS</account_type>
		                                                <currency></currency>
                                                        <status_account></status_account>
	                                                </account_details>
                                                    <Personal_data>
                                                        <id_details>
                                                            <id_number></id_number>
                                                            <id_type></id_type>
                                                        </id_details>
                                                        <address>
                                                        <addr></addr>
                                                            <city></city>
                                                            <state></state>
                                                            <country_name></country_name>
                                                            <country_code></country_code>
                                                        </address>
                                                        <phone_number></phone_number>
                                                        <sex>M</sex>
                                                        <date_of_birth></date_of_birth> 
                                                        <country_of_birth></country_of_birth>
                                                        <nationality></nationality>
                                                        <occupation></occupation>
                                                        <marital_status></marital_status>
                                                    </Personal_data>
	                                                <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                <status>
		                                                <status_code>" + status.StatusCode + @"</status_code>
		                                                <status_description>" + status.StatusMessage + @"</status_description>
		                                                <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                </status>
                                                </account-search-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope>";
                    }

                }
                else if (transactionType.CompareTo("payout-transfer-request") == 0)
                {
                    if (error_interno == false && status.StatusCode.Equals("50000") )
                    {
                        bool cuenta_valida = da.valida_cuenta(transaction.account_number);
                        bool exitoCredito = false;

                        if (cuenta_valida == true || status.StatusCode != "00000" || status.StatusCode != "01000")
                        {
                            decimal Factor = Convert.ToDecimal(da.Factor());
                            decimal monto = 0.0M;

                            if (transaction.currency_code == "HNL")
                            {
                                monto = (Convert.ToDecimal(transaction.destination_amount));
                            }
                            if (transaction.currency_code == "USD")
                            {
                                monto = (Convert.ToDecimal(transaction.destination_amount) * Factor);
                            }

                            monto = decimal.Round(monto, 2);

                            DataTable dt = da.Data_Transaction(transaction.account_number);
                            int codigo_agencia;
                            int codigo_empresa;
                            int codigo_sub_aplicacion;

                            if (dt.Rows.Count > 0)
                            {
                                codigo_agencia = Int32.Parse(dt.Rows[0]["CODIGO_AGENCIA"].ToString());
                                codigo_empresa = Int32.Parse(dt.Rows[0]["CODIGO_EMPRESA"].ToString());
                                codigo_sub_aplicacion = Int32.Parse(dt.Rows[0]["CODIGO_SUB_APLICACION"].ToString());

                                exitoCredito = da.inser_Datos_monto(codigo_empresa,
                                                    codigo_agencia,
                                                    codigo_sub_aplicacion,
                                                    transaction.account_number,
                                                    monto);

                                bool duplicado = da.valida_duplicidad(transaction.account_number, monto, DateTime.Now.ToString("yyyyMMdd"), transaction.reference_code);

                                if (duplicado == true)
                                {
                                    if (exitoCredito == true)
                                    {
                                        da.MCA_K_AHORROS(transaction.account_number);
                                        da.last_update(transaction.account_number, transaction.service, transaction.Bank_code, transaction.concept,
                                                        DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HH:mm:ss"), status.StatusCode, status.SubStatusCode,
                                                        transaction.additional_Info, transaction.reference_code);

                                        DataTable dataTrans = da.Trasnt_info(transaction.account_number);

                                        string reference_code = string.Empty;
                                        string internal_code = string.Empty;
                                        string cucurrencyTrans = string.Empty;
                                        string dateTrans = string.Empty;
                                        string timeTrans = string.Empty;

                                        foreach (DataRow Trans in dataTrans.Rows)
                                        {
                                            reference_code = Trans["REFERENCE_CODE"].ToString();
                                            internal_code = Trans["INTERNAL_REFERENCE_CODE"].ToString();
                                            cucurrencyTrans = Trans["CURRENCY_CODE"].ToString();
                                            dateTrans = Trans["TRANSACTION_DATE"].ToString();
                                            timeTrans = Trans["TRANSACTION_TIME"].ToString();
                                        }

                                        xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                <soapenv:Header/> 
                                                <soapenv:Body>
                                                    <payout-transfer-reply>
	                                                    <reference_code>" + reference_code + @"</reference_code>
	                                                    <internal_reference_code>" + internal_code + @"</internal_reference_code>
	                                                    <account_details>
		                                                    <account_number>" + transaction.account_number + @"</account_number>
		                                                     <account_type>SAVINGS</account_type>
		                                                    <currency>" + cucurrencyTrans + @"</currency>
	                                                    </account_details>
	                                                    <transaction_date>" + dateTrans + @"</transaction_date>
	                                                    <transaction_time>" + timeTrans + @"</transaction_time>
	                                                    <status>
		                                                    <status_code>" + status.StatusCode + @"</status_code>
		                                                    <status_description>" + status.StatusMessage + @"</status_description>
		                                                    <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                    <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                    </status>
                                                    </payout-transfer-reply>
                                                </soapenv:Body>
                                            </soapenv:Envelope> ";

                                    }
                                    else
                                    {
                                        status.StatusCode = "01000";
                                        status.StatusMessage = "Internal Error";
                                        status.SubStatusCode = "00008";
                                        status.SubStatusMessage = "Rejected by internal validation";

                                        da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                    }
                                }
                                else
                                {
                                    status.StatusCode = "00000";
                                    status.StatusMessage = "Rejected Request";
                                    status.SubStatusCode = "00001";
                                    status.SubStatusMessage = "Duplicate Transaction";

                                    da.inser_Error(transaction.account_number, status.SubStatusMessage);

                                    xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                    <soapenv:Header/>
                                                    <soapenv:Body>
                                                        <payout-transfer-reply>
	                                                        <reference_code></reference_code>
	                                                        <internal_reference_code></internal_reference_code>
	                                                        <account_details>
		                                                        <account_number></account_number>
		                                                        <account_type></account_type>
		                                                        <currency></currency>
	                                                        </account_details>
	                                                        <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                        <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                        <status>
		                                                        <status_code>" + status.StatusCode + @"</status_code>
		                                                        <status_description>" + status.StatusMessage + @"</status_description>
		                                                        <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                        <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                        </status>
                                                        </payout-transfer-reply>
                                                </soapenv:Body>
                                            </soapenv:Envelope> ";
                                }

                            }
                        }
                        else
                        {

                            xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                <soapenv:Header/>
                                                <soapenv:Body>
                                                    <payout-transfer-reply>
	                                                    <reference_code></reference_code>
	                                                    <internal_reference_code></internal_reference_code>
	                                                    <account_details>
		                                                    <account_number></account_number>
		                                                    <account_type></account_type>
		                                                    <currency></currency>
	                                                    </account_details>
	                                                    <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                    <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                    <status>
		                                                    <status_code>" + status.StatusCode + @"</status_code>
		                                                    <status_description>" + status.StatusMessage + @"</status_description>
		                                                    <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                    <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                    </status>
                                                    </payout-transfer-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope> ";
                        }
                    }
                    else {
                        xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                <soapenv:Header/>
                                                <soapenv:Body>
                                                    <payout-transfer-reply>
	                                                    <reference_code></reference_code>
	                                                    <internal_reference_code></internal_reference_code>
	                                                    <account_details>
		                                                    <account_number></account_number>
		                                                    <account_type></account_type>
		                                                    <currency></currency>
	                                                    </account_details>
	                                                    <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                    <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                    <status>
		                                                    <status_code>" + status.StatusCode + @"</status_code>
		                                                    <status_description>" + status.StatusMessage + @"</status_description>
		                                                    <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                    <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                    </status>
                                                    </payout-transfer-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope> ";
                    }
                }
                else if (transactionType.CompareTo("transfer-status-request") == 0)
                {

                    if (estado == true)
                    {
                        bool estadito = da.valida_esistencia_cuentayReferencia(transaction.account_number, transaction.reference_code);
                        if (estadito == true)
                        {
                            xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                <soapenv:Header/>
                                                <soapenv:Body>
                                                    <payout-transfer-reply>
	                                                    <reference_code>" + transaction.reference_code + @"</reference_code>
	                                                    <account_details>
		                                                    <account_number>" + transaction.account_number + @"</account_number>
		                                                    <account_type>" + transaction.AccountType + @"</account_type>
		                                                    <currency>" + transaction.currency_code + @"</currency>
	                                                    </account_details>
	                                                    <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                    <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                    <status>
		                                                    <status_code>" + status.StatusCode + @"</status_code>
		                                                    <status_description>" + status.StatusMessage + @"</status_description>
		                                                    <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                    <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                    </status>
                                                    </payout-transfer-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope> ";

                        }
                        else {
                            status.StatusCode = "00000";
                            status.StatusMessage = "Rejected Request";
                            status.SubStatusCode = "01002";
                            status.SubStatusMessage = "Invalid Data";

                            xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                <soapenv:Header/>
                                                <soapenv:Body>
                                                    <payout-transfer-reply>
	                                                    <reference_code></reference_code>
	                                                    <account_details>
		                                                    <account_number></account_number>
		                                                    <account_type></account_type>
		                                                    <currency></currency>
	                                                    </account_details>
	                                                    <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                    <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                    <status>
		                                                    <status_code>" + status.StatusCode + @"</status_code>
		                                                    <status_description>" + status.StatusMessage + @"</status_description>
		                                                    <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                    <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                    </status>
                                                    </payout-transfer-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope> ";
                        }

                    }
                    else {

                        status.StatusCode = "00000";
                        status.StatusMessage = "Rejected Request";
                        status.SubStatusCode = "01002";
                        status.SubStatusMessage = "Invalid Data";

                        xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                                <soapenv:Header/>
                                                <soapenv:Body>
                                                    <payout-transfer-reply>
	                                                    <reference_code></reference_code>
	                                                    <account_details>
		                                                    <account_number></account_number>
		                                                    <account_type></account_type>
		                                                    <currency></currency>
	                                                    </account_details>
	                                                    <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                    <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                    <status>
		                                                    <status_code>" + status.StatusCode + @"</status_code>
		                                                    <status_description>" + status.StatusMessage + @"</status_description>
		                                                    <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                    <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                    </status>
                                                    </payout-transfer-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope> ";
                    }

                }
                else
                {
                    status.StatusCode = "01000";
                    status.StatusMessage = "Internal Error";
                    status.SubStatusCode = "01001";
                    status.SubStatusMessage = "System Error";

                    da.inser_Error(transaction.account_number, status.SubStatusMessage);
                }

            }
            catch (Exception ex)
            {
                status.StatusCode = "01000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "01001";
                status.SubStatusMessage = "System Error";
                oErr = ex.Message;

                da.inser_Error(transaction.account_number, status.SubStatusMessage);

                xmlResponse = @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsi = 'http://www.w3.org/2001/XMLSchema-instance'>
                                            <soapenv:Header/>
                                            <soapenv:Body>
	                                                <transaction_date>" + DateTime.Now.ToString("yyyyMMdd") + @"</transaction_date>
	                                                <transaction_time>" + DateTime.Now.ToString("HH:mm:ss") + @"</transaction_time>
	                                                <status>
		                                                <status_code>" + status.StatusCode + @"</status_code>
		                                                <status_description>" + status.StatusMessage + @"</status_description>
		                                                <sub_status_code>" + status.SubStatusCode + @"</sub_status_code>
		                                                <sub_status_description>" + status.SubStatusMessage + @"</sub_status_description>
	                                                </status>
                                                </account-search-reply>
                                            </soapenv:Body>
                                        </soapenv:Envelope>";

            }
        }
        #endregion

        public string porcessRequest(string xmlRequest)
        {
            ClsCredencials credencials = new ClsCredencials();
            ClsStatus status = new ClsStatus();
            ClsTransaction transaction = new ClsTransaction();
            string transtring = string.Empty;
            string xmlResponse = string.Empty;
            bool exist = false;
            bool bloqued = false;
            bool cuenta_existe = false;

            credencials.User = "AirpackCoopsafa";
            credencials.pass = "C00psafa2020#";

            if (string.IsNullOrEmpty(xmlRequest))
            {
                status.StatusCode = "01000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "01001";
                status.SubStatusMessage = "Request cannot be null or empty";
                Response(transaction, status, ref xmlResponse);

                return xmlResponse;
            }
            
            try
            {

                XDocument xDocument = XDocument.Parse(xmlRequest);
                transtring = xDocument.Descendants().Elements("service").First().Value.ToString();

                if (transtring.CompareTo("account-search-request") == 0)
                {
                    transaction = (from iReq in xDocument.Descendants().Descendants("account-search-request")
                                    select new ClsTransaction
                                    {
                                        chanel = iReq.Element("authenticate").Element("channel").Value.ToString(),
                                        user = iReq.Element("authenticate").Element("user").Value.ToString(),
                                        pass = iReq.Element("authenticate").Element("pass").Value.ToString(),
                                        service = iReq.Element("service").ToString(),
                                        Bank_code = iReq.Element("bank_code").Value.ToString(),
                                        account_number = iReq.Element("account_details").Element("account_number").Value.ToString(),
                                    }).First();

                    if (transaction.user.Equals(credencials.User) && transaction.pass.Equals(credencials.pass))
                    {
                        cuenta_existe = da.valida_esistencia_cuenta(transaction.account_number);

                        if (cuenta_existe == true)
                        {

                            bloqued = da.valida_bloqueo(transaction.account_number);
                            if (bloqued == false)
                            {
                                status.StatusCode = "00000";
                                status.StatusMessage = "Rejected Request";
                                status.SubStatusCode = "00006";
                                status.SubStatusMessage = "Blocked Account";
                            }
                            else
                            {
                                exist = da.valida_cuenta(transaction.account_number);
                                if (exist == false)
                                {
                                    status.StatusCode = "00000";
                                    status.StatusMessage = "Rejected Request";
                                    status.SubStatusCode = "00004";
                                    status.SubStatusMessage = "Account Closed";
                                }
                                else
                                {
                                    status.StatusCode = "50000";
                                    status.StatusMessage = "Completed";
                                    status.SubStatusCode = "50001";
                                    status.SubStatusMessage = "Success";
                                    estado = true;
                                }
                            }
                        }
                        else {
                            status.StatusCode = "00000";
                            status.StatusMessage = "Rejected Request";
                            status.SubStatusCode = "00005";
                            status.SubStatusMessage = "Nonexistent Account";
                            estado = false;
                        }
                    }
                    else
                    {
                        status.StatusCode = "01000";
                        status.StatusMessage = "Internal Error";
                        status.SubStatusCode = "01002";
                        status.SubStatusMessage = "Authentication Error";
                        estado = false;
                    }
                    

                }
                else
                {

                    if (transtring.CompareTo("transfer-status-request") == 0)
                    {
                        transaction = (from iReq in xDocument.Descendants().Descendants("transfer-status-request")
                                       select new ClsTransaction
                                       {
                                           chanel = iReq.Element("authenticate").Element("channel").Value.ToString(),
                                           user = iReq.Element("authenticate").Element("user").Value.ToString(),
                                           pass = iReq.Element("authenticate").Element("pass").Value.ToString(),
                                           service = iReq.Element("service").ToString(),
                                           reference_code = iReq.Element("reference_code").ToString(),
                                           Bank_code = iReq.Element("bank_code").Value.ToString(),
                                           account_number = iReq.Element("account_details").Element("account_number").Value.ToString(),
                                       }).First();

                        if (transaction.user.Equals("airpakuser") && transaction.pass.Equals("test2018*"))
                        {
                            status.StatusCode = "50000";
                            status.StatusMessage = "Completed";
                            status.SubStatusCode = "50001";
                            status.SubStatusMessage = "Success";
                            estado = true;
                        }
                        else
                        {
                            status.StatusCode = "01000";
                            status.StatusMessage = "Internal Error";
                            status.SubStatusCode = "01002";
                            status.SubStatusMessage = "Authentication Error";
                            estado = false;
                        }
                    }
                    else {

                        transaction = (from iReq in xDocument.Descendants().Descendants("payout-transfer-request")
                                        select new ClsTransaction
                                        {
                                            chanel = iReq.Element("authenticate").Element("channel").Value.ToString(),
                                            user = iReq.Element("authenticate").Element("user").Value.ToString(),
                                            pass = iReq.Element("authenticate").Element("pass").Value.ToString(),
                                            reference_code = iReq.Element("reference_code").Value.ToString(),
                                            service = iReq.Element("service").ToString(),
                                            Receiver = string.Format("{0} {1}", iReq.Element("receiver").Element("name").Element("first_name").Value.ToString(),
                                                                            iReq.Element("receiver").Element("name").Element("last_name").Value.ToString()),
                                            first_name = iReq.Element("receiver").Element("name").Element("first_name").Value.ToString(),
                                            last_name = iReq.Element("receiver").Element("name").Element("last_name").Value.ToString(),
                                            account_number = iReq.Element("account_details").Element("account_number").Value.ToString(),
                                            Bank_code = iReq.Element("bank_code").Value.ToString(),
                                            concept = iReq.Element("concept").Value.ToString(),
                                            destination_amount = iReq.Element("financials").Element("destination_amount").Value.ToString(),
                                            currency_code = iReq.Element("financials").Element("currency_code").Value.ToString(),
                                        }).First();
                        
                        if (transaction.user.Equals(credencials.User) && transaction.pass.Equals(credencials.pass))
                        {
                            cuenta_existe = da.valida_esistencia_cuenta(transaction.account_number);

                            if (cuenta_existe == true)
                            {

                                bloqued = da.valida_bloqueo(transaction.account_number);
                                if (bloqued == false)
                                {
                                    status.StatusCode = "00000";
                                    status.StatusMessage = "Rejected Request";
                                    status.SubStatusCode = "00006";
                                    status.SubStatusMessage = "Blocked Account";

                                    bool save = da.insert_Transfer_fist(transaction.chanel, transaction.user, transaction.pass, transaction.reference_code, transaction.service, transaction.first_name, transaction.last_name, transaction.account_number, transaction.Bank_code);
                                    da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                    da.insert_Error_bitacora(status.SubStatusMessage, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
                                }
                                else
                                {

                                    exist = da.valida_cuenta(transaction.account_number);

                                    if (exist == false)
                                    {
                                        status.StatusCode = "00000";
                                        status.StatusMessage = "Rejected Request";
                                        status.SubStatusCode = "00004";
                                        status.SubStatusMessage = "Account Closed";

                                        bool save = da.insert_Transfer_fist(transaction.chanel, transaction.user, transaction.pass, transaction.reference_code, transaction.service, transaction.first_name, transaction.last_name,transaction.account_number, transaction.Bank_code);
                                        da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                        da.insert_Error_bitacora(status.SubStatusMessage, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
                                    }
                                    else
                                    {
                                        bool save = da.insert_Transfer_fist(transaction.chanel, transaction.user, transaction.pass, transaction.reference_code, transaction.service, transaction.first_name, transaction.last_name, transaction.account_number, transaction.Bank_code);

                                        bool monto_TF;
                                        bool monto_Lps = false;

                                        if (transaction.currency_code == "HNL")
                                        {
                                            monto_Lps = valida_monto_Lempiras(Convert.ToDecimal(transaction.destination_amount));
                                        }
                                        if (transaction.currency_code == "USD")
                                        {
                                            monto_TF = valida_monto(Convert.ToDecimal(transaction.destination_amount));
                                        }

                                        if (monto_Lps == false)
                                        {
                                            status.StatusCode = "00000";
                                            status.StatusMessage = "Rejected Request";
                                            status.SubStatusCode = "00003";
                                            status.SubStatusMessage = "Amount Exceeded Limit";

                                            da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                            da.insert_Error_bitacora(status.SubStatusMessage, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
                                        }
                                        else
                                        {
                                            if (save == true)
                                            {
                                                bool inserto_personal = da.Insert_personal_data(transaction.account_number);

                                                if (inserto_personal == false)
                                                {
                                                    status.StatusCode = "00000";
                                                    status.StatusMessage = "Rejected Request";
                                                    status.SubStatusCode = "00008";
                                                    status.SubStatusMessage = "Rejected by internal validation";
                                                    error_interno = true;
                                                    da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                                    da.insert_Error_bitacora(status.SubStatusMessage, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
                                                }
                                                else
                                                {

                                                    DataTable daper = da.Personal_data(transaction.account_number);
                                                    string name = string.Empty;

                                                    foreach (DataRow personal in daper.Rows)
                                                    {
                                                        name = personal["FIRST_NAME"].ToString() + " " + personal["LAST_NAME"].ToString();
                                                    }

                                                    if (transaction.Receiver.Equals(name))
                                                    {
                                                        if (transaction.currency_code == "HNL")
                                                        {
                                                            status.StatusCode = "50000";
                                                            status.StatusMessage = "Completed";
                                                            status.SubStatusCode = "50001";
                                                            status.SubStatusMessage = "Success";
                                                        }
                                                        else
                                                        {
                                                            status.StatusCode = "00000";
                                                            status.StatusMessage = "Rejected Request";
                                                            status.SubStatusCode = "00009";
                                                            status.SubStatusMessage = "Currency does not correspond";
                                                        }

                                                    }
                                                    else
                                                    {
                                                        status.StatusCode = "00000";
                                                        status.StatusMessage = "Rejected Request";
                                                        status.SubStatusCode = "00007";
                                                        status.SubStatusMessage = "Customer does not match";
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                status.StatusCode = "01000";
                                                status.StatusMessage = "Internal Error";
                                                status.SubStatusCode = "01004";
                                                status.SubStatusMessage = "Data Base Error";

                                                da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                                da.insert_Error_bitacora(status.SubStatusMessage, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
                                            }
                                        }
                                    }

                                }

                            }
                            else
                            {
                                status.StatusCode = "00000";
                                status.StatusMessage = "Rejected Request";
                                status.SubStatusCode = "00005";
                                status.SubStatusMessage = "Nonexistent Account";

                                bool save = da.insert_Transfer_fist(transaction.chanel, transaction.user, transaction.pass, transaction.reference_code, transaction.service, transaction.first_name, transaction.last_name, transaction.account_number, transaction.Bank_code);
                                da.inser_Error(transaction.account_number, status.SubStatusMessage);
                                da.insert_Error_bitacora(status.SubStatusMessage, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
                            }
                        }
                        else
                        {
                            status.StatusCode = "01000";
                            status.StatusMessage = "Internal Error";
                            status.SubStatusCode = "01002";
                            status.SubStatusMessage = "Authentication Error";
                        }
                    }

                }

                Validate(transaction, ref status);

                if (string.IsNullOrEmpty(status.StatusCode))
                {
                    status.StatusCode = "50000";
                    status.StatusMessage = "Completed";
                    status.SubStatusCode = "50001";
                    status.SubStatusMessage = "Success";
                }

            }
            catch (Exception ex)
            {
                status.StatusCode = "01000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "01001";
                status.SubStatusMessage = "System Error";

                error_interno = true;

                bool save = da.insert_Transfer_fist(transaction.chanel, transaction.user, transaction.pass, transaction.reference_code, transaction.service, transaction.first_name, transaction.last_name, transaction.account_number, transaction.Bank_code);
                da.inser_Error(transaction.account_number, status.SubStatusMessage);
                da.insert_Error_bitacora(status.SubStatusMessage +" error= "+ ex.Message, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
            }


            Response(transaction, status, ref xmlResponse, transtring);

            return xmlResponse;
        }

        bool valida_monto(decimal monto) {
            decimal monto_valido = 1999.99M;
            if (monto <= monto_valido) {
                return true;
            }
            else{
                return false;
            }
        }

        bool valida_monto_Lempiras(decimal monto)
        {
            decimal monto_valido = 45000.00M;
            if (monto <= monto_valido)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}