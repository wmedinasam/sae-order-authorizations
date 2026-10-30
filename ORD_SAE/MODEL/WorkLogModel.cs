using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    public class WorkLogModel
    {
        public string correlator_id { get; set; }
        public DateTime fecha_registro { get; set; }
        public string no_orden { get; set; }
        public string proveedor { get; set; }
        public string descripcion { get; set; }
        public string usuario { get; set; }

        private readonly DataAction _data = new DataAction();


        public ResponseModel<List<WorkLogModel>> getWorkLog(string fIni, string fFin)
        {
            ResponseModel<List<WorkLogModel>> res = new ResponseModel<List<WorkLogModel>>();
            List<WorkLogModel> lst = new List<WorkLogModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetWorkLog(fIni, fFin).Tables[0];
                lst = dt.AsEnumerable().Select(fila => new WorkLogModel
                {
                    correlator_id = fila["CorrelatorId"].ToString(),
                    fecha_registro = Convert.ToDateTime(fila["FechaRegistro"].ToString()),
                    no_orden = fila["NoOrden"].ToString(),
                    proveedor = fila["Proveedor"].ToString(),
                    descripcion = fila["Descripcion"].ToString(),
                    usuario = fila["Usuario"].ToString()
                }).ToList<WorkLogModel>();
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            res.Message = message;
            res.Success = success;
            res.Object = lst;
            return res;
        }

        public ResponseModel<List<WorkLogModel>> getWorkLog(string fIni, string fFin, string texto)
        {
            ResponseModel<List<WorkLogModel>> res = new ResponseModel<List<WorkLogModel>>();
            List<WorkLogModel> lst = new List<WorkLogModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetWorkLog(fIni, fFin, texto).Tables[0];
                lst = dt.AsEnumerable().Select(fila => new WorkLogModel
                {
                    correlator_id = fila["CorrelatorId"].ToString(),
                    fecha_registro = Convert.ToDateTime(fila["FechaRegistro"].ToString()),
                    no_orden = fila["NoOrden"].ToString(),
                    proveedor = fila["Proveedor"].ToString(),
                    descripcion = fila["Descripcion"].ToString(),
                    usuario = fila["Usuario"].ToString()
                }).ToList<WorkLogModel>();
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            res.Message = message;
            res.Success = success;
            res.Object = lst;
            return res;
        }

        public ResponseModel<List<WorkLogModel>> getAllWorkLog()
        {
            ResponseModel<List<WorkLogModel>> res = new ResponseModel<List<WorkLogModel>>();
            List<WorkLogModel> lst = new List<WorkLogModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetAllWorkLog().Tables[0];
                lst = dt.AsEnumerable().Select(fila => new WorkLogModel
                {
                    correlator_id = fila["CorrelatorId"].ToString(),
                    fecha_registro = Convert.ToDateTime(fila["FechaRegistro"].ToString()),
                    no_orden = fila["NoOrden"].ToString(),
                    proveedor = fila["Proveedor"].ToString(),
                    descripcion = fila["Descripcion"].ToString(),
                    usuario = fila["Usuario"].ToString()
                }).ToList<WorkLogModel>();
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            res.Message = message;
            res.Success = success;
            res.Object = lst;
            return res;
        }
        public ResponseModel<object> ActionWorkLog(string action, string idOrder, string userId, string idSupplier)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            try
            {
                DataTable dt = new DataTable();
                dt = _data.ActionWorkLog(action,
                                    idOrder,
                                    userId,
                                    idSupplier).Tables[0];
                var lst = dt.AsEnumerable().Select(fila => new ResponseModel<object>
                {
                    Message = fila["Message"].ToString(),
                    Success = Convert.ToBoolean(fila["Success"])
                }).ToList<ResponseModel<object>>();
                res = lst.FirstOrDefault();
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Message = ex.Message;
            }
            return res;
        }

    }
}
