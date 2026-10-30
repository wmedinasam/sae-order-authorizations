using System.Data.SqlClient;
using ORD_SAE.DAO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.DB
{
    class DataBaseAction
    {
      
        public DataSet GetMSSqlDataFromSp(DaoProcedure procedure)
        {
            DataSet ds = new DataSet("datos");
            try
            {
                var cn = DaoConnection.SqConnection();
                cn.Open();
                SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procedure.ProcedureName;
                if (procedure.Parameter != null)
                {
                    foreach (var item in procedure.Parameter)
                    {
                        cmd.Parameters.AddWithValue(item.Name, item.Value).DbType = item.Type;
                    }
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                cn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return ds;
        }
        public DataSet GetMSSqlDataFromQuery(string query)
        {
            DataSet ds = new DataSet("datos");
            try
            {
                var cn = DaoConnection.SqConnection();
                cn.Open();
                SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                cn.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return ds;
        }
        public DataSet ExecuteMSSqlQuery(string query)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            dt.Columns.Add("Success");
            dt.Columns.Add("Message");
            dt.Columns.Add("Afected");
            var cn = DaoConnection.SqConnection();
            try
            {
                cn.Open();
                SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                var afected = cmd.ExecuteNonQuery();
                if (afected > 0)
                {
                    dt.Rows.Add(true, "", afected.ToString());
                }
                else
                {
                    dt.Rows.Add(false, "Error no identificado", afected.ToString());
                }
                ds.Tables.Add(dt);
                cn.Close();
            }
            catch (Exception ex)
            {
                dt.Rows.Add(false, ex.Message, "0");
                throw ex;
            }
            return ds;
        }
    }
}
