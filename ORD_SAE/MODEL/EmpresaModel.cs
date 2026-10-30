using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    class EmpresaModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string DB_SAE { get; set; }
        public string Sufijo { get; set; }
        public bool Estado { get; set; }
        public string EstadoGrid { get; set; }

        private readonly DataAction _data = new DataAction();

        public ResponseModel<List<EmpresaModel>> GetEmpresas()
        {
            ResponseModel<List<EmpresaModel>> res = new ResponseModel<List<EmpresaModel>>();
            List<EmpresaModel> lst = new List<EmpresaModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetEmpresas().Tables[0];
                lst = dt.AsEnumerable().Select(fila => new EmpresaModel
                {
                    Id = fila["Id"].ToString(),
                    Nombre = fila["Nombre"].ToString(),
                    Direccion = fila["Direccion"].ToString(),
                    DB_SAE = fila["DB_SAE"].ToString(),
                    Sufijo = fila["Sufijo"].ToString(),
                    EstadoGrid = fila["EstadoGrid"].ToString(),
                    Estado = Convert.ToBoolean(fila["Estado"])
                }).ToList<EmpresaModel>();
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
        public ResponseModel<object> ActionEmpresa(EmpresaModel model, string action)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            try
            {
                DataTable dt = new DataTable();
                dt = _data.ActionEmpresa(action,
                                      model.Id,
                                      model.Nombre,
                                      model.Direccion,
                                      model.DB_SAE,
                                      model.Sufijo,
                                      model.Estado).Tables[0];
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
