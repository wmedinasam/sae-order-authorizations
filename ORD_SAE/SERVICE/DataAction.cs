using ORD_SAE.DAO;
using ORD_SAE.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.Service
{
    class DataAction
    {
        #region Rol
        public DataSet GetRol()
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format("SELECT [Id] " +
                                            ",[Nombre] " +
                                            ",[Descripcion] " +
                                            ",[EsAdministrador] " +
                                            ",[Estado] " +
                                    "FROM [dbo].[ROL] ");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }
        public DataSet ActionRole(
                                string action,
                                string id,
                                string nombre,
                                string descripcion,
                                bool esAdministrador,
                                bool estado)
        {
            DataBaseAction dbaction = new DataBaseAction();
            string query = string.Empty;
            if (action == "Insert")
            {
                query = string.Format("INSERT INTO [dbo].[ROL] " +
                                                "([Nombre] " +
                                                ",[Descripcion] " +
                                                ",[EsAdministrador] " +
                                                ",[Estado]) " +
                                            "VALUES " +
                                                $"('{nombre}'" +
                                                $",'{descripcion}' " +
                                                $",'{esAdministrador}' " +
                                                $",'{estado}')");
            }
            else
            {
                query = string.Format("UPDATE [dbo].[ROL] " +
                                         $"SET [Nombre] = '{nombre}' " +
                                            $",[Descripcion] = '{descripcion}' " +
                                            $",[EsAdministrador] = '{esAdministrador}' " +
                                            $",[Estado] = '{estado}' " +
                                        $"WHERE [Id] = '{id}' ");
            }
            var ds = dbaction.ExecuteMSSqlQuery(query);
            return ds;
        }
        #endregion
        #region Usuario
        public DataSet GetUser()
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format("SELECT U.[ID] " +
                                            ",U.[FULL_NAME] " +
                                             ",U.[NICK_NAME] " +
                                             ",U.[PASSWORD] " +
                                             ",U.[EMAIL] " +
                                             ",U.[SEND_MAIL] " +
                                             ",U.[ROL_ID] RolId " +
                                             ",R.[Descripcion] Rol " +
                                             ",R.[EsAdministrador] " +
                                             ",U.[ACTIVATED_DATE] " +
                                             ",U.[USER_ACTIVATES] " +
                                             "FROM [dbo].[WIS_USER] U " +
                                         "INNER JOIN [dbo].[ROL] R " +
                                         "ON R.Id = U.ROL_ID");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet ActionUser(
                                string action,
                                string id,
                                string fullName,
                                string nick,
                                string password,
                                string email,
                                bool sendMail,
                                string rol,
                                string userActivates
                                )
        {
            DataBaseAction dbaction = new DataBaseAction();
            string query = string.Empty;
            if (action == "Insert")
            {
                query = string.Format("INSERT INTO [dbo].[WIS_USER] " +
           "([FULL_NAME] " +
           ",[NICK_NAME] " +
           ",[PASSWORD] " +
           ",[EMAIL] " +
           ",[SEND_MAIL] " +
           ",[ROL_ID] " +
           ",[ACTIVATED_DATE] " +
           ",[USER_ACTIVATES]) " +
     "VALUES  " +
           $"('{fullName}' " +
           $",'{nick}' " +
           $",'{password}' " +
           $",'{email}' " +
           $",'{sendMail}' " +
           $",'{rol}' " +
           $",GETDATE() " +
           $",'{userActivates}'); SELECT '' Message,'True' as Success; ");
            }
            else
            {
                query = string.Format("UPDATE [dbo].[WIS_USER] " +
                                    $"SET [FULL_NAME] = '{fullName}' " +
                                    $",[NICK_NAME] = '{nick}' " +
                                    $",[PASSWORD] = '{password}' " +
                                    $",[EMAIL] = '{email}' " +
                                    $",[SEND_MAIL] = '{sendMail}' " +
                                    $",[ROL_ID] = '{rol}' " +
                                $"WHERE[ID] = '{id}'; SELECT '' Message,'True' as Success; ");
            }
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }
        #endregion
        #region Empresa
        public DataSet GetEmpresas()
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format("SELECT [Id] " +
                                            ",[Nombre] " +
                                            ",[Direccion] " +
                                            ",[DB_SAE] " +
                                            ",[Sufijo] " +
                                            ",[Estado] " +
                                            ",CASE WHEN [Estado] = 1 THEN 'ACTIVO' ELSE 'INACTIVO' END AS [EstadoGrid] " +
                                    "FROM [dbo].[EMPRESA]");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }
        public DataSet ActionEmpresa(
                                string action,
                                string id,
                                string nombre,
                                string direccion,
                                string bd_sae,
                                string sufijo,
                                bool estado)
        {
            DataBaseAction dbaction = new DataBaseAction();
            string query = string.Empty;
            if (action == "Insert")
            {
                query = string.Format("INSERT INTO [dbo].[EMPRESA] " +
                                                "([Nombre] " +
                                                ",[Direccion] " +
                                                ",[DB_SAE] " +
                                                ",[Sufijo] " +
                                                ",[Estado]) " +
                                            "VALUES " +
                                                $"('{nombre}' " +
                                                $",'{direccion}' " +
                                                $",'{bd_sae}' " +
                                                $",'{sufijo}' " +
                                                $",'{estado}')");
            }
            else
            {
                query = string.Format("UPDATE [dbo].[EMPRESA] " +
                                         $"SET [Nombre] = '{nombre}' " +
                                            $",[Direccion] = '{direccion}' " +
                                            $",[DB_SAE] = '{bd_sae}' " +
                                            $",[Sufijo] = '{sufijo}' " +
                                            $",[Estado] = '{estado}' " +
                                        $"WHERE [Id] = '{id}' ");
            }
            var ds = dbaction.ExecuteMSSqlQuery(query);
            return ds;
        }
        #endregion
        #region RolEmpresa
        public DataSet GetRolEmpresa(string rolId)
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format("SELECT " +
                                            "E.Id EmpresaId " +
                                            ", E.Nombre, " +
                                            "CASE WHEN RE.RolId IS NULL THEN 0 ELSE 1 END Estado " +
                                       "FROM " +
                                                    "(" +
                                                    "SELECT RolId, EmpresaId " +
                                                    "FROM ROL_EMPRESA " +
                                                    $"WHERE RolId = '{rolId}' " +
                                                    ") RE " +
                                            "RIGHT JOIN EMPRESA E " +
                                            "ON RE.EmpresaId = E.Id; ");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }
        public DataSet GetEmpresasAsignadas(string rolId)
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format("SELECT " +
                                        "Nombre " +
                                        ",EmpresaId " +
                                        ",[DB_SAE] " +
                                        ",[Sufijo] " +
                                        "FROM ROL_EMPRESA RE " +
                                        "INNER JOIN EMPRESA E " +
                                        "ON E.Id = RE.EmpresaId " +
                                        $"WHERE RE.RolId = '{rolId}' ");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }
        public DataSet ActionRolEmpresa(
                                string action,
                                string rolId,
                                string empresaId
                                )
        {
            DataBaseAction dbaction = new DataBaseAction();
            string query = string.Empty;
            if (action == "Insert")
            {
                query = string.Format("INSERT INTO [dbo].[ROL_EMPRESA] " +
                                                    "([RolId] " +
                                                    ",[EmpresaId]) " +
                                            "VALUES " +
                                                    $"('{rolId}' " +
                                                    $",'{empresaId}')");
            }
            else
            {
                query = string.Format("DELETE FROM [dbo].[ROL_EMPRESA] " +
                                        $"WHERE [RolId] = '{rolId}' AND [EmpresaId] = '{empresaId}' ");
            }
            var ds = dbaction.ExecuteMSSqlQuery(query);
            return ds;
        }
        #endregion
        public DataSet GetConfiguration()
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format("SELECT [ID] " +
                                            ",[KEY] " +
                                            ",[VALUE] " +
                                    "FROM [dbo].[WIS_CONFIGURATION]");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet ActionConfiguration(
                               string action,
                               string id,
                               string key,
                               string value)
        {
            DataBaseAction dbaction = new DataBaseAction();
            string query = string.Empty;
            if (action == "Insert")
            {
                query = string.Format("INSERT INTO [dbo].[WIS_CONFIGURATION] " +
                                                "([KEY] " +
                                                ",[VALUE]) " +
                                        "VALUES " +
                                                $"('{key}'" +
                                                $",'{value}'); SELECT '' Message,'True' as Success; ");
            }
            else
            {
                query = string.Format("UPDATE [dbo].[WIS_CONFIGURATION] " +
                                                $" SET [VALUE] = '{value}' " +
                                        $"WHERE [KEY]= '{key}'; SELECT '' Message,'True' as Success; ");
            }
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet GetOrders(string empresaId, string db, string sufix)
        {
            DataBaseAction dbaction = new DataBaseAction();
            DaoProcedure procedure = new DaoProcedure();
            procedure.ProcedureName = "Sp_GetOrders";
            procedure.Parameter.Add(new DaoParameter { Name = "@EmpresaId", Value = empresaId, Type = System.Data.DbType.String, Out = false });
            procedure.Parameter.Add(new DaoParameter { Name = "@pDataBase", Value = db, Type = System.Data.DbType.String, Out = false });
            procedure.Parameter.Add(new DaoParameter { Name = "@pSuffix", Value = sufix, Type = System.Data.DbType.String, Out = false });
            var ds = dbaction.GetMSSqlDataFromSp(procedure);
            return ds;
        }

        public DataSet ActionPurchaseOrder(
                                string action,
                                string IdOrder,
                                string EmpresaId,
                                string db,
                                string sufix)
        {
            DataBaseAction dbaction = new DataBaseAction();
            string query = string.Empty;
            if (action == "UpdateOrderAprobada")
            {
                query = string.Format($"UPDATE WIS_Orders SET Status='O' WHERE IdOrder='{IdOrder}' AND EmpresaId = '{EmpresaId}'; UPDATE {db}.dbo.COMPO{sufix} SET BLOQ='N' WHERE CVE_DOC='{IdOrder}' ; SELECT '' Message,'True' as Success; ");
            }
            if (action == "UpdateOrderRechazada")
            {
                query = string.Format($"UPDATE WIS_Orders SET Status='R' WHERE IdOrder='{IdOrder}' AND EmpresaId = '{EmpresaId}';UPDATE {db}.dbo.COMPO{sufix} SET BLOQ='N', STATUS='C' WHERE CVE_DOC='{IdOrder}' ; SELECT '' Message,'True' as Success; ");
            }
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet GetWorkLog(string fIni, string fFin)
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format($"SELECT CorrelatorId, FechaRegistro, NoOrden, Proveedor, Descripcion, Usuario FROM " +
                $"WIS_viewLogView " +
                $"WHERE CONVERT(DATE,FechaRegistro,102) BETWEEN '{fIni}' AND '{fFin}'");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet GetWorkLog(string fIni, string fFin, string txtBusqueda)
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format($"SELECT CorrelatorId, FechaRegistro, NoOrden, Proveedor, Descripcion, Usuario FROM " +
                $"WIS_viewLogView " +
                $"WHERE (CONVERT(DATE,FechaRegistro,102) BETWEEN '{fIni}' AND '{fFin}') " +
                $"AND Descripcion LIKE '%{txtBusqueda.ToLower()}%'");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet GetAllWorkLog()
        {
            DataBaseAction dbaction = new DataBaseAction();
            var query = string.Format($"SELECT CorrelatorId, FechaRegistro, NoOrden,Proveedor, Descripcion, Usuario FROM " +
                $"WIS_viewLogView ");
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet ActionWorkLog(
                                string action,
                                string IdOrder,
                                string userId,
                                string idSupplier)
        {
            DataBaseAction dbaction = new DataBaseAction();
            string query = string.Empty;
            if (action == "InsertAprobada")
            {
                query = string.Format($"INSERT INTO [dbo].[WIS_WorkLog] " +
                                                   "([CorrelatorId] " +
                                                  ",[LogDate] " +
                                                  ",[IdOrder] " +
                                                   ",[Descr] " +
                                                   ",[UserId]" +
                                                   ",[IdSupplier]) " +
                                        "VALUES " +
                                                   "(CONCAT(18, '_', SUBSTRING(CONVERT(varchar(40), NEWID()), 0, 9)) " +
                                                   ", GETDATE() " +
                                                   $", '{IdOrder}' " +
                                                   $", 'Se ha aprobado la Orden de Compra Numero: {IdOrder} desde el Sistema SAE' " +
                                                   $", '{userId}','{idSupplier}'); SELECT '' Message,'True' as Success; ");
            }
            else if (action == "InsertRechazada")
            {
                query = string.Format($"INSERT INTO [dbo].[WIS_WorkLog] " +
         "([CorrelatorId] " +
        ",[LogDate] " +
        ",[IdOrder] " +
         ",[Descr] " +
         ",[UserId]" +
         ",[IdSupplier]) " +
   "VALUES " +
         "(CONCAT(18, '_', SUBSTRING(CONVERT(varchar(40), NEWID()), 0, 9)) " +
         ", GETDATE() " +
         $", '{IdOrder}' " +
         $", 'Se ha rechazado la Orden de Compra Numero: {IdOrder} desde el Sistema SAE' " +
         $", '{userId}','{idSupplier}'); SELECT '' Message,'True' as Success; ");
            }
            var ds = dbaction.GetMSSqlDataFromQuery(query);
            return ds;
        }

        public DataSet GetOrderHead(string IdOrder, string db, string sufix)
        {
            DataBaseAction dbaction = new DataBaseAction();
            DaoProcedure procedure = new DaoProcedure();

            procedure.ProcedureName = "Sp_OrderHeader";
            procedure.Parameter.Add(new DaoParameter { Name = "@pDataBase", Value = db, Type = System.Data.DbType.String, Out = false });
            procedure.Parameter.Add(new DaoParameter { Name = "@pSuffix", Value = sufix, Type = System.Data.DbType.String, Out = false });
            procedure.Parameter.Add(new DaoParameter { Name = "@IdOrder", Value = IdOrder, Type = System.Data.DbType.String, Out = false });
            var ds = dbaction.GetMSSqlDataFromSp(procedure);
            return ds;
        }

        public DataSet GetOrderDetail(string IdOrder, string db, string sufix)
        {
            DataBaseAction dbaction = new DataBaseAction();
            DaoProcedure procedure = new DaoProcedure();
            procedure.ProcedureName = "Sp_OrderDetail";
            procedure.Parameter.Add(new DaoParameter { Name = "@pDataBase", Value = db, Type = System.Data.DbType.String, Out = false });
            procedure.Parameter.Add(new DaoParameter { Name = "@pSuffix", Value = sufix, Type = System.Data.DbType.String, Out = false });
            procedure.Parameter.Add(new DaoParameter { Name = "@IdOrder", Value = IdOrder, Type = System.Data.DbType.String, Out = false });
            var ds = dbaction.GetMSSqlDataFromSp(procedure);
            return ds;
        }

    }
}
