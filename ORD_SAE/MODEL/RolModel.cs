using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    class RolModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsAdministrador { get; set; }
        public bool Estado { get; set; }

        private readonly DataAction _data = new DataAction();

        public ResponseModel<List<RolModel>> GetRol()
        {
            ResponseModel<List<RolModel>> res = new ResponseModel<List<RolModel>>();
            List<RolModel> lst = new List<RolModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetRol().Tables[0];
                lst = dt.AsEnumerable().Select(fila => new RolModel
                {
                    Id = fila["Id"].ToString(),
                    Nombre = fila["Nombre"].ToString(),
                    Descripcion = fila["Descripcion"].ToString(),
                    EsAdministrador = Convert.ToBoolean(fila["EsAdministrador"]),
                    Estado = Convert.ToBoolean(fila["Estado"])
                }).ToList<RolModel>();
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
        public ResponseModel<object> ActionRole(RolModel model, string action)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            try
            {
                DataTable dt = new DataTable();
                dt = _data.ActionRole(action,
                                      model.Id,
                                      model.Nombre,
                                      model.Descripcion,
                                      model.EsAdministrador,
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
