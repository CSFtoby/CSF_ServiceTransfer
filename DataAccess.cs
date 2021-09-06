using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Web;

namespace CSF_ServiceTransfer
{
    public class DataAccess
    {
        public string cadenaConexionOracle = "";

        public DataAccess()
        {
            cadenaConexionOracle = ConfigurationManager.ConnectionStrings["DbContext"].ConnectionString;// "Data Source= "+ p_tnsnames + ";Persist Security Info=True;User ID="+ p_usuario + " ;Password=#" + p_password;
        }


        #region validaciones de cuenta
        public bool valida_cuenta(string accaount) {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle)) { 
                    string sql = @"SELECT CANCELADA_B
                                    FROM MCA_CUENTAS
                                    WHERE NUMERO_CUENTA = :pa_accaount";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows) {
                        reader.Read();
                        if (reader["CANCELADA_B"].ToString() == "N")
                            returno = true;
                        else
                            returno = false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        public bool valida_bloqueo(string accaount)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT BLOQUEADA_B
                                    FROM MCA_CUENTAS
                                    WHERE NUMERO_CUENTA = :pa_accaount";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (reader["BLOQUEADA_B"].ToString() == "N")
                            returno = true;
                        else
                            returno = false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        public bool valida_esistencia_cuenta(string accaount)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT NUMERO_CUENTA
                                    FROM MCA_CUENTAS
                                    WHERE NUMERO_CUENTA = :pa_accaount";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (String.IsNullOrEmpty(reader["NUMERO_CUENTA"].ToString()))
                            returno = false;
                        else
                            returno = true;
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return returno;
        }

        public bool valida_esistencia_cuentayReferencia (string accaount, string reference)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;
            int Codigo = obtener_codigo(accaount);

            try
            {
                if (Codigo > 0)
                {
                    using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                    {
                        string sql = @"SELECT ACCOUNT_NUMBER, REFERENCE_CODE
                                        from MCA.MCA_DIRECTO_CUENTA
                                        where ACCOUNT_NUMBER=:pa_account
                                        and REFERENCE_CODE = :pa_reference
                                        amd CODIGO_OPERACION = :pa_codigo";
                        command.CommandText = sql;
                        command.Connection = connection;
                        command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                        command.Parameters.Add("pa_reference", OracleDbType.Varchar2).Value = reference;
                        command.Parameters.Add("pa_codigo", OracleDbType.Varchar2).Value = Codigo;
                        connection.Open();

                        reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            reader.Read();
                            if (String.IsNullOrEmpty(reader["ACCOUNT_NUMBER"].ToString()))
                                returno = false;
                            else
                                returno = true;
                        }

                    }
                }
                else {
                    returno = false;
                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }
#endregion

        public decimal Factor() {
            decimal factor_cambio=0;

            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"select MAX(VALOR_CAMBIO) FACTOR_CAMBIO
                                    From MGI.MGI_FACTOR_DE_CAMBIO";
                    command.CommandText = sql;
                    command.Connection = connection;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        factor_cambio = Convert.ToDecimal(reader["FACTOR_CAMBIO"]);
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return factor_cambio;
        }

        //apartado para encontrar los datos que tienen que enviarse al procedimiento de movimientos diarios y el credito
        public DataTable Data_Transaction(string accaount) {
            DataTable da = new DataTable();
            OracleCommand command = new OracleCommand();
            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"select CODIGO_AGENCIA, CODIGO_EMPRESA, CODIGO_SUB_APLICACION
                                    from MCA_CUENTAS
                                    where NUMERO_CUENTA = :pa_accaount";
               
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", accaount);

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    try
                    {
                        dataAdapter.Fill(da);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error en " + e.TargetSite.ToString() + " " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return da;
        }

        //obtener el codigo de operacion antes del credito
        public int obtener_codigo(string accaount)
        {
            int returno = 0;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT MAX(CODIGO_OPERACION) CODIGO_OPERACION
                                    from MCA.MCA_DIRECTO_CUENTA
                                    where ACCOUNT_NUMBER=:pa_account
                                    and DEPOSITO is null";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (String.IsNullOrEmpty(reader["CODIGO_OPERACION"].ToString()))
                            returno = 0;
                        else
                            returno = Convert.ToInt32(reader["CODIGO_OPERACION"]);
                    }

                }
            }
            catch (Exception ex)
            {
                returno = 0;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return returno;
        }

        //obtener el codigo de operacion una vez se haya logrado el credito 
        public int obtener_codigo_despues_Credito(string accaount)
        {
            int returno = 0;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT MAX(CODIGO_OPERACION) CODIGO_OPERACION
                                    from MCA.MCA_DIRECTO_CUENTA
                                    where ACCOUNT_NUMBER=:pa_account
                                    and DEPOSITO = 'S'";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (reader["CODIGO_OPERACION"].ToString() != null)
                            returno = Convert.ToInt32(reader["CODIGO_OPERACION"]);
                        else
                            returno = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                returno = 0;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return returno;
        }

        //ingresar los datos de la operacion antes de efectuar las demas tareas
        public bool insert_Transfer_fist(string chanel, string user, string pass, string reference, string service, string fistName, string lastName, string account, string bank) {
            bool returno = false;

            OracleCommand command= new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"Insert into MCA.MCA_DIRECTO_CUENTA
                            (CHANEL,USSER,PASS,REFERENCE_CODE,SERVICE,ACCOUNT_NUMBER,BANK_CODE,CONCEPT,
                            DESTINATION_AMOUNT,INTERNAL_REFERENCE_CODE,TRANSACTION_DATE,TRANSACTION_TIME,
                            STATUS_CODE,SUB_STATUS_CODE,ADDITIONAL_INFO,FIRST_NAME,LAST_NAME,CURRENCY_CODE,
                            ACOUNTTYPE,ID_NUMBER,ID_TYPE,ADDR,CITY,STATE,COUNTRY_NAME,COUNTRY_CODE,PHONE_NUMBER,
                            SEX,DATE_OF_BIRTH,COUNTRY_OF_BIRTH,NATIONALITY,OCCUPATION,MARITAL_STATUS,STATUS_ACCOUN) 
                    values (:pa_chanel,:pa_user,:pa_pass,:pa_reference,:pa_service,:pa_accaount,:pa_bank,null,null,null,null,null,null,null,null,:pa_first,
                            :pa_last,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null)";

                    command.CommandText = sql;
                    command.Connection = connection;

                    command.Parameters.Add("pa_chanel", OracleDbType.Varchar2, 50).Value = chanel;
                    command.Parameters.Add("pa_user", OracleDbType.Varchar2, 20).Value = user;
                    command.Parameters.Add("pa_pass", OracleDbType.Varchar2, 20).Value = pass;
                    command.Parameters.Add("pa_reference", OracleDbType.Varchar2, 20).Value = reference;
                    command.Parameters.Add("pa_service", OracleDbType.Varchar2, 50).Value = service;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2, 20).Value = account;
                    command.Parameters.Add("pa_bank", OracleDbType.Varchar2, 50).Value = bank;
                    command.Parameters.Add("pa_first", OracleDbType.Varchar2, 50).Value = fistName;
                    command.Parameters.Add("pa_last", OracleDbType.Varchar2, 50).Value = lastName;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.ExecuteNonQuery();
                    returno = true;
                    connection.Clone();

                }
            }
            catch (Exception ex)
            {
                returno = false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        //agregar la informacion personal 
        public bool Insert_personal_data(string accaount) { 

            bool returno = false;
            DataTable data = Personal_data(accaount);
            int Codigo = obtener_codigo(accaount);

            if (Codigo == 0)
            {
                returno = false;
                throw new Exception("Error en obtener informacion");
            }
            else {

                OracleCommand command = new OracleCommand();

                string first_name;
                string last_name;
                string account_type;
                string cuncurrecy = string.Empty;
                string id_number;
                string id_type;
                string address;
                string city;
                string state;
                string country_name;
                string country_code;
                string phone;
                char sex;
                string fecha_nacimiento;
                string country_birth;
                string nationality;
                string ocupation;
                char marital_status;
                string status_account;

                foreach (DataRow personal in data.Rows)
                {
                    first_name = personal["FIRST_NAME"].ToString();
                    last_name = personal["LAST_NAME"].ToString();
                    cuncurrecy = "HNL";
                    account_type = personal["ACCOUNT_TYPE"].ToString();
                    id_number = personal["ID_NUMBER"].ToString();

                    if (personal["ID_TYPE"].ToString() == "1")
                    {
                        id_type = "B";
                    }
                    else
                    {
                        if (personal["ID_TYPE"].ToString() == "3")
                        {
                            id_type = "A";
                        }
                        else {
                            if (personal["ID_TYPE"].ToString() == "8")
                            {
                                id_type = "C";
                            }
                            else{
                                if (personal["ID_TYPE"].ToString() == "11")
                                {
                                    id_type = "D";
                                }
                                else {
                                    id_type = "E";
                                }
                            }
                        }
                    }

                    address = personal["ADDR"].ToString();
                    city = personal["CITY"].ToString();
                    state = personal["STATE"].ToString();
                    country_name = personal["COUNTRY_NAME"].ToString();

                    if (personal["COUNTRY_CODE"].ToString() == "HONDUREÑO")
                    {
                        country_code = "HN";
                    }
                    else {
                        country_code = "HN";
                    }

                    phone = personal["PHONE_NUMBER"].ToString();
                    sex = Convert.ToChar(personal["SEX"].ToString());
                    fecha_nacimiento = personal["DATE_OF_BIRTHDAY"].ToString();
                    country_birth = personal["COUNTRY_OF_BIRTH"].ToString();

                    if (personal["NATIONALITY"].ToString() == "HONDUREÑO") {
                        nationality = "HN";
                         }
                    else {
                            nationality = "HN";
                        }

                    ocupation = personal["OCCUPATION"].ToString();
                    marital_status = Convert.ToChar(personal["MARITAL_STATUS"].ToString());
                    status_account = personal["STATUS_ACCOUN"].ToString();

                    if (phone == "N/A" || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(ocupation))
                    {
                        returno = false;
                    }
                    else {
                        try
                        {
                            using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                            {
                                string sql = @" update MCA.MCA_DIRECTO_CUENTA set 
                                                FIRST_NAME= :pa_first, 
                                                LAST_NAME= :pa_last, 
                                                CURRENCY_CODE= :pa_currency,
                                                ACOUNTTYPE=:pa_accountype,
                                                ID_NUMBER=:pa_id,
                                                ID_TYPE=:pa_idtype,
                                                ADDR=:pa_addres,
                                                CITY=:pa_city,
                                                STATE=:pa_state,
                                                COUNTRY_NAME=:pa_contry,
                                                COUNTRY_CODE=:pa_countrcode,
                                                PHONE_NUMBER=:pa_phone,
                                                SEX=:pa_sex,
                                                DATE_OF_BIRTH=:pa_date,
                                                COUNTRY_OF_BIRTH=:pa_nacio,
                                                NATIONALITY=:pa_nacional,
                                                OCCUPATION=:pa_ocupa,
                                                MARITAL_STATUS=:pa_marital,
                                                STATUS_ACCOUN=:pa_status
                                      where ACCOUNT_NUMBER= :pa_accaount
                                            and CODIGO_OPERACION = :pa_codigo_operacion";

                                command.CommandText = sql;
                                command.Connection = connection;

                                command.Parameters.Add("pa_first", OracleDbType.Varchar2, 40).Value = first_name;
                                command.Parameters.Add("pa_last", OracleDbType.Varchar2, 40).Value = last_name;
                                command.Parameters.Add("pa_currency", OracleDbType.Varchar2, 3).Value = "HNL";
                                command.Parameters.Add("pa_accountype", OracleDbType.Varchar2, 1000).Value = account_type;
                                command.Parameters.Add("pa_id", OracleDbType.Varchar2, 50).Value = id_number;
                                command.Parameters.Add("pa_idtype", OracleDbType.Varchar2, 50).Value = id_type;
                                command.Parameters.Add("pa_addres", OracleDbType.Varchar2, 50).Value = address;
                                command.Parameters.Add("pa_city", OracleDbType.Varchar2, 150).Value = city;
                                command.Parameters.Add("pa_state", OracleDbType.Varchar2, 50).Value = state;
                                command.Parameters.Add("pa_contry", OracleDbType.Varchar2, 50).Value = country_name;
                                command.Parameters.Add("pa_countrcode", OracleDbType.Varchar2, 3).Value = country_code;
                                command.Parameters.Add("pa_phone", OracleDbType.Varchar2, 8).Value = phone;
                                command.Parameters.Add("pa_sex", OracleDbType.Char, 1).Value = sex;
                                command.Parameters.Add("pa_date", OracleDbType.Varchar2, 8).Value = fecha_nacimiento;
                                command.Parameters.Add("pa_nacio", OracleDbType.Varchar2, 2).Value = country_birth;
                                command.Parameters.Add("pa_nacional", OracleDbType.Varchar2, 2).Value = nationality;
                                command.Parameters.Add("pa_ocupa", OracleDbType.Varchar2, 50).Value = ocupation;
                                command.Parameters.Add("pa_marital", OracleDbType.Char, 1).Value = marital_status;
                                command.Parameters.Add("pa_status", OracleDbType.Varchar2, 50).Value = status_account;
                                command.Parameters.Add("pa_accaount", OracleDbType.Varchar2, 20).Value = accaount;
                                command.Parameters.Add("pa_codigo_operacion", OracleDbType.Int32).Value = Codigo;

                                if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                                    connection.Open();

                                command.ExecuteNonQuery();
                                returno = true;
                                connection.Clone();
                            }
                        }
                        catch (Exception ex)
                        {
                            returno = false;
                            throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
                        }
                    }

                    

                }
            }
            return returno;
        }

        public DataTable Personal_data(string accaount)
        {
            DataTable returno = new DataTable();
            OracleCommand command = new OracleCommand();
            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"select 
                                       REPLACE(CL.NOMBRES,'  ',' ')  FIRST_NAME, 
                                       (CL.PRIMER_APELLIDO ||' '|| CL.SEGUNDO_APELLIDO) LAST_NAME,
                                       PA.ABREVIATURA CUNCURRECY, 
                                       CTS.CODIGO_SUB_APLICACION ACCOUNT_TYPE, 
                                       CL.NUMERO_IDENTIFICACION ID_NUMBER,
                                       CL.CODIGO_TIPO_IDENTIFICACION ID_TYPE,
                                       DIR.DIRECCION ADDR,
                                       MUN.NOMBRE CITY,
                                       DEP.NOMBRE STATE,
                                       PA.NOMBRE COUNTRY_NAME, 
                                       PA.ABREVIATURA COUNTRY_CODE, 
                                       DIR.TELEFONO PHONE_NUMBER,
                                       CL.SEXO SEX,
                                       (to_char(CL.FECHA_DE_NACIMIENTO, 'DD')||to_char(CL.FECHA_DE_NACIMIENTO, 'MM')||to_char(CL.FECHA_DE_NACIMIENTO, 'YYYY')) DATE_OF_BIRTHDAY,
                                       PA.ABREVIATURA COUNTRY_OF_BIRTH,
                                       PA.ABREVIATURA NATIONALITY,
                                       EQ.DESC_ESQUIVALENCIA OCCUPATION,
                                       CL.ESTADO_CIVIL MARITAL_STATUS,
                                       CTS.CANCELADA_B STATUS_ACCOUN
                                FROM MCA.MCA_CUENTAS CTS,
                                     MGI.MGI_CLIENTES CL,
                                     (
                                       SELECT CODIGO_CLIENTE,CODIGO_MUNICIPIO,CODIGO_DEPARTAMENTO,CODIGO_PAIS,NOMENCLATURA_2 DIRECCION,
                                         CASE
                                            WHEN TO_NUMBER(REPLACE(TRANSLATE(SUBSTR((REPLACE(LTRIM(TRANSLATE(TELEFONOS, TRANSLATE(TELEFONOS, '1234567890', ' ') , ' ')),' ' ,'')),1,30), '-',' '),' ',''))IS  NULL  THEN  FAX
                                            WHEN SUBSTR(TELEFONOS,0,1) = '2' THEN FAX
                                            WHEN SUBSTR(TELEFONOS,0,1) != '2' THEN TO_NUMBER(REPLACE(TRANSLATE(SUBSTR((REPLACE(LTRIM(TRANSLATE(TELEFONOS, TRANSLATE(TELEFONOS, '1234567890', ' ') , ' ')),' ' ,'')),1,30), '-',' '),' ',''))
                                            ELSE
                                                   TO_NUMBER(REPLACE(TRANSLATE(SUBSTR((REPLACE(LTRIM(TRANSLATE(FAX, TRANSLATE(FAX, '1234567890', ' ') , ' ')),' ' ,'')),1,30), '-',' '),' ',''))
                                        END TELEFONO 
                                       FROM MGI.MGI_DIRECCIONES
                                        WHERE CODIGO_DIRECCION=1 
                                     )DIR,
                                     MGI.MGI_PAISES PA,
                                     MGI.MGI_DEPARTAMENTOS DEP,
                                     MGI.MGI_MUNICIPIOS MUN,
                                     MGI.MGI_PROFESIONES P,
                                     MGI.MGI_EQUXPROFESION_WU EQ
                                WHERE CL.CODIGO_CLIENTE = CTS.CODIGO_CLIENTE
                                  AND CTS.CODIGO_CLIENTE = DIR.CODIGO_CLIENTE(+)
                                  AND PA.CODIGO_PAIS = DIR.CODIGO_PAIS
                                  AND DEP.CODIGO_PAIS = PA.CODIGO_PAIS
                                  AND DEP.CODIGO_DEPARTAMENTO = DIR.CODIGO_DEPARTAMENTO
                                  AND MUN.CODIGO_DEPARTAMENTO = DEP.CODIGO_DEPARTAMENTO
                                  AND MUN.CODIGO_MUNICIPIO = DIR.CODIGO_MUNICIPIO
                                  AND CL.CODIGO_PROFESION = P.CODIGO_PROFESION
                                  AND P.CODIGO_PROFESION = EQ.CODIGO_PROFESION
                                  AND (substr(DIR.TELEFONO,0,1) = '9' or substr(DIR.TELEFONO,0,1) = '3' or substr(DIR.TELEFONO,0,1) = '8') 
                                  AND CTS.NUMERO_CUENTA = :pa_accaount
                                  AND ROWNUM = 1";

                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", accaount);

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    try
                    {
                        dataAdapter.Fill(returno);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error en " + e.TargetSite.ToString() + " " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        //insertar la informacion de los montos para poder efectuar el credito
        public bool inser_Error(string cuenta, string mensaje)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            int Codigo = obtener_codigo(cuenta);

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"update MCA.MCA_DIRECTO_CUENTA set 
                                                DESCRIP_DEP = :pa_mensaje, 
                                                DEPOSITO = 'N'
                                      where ACCOUNT_NUMBER = :pa_accaount
                                        and CODIGO_OPERACION = :pa_codigo_operacion";

                    command.CommandText = sql;
                    command.Connection = connection;

                    command.Parameters.Add("pa_mensaje", OracleDbType.Varchar2).Value = mensaje;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2, 50).Value = cuenta;
                    command.Parameters.Add("pa_codigo_operacion", OracleDbType.Int32).Value = Codigo;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.ExecuteNonQuery();
                    returno = true;
                    connection.Clone();
                }

            }
            catch (Exception ex)
            {
                returno = false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        public bool insert_Error_bitacora(string descrip, string account, decimal monto)
        {
            bool returno = false;

            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"Insert into MCA_BITACORA_ERROR 
                                        (DESCRIPCION,FECHA,CUENTA,MONTO) 
                                values (:pa_descrp,trunc(sysdate),:pa_account, :pa_monto)";

                    command.CommandText = sql;
                    command.Connection = connection;

                    command.Parameters.Add("pa_descrp", OracleDbType.Varchar2, 100).Value = descrip;
                    command.Parameters.Add("pa_account", OracleDbType.Varchar2, 20).Value = account;
                    command.Parameters.Add("monto", OracleDbType.Decimal).Value = monto;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.ExecuteNonQuery();
                    returno = true;
                    connection.Clone();

                }
            }
            catch (Exception ex)
            {
                returno = false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        //insertar la informacion de los montos para poder efectuar el credito
        public bool inser_Datos_monto(int codigo_empresa, int codigo_agencia, int codigo_sub_aplicacion, string cuenta, decimal monto) {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            int Codigo = obtener_codigo(cuenta);

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"update MCA.MCA_DIRECTO_CUENTA set 
                                                DESTINATION_AMOUNT = :pa_amount, 
                                                COD_AGENCIA = :pa_agencia, 
                                                CD_EMPRESA = :pa_empresa,
                                                COD_SUB_AP = :pa_sub_app
                                      where ACCOUNT_NUMBER = :pa_accaount
                                      and CODIGO_OPERACION = :pa_codigo_operacion";

                    command.CommandText = sql;
                    command.Connection = connection;

                    command.Parameters.Add("pa_amount", OracleDbType.Decimal).Value = monto;
                    command.Parameters.Add("pa_agencia", OracleDbType.Int32).Value = codigo_agencia;
                    command.Parameters.Add("pa_empresa", OracleDbType.Int32).Value = codigo_empresa;
                    command.Parameters.Add("pa_sub_app", OracleDbType.Int32).Value = codigo_sub_aplicacion;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2, 50).Value = cuenta;
                    command.Parameters.Add("pa_codigo_operacion", OracleDbType.Int32).Value = Codigo;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.ExecuteNonQuery();
                    returno = true;
                    connection.Clone();
                }

            }
            catch (Exception ex)
            {
                returno = false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        public bool valida_duplicidad(string accaount, decimal amount, string date, string reference)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"select ACCOUNT_NUMBER, DESTINATION_AMOUNT, TRANSACTION_DATE, REFERENCE_CODE from MCA.MCA_DIRECTO_CUENTA
                                        where ACCOUNT_NUMBER = :pa_cuenta
                                        and DESTINATION_AMOUNT = :pa_amount
                                        and TRANSACTION_DATE = :pa_trasnaction_date
                                        and REFERENCE_CODE = :pa_reference
                                        and ROWNUM = 1";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    command.Parameters.Add("pa_amount", OracleDbType.Decimal).Value = amount;
                    command.Parameters.Add("pa_trasnaction_date", OracleDbType.Varchar2).Value = date;
                    command.Parameters.Add("pa_reference", OracleDbType.Varchar2).Value = reference;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        string a = reader["ACCOUNT_NUMBER"].ToString();
                        decimal b = Convert.ToDecimal(reader["DESTINATION_AMOUNT"].ToString());
                        string c = reader["TRANSACTION_DATE"].ToString();
                        string d = reader["REFERENCE_CODE"].ToString();

                        if (a.Equals(accaount)  && b.Equals(amount)  && c.Equals(date) && d.Equals(reference))
                        {
                            returno = false;
                        }
                        else {
                            returno = true;
                        }
                    }
                    else {
                        returno = true;
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        //es en este apartado donde se llama el procedimiento que hace el credito a la cuenta del afiliado
        #region paquete
        public bool MCA_K_AHORROS(string cuenta) {

            bool returno = false;
            int cuentaInt = Convert.ToInt32(cuenta);
            int Codigo = obtener_codigo(cuenta);

            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                     string query = @"MCA.MCA_P_DC_CREDITO";

                    command.CommandText = query;
                    command.Connection = connection;
                    command.Parameters.Add("P_CUENTA", OracleDbType.Int32).Value = cuentaInt;
                    command.Parameters.Add("P_OPERACION", OracleDbType.Int32).Value = Codigo;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.CommandType = CommandType.StoredProcedure;
                    command.ExecuteNonQuery();

                    command.Dispose();
                    connection.Close();
                    returno = true;

                }
            }

            catch (Exception ex)
            {
                returno = false;
                throw new Exception($"{ex.TargetSite}: {ex.Message}", ex.InnerException);
            }

            return returno;
        }
        #endregion

        #region ingreso de la informacion
        //una vez aceptado el credito se procede a actualizar la informacion entrante del xml request
        public bool last_update(string cuenta, string servicio, string bank, string concep, string data, string time, string code,
                                string subcode, string aditional, string reference)
        {

            bool returno = false;
            int Codigo = obtener_codigo_despues_Credito(cuenta);

            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string query = @"update MCA.MCA_DIRECTO_CUENTA set  
                                        SERVICE = :pa_servicio, 
                                        BANK_CODE = :pa_banck,
                                        CONCEPT= :pa_concep,
                                        TRANSACTION_DATE = :pa_date,
                                        TRANSACTION_TIME  = :pa_time,
                                        STATUS_CODE = :pa_code,
                                        SUB_STATUS_CODE = :pa_sub_code,
                                        ADDITIONAL_INFO = :pa_aditional
                                      where ACCOUNT_NUMBER= :pa_accaount
                                      and CODIGO_OPERACION = :pa_codigo_operacion
                                      and DEPOSITO = 'S'";

                    command.CommandText = query;
                    command.Connection = connection;

                    command.Parameters.Add("pa_servicio", OracleDbType.Varchar2).Value = servicio;
                    command.Parameters.Add("pa_banck", OracleDbType.Varchar2).Value = bank;
                    command.Parameters.Add("pa_concep", OracleDbType.Varchar2).Value = concep;
                    command.Parameters.Add("pa_date", OracleDbType.Varchar2).Value = data;
                    command.Parameters.Add("pa_time", OracleDbType.Varchar2).Value = time;
                    command.Parameters.Add("pa_code", OracleDbType.Varchar2).Value = code;
                    command.Parameters.Add("pa_sub_code", OracleDbType.Varchar2, 50).Value = subcode;
                    command.Parameters.Add("pa_aditional", OracleDbType.Varchar2).Value = aditional;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = cuenta;
                    command.Parameters.Add("pa_codigo_operacion", OracleDbType.Int32).Value = Codigo;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.ExecuteNonQuery();
                    returno = true;
                    connection.Clone();

                }
            }

            catch (Exception ex)
            {
                returno = false;
                throw new Exception($"{ex.TargetSite}: {ex.Message}", ex.InnerException);
            }

            return returno;
        }

        public DataTable Trasnt_info(string accaount)
        {
            DataTable returno = new DataTable();
            OracleCommand command = new OracleCommand();
            int Codigo = obtener_codigo_despues_Credito(accaount);
            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT 
                            REFERENCE_CODE, 
                            INTERNAL_REFERENCE_CODE, 
                            CURRENCY_CODE, 
                            TRANSACTION_DATE, 
                            TRANSACTION_TIME
                        FROM MCA.MCA_DIRECTO_CUENTA
                        WHERE ACCOUNT_NUMBER = :pa_account
                          AND CODIGO_OPERACION = :pa_operation";

                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_account", accaount);
                    command.Parameters.Add("pa_operation", Codigo);

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    try
                    {
                        dataAdapter.Fill(returno);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error en " + e.TargetSite.ToString() + " " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }
        #endregion
    }
}