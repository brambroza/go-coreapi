using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data; 

namespace coreapi.DB
{
    public static class DBConn
    {
        public static SqlCommand Cmd;
        public static SqlConnection Cnn;
        public static SqlTransaction Tran;
        public static SqlCommand Cmd2;
        public static SqlConnection Cnn2;
        public static SqlTransaction Tran2;


        public static DBConntext _dBConntext = new DBConntext();
        public static DBConntextSystem _dBConntextSystem = new DBConntextSystem();


        #region " CONNECTTION "

        public static void SqlConnectionOpen()
        {
            if (DBConn.Cnn == null) { DBConn.Cnn = new SqlConnection(); };
            if (DBConn.Cnn.State == ConnectionState.Open)
            {
                DBConn.Cnn.Close();
            };
            DBConn.Cnn.ConnectionString = _dBConntext.getConnectionString();
            DBConn.Cnn.Open();
        }

        public static void SqlBeginTransaction()
        {
            if (DBConn.Cnn == null) { DBConn.Cnn = new SqlConnection(); }

            if (DBConn.Cnn.State == ConnectionState.Open)
            {
                DBConn.Cnn.Close();
            };
            DBConn.Cnn.ConnectionString = _dBConntext.getConnectionString();
            DBConn.Cnn.Open();
            DBConn.Tran = DBConn.Cnn.BeginTransaction();
        }

        public static SqlConnection SqlConnectionOpen(SqlConnection _cnn)
        {
            if (_cnn == null) { _cnn = new SqlConnection(); };
            if (_cnn.State == ConnectionState.Open)
            {
                _cnn.Close();
            };
            _cnn.ConnectionString = "";
            _cnn.Open();
            return _cnn;
        }

        public static SqlConnection SqlBeginTransaction(SqlConnection _cnn)
        {
            if (_cnn == null) { _cnn = new SqlConnection(); }

            if (_cnn.State == ConnectionState.Open)
            {
                _cnn.Close();
            };
            _cnn.ConnectionString = "";
            _cnn.Open();
            DBConn.Tran = _cnn.BeginTransaction();
            return _cnn;
        }

        public static SqlConnection SqlBeginTransaction(SqlConnection _cnn, SqlTransaction _tran)
        {
            if (_cnn == null) { _cnn = new SqlConnection(); };

            if (_cnn.State == ConnectionState.Open)
            {
                _cnn.Close();
            };
            _cnn.ConnectionString = "";
            _cnn.Open();
            _tran = _cnn.BeginTransaction();
            return _cnn;
        }

        public static void DisposeSqlConnection(SqlConnection _cnn)
        {
            if ((_cnn != null))
            {
                if (_cnn.State == ConnectionState.Open)
                {
                    _cnn.Close();
                }
                _cnn.Dispose();
            }
        }

        public static void DisposeSqlConnection(SqlCommand _cmd)
        {
            if ((_cmd != null))
            {
                if ((_cmd.Connection != null))
                {
                    if (_cmd.Connection.State == ConnectionState.Open)
                    {
                        _cmd.Connection.Close();
                    }
                    _cmd.Connection.Dispose();
                }
                _cmd.Dispose();
            }
        }

        public static void DisposeSqlConnection(SqlDataAdapter _adapter)
        {
            if ((_adapter != null))
            {
                if ((_adapter.SelectCommand != null))
                {
                    if ((_adapter.SelectCommand.Connection != null))
                    {
                        if (!(_adapter.SelectCommand.Connection.State == ConnectionState.Open))
                        {
                            _adapter.SelectCommand.Connection.Close();
                        }
                        _adapter.SelectCommand.Connection.Dispose();
                    }
                    _adapter.SelectCommand.Dispose();
                }
                _adapter.Dispose();
            }
        }

        public static void DisposeSqlTransaction(SqlTransaction _tran)
        {
            if ((_tran != null))
            {
                if ((_tran.Connection != null))
                {
                    if (_tran.Connection.State == ConnectionState.Open)
                    {
                        _tran.Connection.Close();
                    }
                    _tran.Connection.Dispose();
                }
                _tran.Dispose();
            }
        }

        public static void ClearParameterObject(SqlCommand _cmd)
        {
            if (_cmd.Parameters.Count > 0)
            {
                _cmd.Parameters.Clear();
            }
        }
        #endregion


        public static bool Execute_Tran(string[] QryStr)
        {
            try
            {
                int Complete = 0;



                DBConn.SqlConnectionOpen();
                DBConn.Cmd = DBConn.Cnn.CreateCommand();
                DBConn.Tran = DBConn.Cnn.BeginTransaction();

                foreach (string Str in QryStr)
                {

                    DBConn.Cmd.CommandType = CommandType.Text;
                    DBConn.Cmd.CommandText = Str;
                    DBConn.Cmd.Transaction = DBConn.Tran;
                    Complete = DBConn.Cmd.ExecuteNonQuery();
                    DBConn.Cmd.Parameters.Clear();

                    if (Complete <= 0)
                    {
                        DBConn.Tran.Rollback();
                        DBConn.DisposeSqlTransaction(DBConn.Tran);
                        DBConn.DisposeSqlConnection(DBConn.Cmd);
                        return false;
                    }

                }

                DBConn.Tran.Commit();
                DBConn.DisposeSqlTransaction(DBConn.Tran);
                DBConn.DisposeSqlConnection(DBConn.Cmd);
                return true;

            }
            catch (Exception ex)
            {
                DBConn.Tran.Rollback();
                DBConn.DisposeSqlTransaction(DBConn.Tran);
                DBConn.DisposeSqlConnection(DBConn.Cmd);
                return false;
            }
        }

        public static int Execute_Tran(string sqlStr, SqlCommand sqlcmd, SqlTransaction Tr)
        {
            try
            {
                int Complete = 0;

                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sqlStr;
                sqlcmd.CommandTimeout = 0;
                sqlcmd.Transaction = Tr;
                Complete = sqlcmd.ExecuteNonQuery();
                sqlcmd.Parameters.Clear();

                return Complete;

            }

            catch (Exception ex)
            {

                return -1;
            }

        }


        public static int ExecuteTranParam(string sqlStr, SqlCommand sqlcmd, SqlTransaction Tr)
        {
            int Complete = 0;
            try
            {

                sqlcmd.CommandType = CommandType.Text;
               
               sqlcmd.Transaction = Tr;
                Complete = sqlcmd.ExecuteNonQuery();
                sqlcmd.Parameters.Clear();

                return Complete;

            }
            catch (Exception ex)
            {
                return Complete;
            }
        }


        public static int ExecuteTran(string sqlStr, SqlCommand sqlcmd, SqlTransaction Tr)
        {
            int Complete = 0;
            try
            {

                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = sqlStr;
                sqlcmd.Transaction = Tr;
                Complete = sqlcmd.ExecuteNonQuery();
                sqlcmd.Parameters.Clear();

                return Complete;

            }
            catch(Exception ex)
            {
                return Complete;
            }
        }



        #region "NonTransection"

        public static bool ExecuteOnly(string QryStr)
        {

            SqlConnection _Cnn = new SqlConnection();
            SqlCommand _Cmd = new SqlCommand();

            try
            {


                if (_Cnn.State == ConnectionState.Open) { _Cnn.Close(); };
                _Cnn.ConnectionString = _dBConntext.getConnectionString() ;
                _Cnn.Open();
                _Cmd = _Cnn.CreateCommand();
                _Cmd.CommandTimeout = 0;
                _Cmd.CommandType = CommandType.Text;
                _Cmd.CommandText = QryStr;
                _Cmd.ExecuteNonQuery();
                _Cmd.Parameters.Clear();

                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
                return true;
            }
            catch (Exception ex)
            {
                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
                //Interaction.MsgBox(ex.Message);
                return false;
            }

        }


        public static bool ExecuteOnlySystem(string QryStr)
        {

            SqlConnection _Cnn = new SqlConnection();
            SqlCommand _Cmd = new SqlCommand();

            try
            {


                if (_Cnn.State == ConnectionState.Open) { _Cnn.Close(); };
                _Cnn.ConnectionString = _dBConntextSystem.getConnectionString();
                _Cnn.Open();
                _Cmd = _Cnn.CreateCommand();
                _Cmd.CommandTimeout = 0;
                _Cmd.CommandType = CommandType.Text;
                _Cmd.CommandText = QryStr;
                _Cmd.ExecuteNonQuery();
                _Cmd.Parameters.Clear();

                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
                return true;
            }
            catch (Exception ex)
            {
                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
                //Interaction.MsgBox(ex.Message);
                return false;
            }

        }

        public static DataTable GetDataTableSystem(string QryStr, string TableName = "DataTalble1", bool useapi = true)
        {
            DataTable objDT = new DataTable(TableName);

            try
            {


                SqlConnection _Cnn = new SqlConnection();
                SqlCommand _Cmd = new SqlCommand();
                try
                {


                    if (_Cnn.State == ConnectionState.Open) { _Cnn.Close(); };
                    _Cnn.ConnectionString = _dBConntextSystem.getConnectionString();
                    _Cnn.Open();
                    _Cmd = _Cnn.CreateCommand();

                    var _Adepter = new SqlDataAdapter(_Cmd);
                    _Adepter.SelectCommand.CommandTimeout = 0;
                    _Adepter.SelectCommand.CommandType = CommandType.Text;
                    _Adepter.SelectCommand.CommandText = QryStr;
                    _Adepter.Fill(objDT);
                    _Adepter.Dispose();

                    DisposeSqlConnection(_Cmd);
                    DisposeSqlConnection(_Cnn);
                }
                catch (Exception ex)
                {
                    DisposeSqlConnection(_Cmd);
                    DisposeSqlConnection(_Cnn);
                }


            }
            catch { }




            return objDT;
        }



        public static bool ExecuteNonQuery(string QryStr)
        {
            try
            {
                int Complete = 0;


                DBConn.SqlConnectionOpen();
                DBConn.Cmd = DBConn.Cnn.CreateCommand();
                Tran = Cnn.BeginTransaction();

                DBConn.Cmd.CommandType = CommandType.Text;
                DBConn.Cmd.CommandText = QryStr;
                DBConn.Cmd.Transaction = Tran;
                Complete = DBConn.Cmd.ExecuteNonQuery();
                DBConn.Cmd.Parameters.Clear();

                if (Complete <= 0)
                {
                    DBConn.Tran.Rollback();
                    DBConn.DisposeSqlTransaction(DBConn.Tran);
                    DBConn.DisposeSqlConnection(DBConn.Cmd);
                    return false;
                }

                DBConn.Tran.Commit();
                DBConn.DisposeSqlTransaction(DBConn.Tran);
                DBConn.DisposeSqlConnection(DBConn.Cmd);
                return true;

            }

            catch (Exception ex)
            {
                DBConn.Tran.Rollback();
                DBConn.DisposeSqlTransaction(DBConn.Tran);
                DBConn.DisposeSqlConnection(DBConn.Cmd);
                return false;
            }
        }
        public static bool ExecuteNonQuery(ref SqlCommand _Cmd)
        {
            try
            {
                int Complete = 0;


                DBConn.SqlConnectionOpen();
                Tran = Cnn.BeginTransaction();

                _Cmd.Connection = DBConn.Cnn;
                _Cmd.CommandTimeout = 0;
                _Cmd.Transaction = Tran;
                Complete = _Cmd.ExecuteNonQuery();
                _Cmd.Parameters.Clear();

                if (Complete <= 0)
                {
                    DBConn.Tran.Rollback();
                    DBConn.DisposeSqlTransaction(DBConn.Tran);
                    DBConn.DisposeSqlConnection(_Cmd);
                    return false;
                }

                DBConn.Tran.Commit();
                DBConn.DisposeSqlTransaction(DBConn.Tran);
                DBConn.DisposeSqlConnection(_Cmd);
                return true;

            }
            catch (Exception ex)
            {
                DBConn.Tran.Rollback();
                DBConn.DisposeSqlTransaction(DBConn.Tran);
                DBConn.DisposeSqlConnection(DBConn.Cmd);
                //Interaction.MsgBox(ex.Message);
                return false;
            }
        }

        public static object ExecuteScalar(string QryStr)
        {

            try
            {

                DBConn.SqlConnectionOpen();
                DBConn.Cmd = DBConn.Cnn.CreateCommand();
                DBConn.Cmd.CommandType = CommandType.Text;
                DBConn.Cmd.CommandText = QryStr;
                return DBConn.Cmd.ExecuteScalar();

            }
            catch (SqlException ex)
            {
                return null;
            }
            finally
            {
                DBConn.DisposeSqlConnection(DBConn.Cmd);
            }

        }

        public static object ExecuteScalar(ref SqlCommand _Cmd)
        {

            try
            {

                DBConn.SqlConnectionOpen();

                _Cmd.Connection = DBConn.Cnn;
                return _Cmd.ExecuteScalar();

            }
            catch (SqlException ex)
            {
                return null;
            }
            finally
            {
                DBConn.DisposeSqlConnection(_Cmd);
            }

        }
        #endregion



        public static DataTable GetDataTable(string QryStr, string TableName = "DataTalble1", bool useapi = true)
        {
            DataTable objDT = new DataTable(TableName);

            try
            {


                SqlConnection _Cnn = new SqlConnection();
                SqlCommand _Cmd = new SqlCommand();
                try
                {


                    if (_Cnn.State == ConnectionState.Open) { _Cnn.Close(); };
                    _Cnn.ConnectionString = _dBConntext.getConnectionString();
                    _Cnn.Open();
                    _Cmd = _Cnn.CreateCommand();

                    var _Adepter = new SqlDataAdapter(_Cmd);
                    _Adepter.SelectCommand.CommandTimeout = 0;
                    _Adepter.SelectCommand.CommandType = CommandType.Text;
                    _Adepter.SelectCommand.CommandText = QryStr;
                    _Adepter.Fill(objDT);
                    _Adepter.Dispose();

                    DisposeSqlConnection(_Cmd);
                    DisposeSqlConnection(_Cnn);
                }
                catch (Exception ex)
                {
                    DisposeSqlConnection(_Cmd);
                    DisposeSqlConnection(_Cnn);
                }


            }
            catch { }




            return objDT;
        }

        public static void GetDataSet(string QryStr, ref DataSet objDataSet, string DefaultTableName = null)
        {
             
            SqlConnection _Cnn = new SqlConnection();
            SqlCommand _Cmd = new SqlCommand();
            try
            {


                if (_Cnn.State == ConnectionState.Open) { _Cnn.Close(); };
                _Cnn.ConnectionString = _dBConntext.getConnectionString();
                _Cnn.Open();
                _Cmd = _Cnn.CreateCommand();

                var _Adepter = new SqlDataAdapter(_Cmd);
                _Adepter.SelectCommand.CommandTimeout = 0;
                _Adepter.SelectCommand.CommandType = CommandType.Text;
                _Adepter.SelectCommand.CommandText = QryStr;
                _Adepter.Fill(objDataSet);
                _Adepter.Dispose();

                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
            }
            catch (Exception ex)
            {
                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
            }

            //return objDataSet;
        }

        public static DataTable GetDataTableConectstring(string QryStr, string _ConnectionString, string TableName = "DataTalble1")
        {
            DataTable objDT = new DataTable(TableName);

            SqlConnection _Cnn = new SqlConnection();
            SqlCommand _Cmd = new SqlCommand();
            try
            {


                if (_Cnn.State == ConnectionState.Open) { _Cnn.Close(); };
                _Cnn.ConnectionString = _dBConntext.getConnectionString();
                _Cnn.Open();
                _Cmd = _Cnn.CreateCommand();

                var _Adepter = new SqlDataAdapter(_Cmd);
                _Adepter.SelectCommand.CommandTimeout = 0;
                _Adepter.SelectCommand.CommandType = CommandType.Text;
                _Adepter.SelectCommand.CommandText = QryStr;
                _Adepter.Fill(objDT);
                _Adepter.Dispose();

                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
            }
            catch (Exception ex)
            {
                DisposeSqlConnection(_Cmd);
                DisposeSqlConnection(_Cnn);
            }

            return objDT;
        }

        public static DataTable GetDataTableOnbeginTrans(string QryStr, string DefaultTableName = "DataTalble1")
        {
            DataTable objDT = new DataTable(DefaultTableName);
            SqlCommand _cmd = null;

            try
            {

                if (Tran != null)
                {
                    _cmd = new SqlCommand(QryStr, DBConn.Cnn, Tran);
                }
                else
                {
                    _cmd = new SqlCommand(QryStr, DBConn.Cnn);
                }

                var _Adepter = new SqlDataAdapter(_cmd);
                _Adepter.SelectCommand.CommandTimeout = 0;
                _Adepter.Fill(objDT);
                _Adepter.Dispose();

                _cmd.Dispose();

            }
            catch (Exception ex)
            {
                _cmd.Dispose();
            }

            return objDT;

        }

        public static string GetFieldConectstring(string strSql, string _ConnecttionString, object defaultValue = null)
        {
            DataTable dt = new DataTable();
            string _Value = Convert.ToString(defaultValue);

            try
            {
                dt = GetDataTableConectstring(strSql, _ConnecttionString);


                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow R in dt.Rows)
                    {
                        if (R[0] == DBNull.Value) { }
                        else { _Value = R[0].ToString(); };
                        break;
                    };
                }

                dt.Dispose();
            }
            catch (Exception ex)
            {
            }


            return _Value;
        }

        public static string GetFieldByName(string strSql, string FieldName, object defaultValue = null, bool useapi = true)
        {
            DataTable dt = new DataTable();
            string _Value = Convert.ToString(defaultValue);

            try
            {
                dt = GetDataTable(strSql, "table1", useapi);
                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow R in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(FieldName) & dt.Columns.IndexOf(FieldName) >= 0)
                        {
                            if (R[FieldName] == DBNull.Value) { }
                            else { _Value = R[FieldName].ToString(); };
                        }
                        else
                        {
                            if (R[0] == DBNull.Value) { }
                            else { _Value = R[0].ToString(); };
                        }
                        break;
                    }
                }
                else
                {
                    _Value = defaultValue.ToString();
                }

            }
            catch (Exception ex)
            {
            }

            dt.Dispose();
            return _Value;
        }

        public static string GetFieldOnBeginTrans(string strSql, object defaultValue = null)
        {
            DataTable dt = new DataTable();
            string _Value = defaultValue.ToString();

            try
            {
                dt = GetDataTableOnbeginTrans(strSql);

                if (dt.Rows.Count != 0)
                {
                    foreach (DataRow R in dt.Rows)
                    {

                        if (R[0] == DBNull.Value) { }
                        else { _Value = R[0].ToString(); };
                        break;
                    }
                };


            }
            catch (Exception ex)
            {
            }

            dt.Dispose();
            return _Value;

        }

        public static string GetFieldByNameOnBeginTrans(string strSql, string FieldName, object defaultValue = null)
        {
            DataTable dt = new DataTable();
            string _Value = Convert.ToString(defaultValue);

            try
            {
                dt = GetDataTableOnbeginTrans(strSql);

                if (dt.Rows.Count != 0)
                {

                    foreach (DataRow R in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(FieldName) & dt.Columns.IndexOf(FieldName) >= 0)
                        {
                            if (R[FieldName] == DBNull.Value) { }
                            else { _Value = R[FieldName].ToString(); };
                        }
                        else
                        {
                            if (R[0] == DBNull.Value) { }
                            else { _Value = R[0].ToString(); };
                        }
                        break;
                    }

                };

            }
            catch (Exception ex)
            {
            }

            dt.Dispose();
            return _Value;

        }

    }


}