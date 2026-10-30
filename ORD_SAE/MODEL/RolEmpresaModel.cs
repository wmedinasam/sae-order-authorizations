using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    class RolEmpresaModel
    {
        public string EmpresaId { get; set; }
        public string RolId { get; set; }
        public string Nombre { get; set; }
        public string db { get; set; }
        public string sufix { get; set; }
        public bool Estado { get; set; }
        private readonly DataAction _data = new DataAction();
        public ResponseModel<List<RolEmpresaModel>> GetRolEmpresa(string rolId)
        {
            ResponseModel<List<RolEmpresaModel>> res = new ResponseModel<List<RolEmpresaModel>>();
            List<RolEmpresaModel> lst = new List<RolEmpresaModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetRolEmpresa(rolId).Tables[0];
                lst = dt.AsEnumerable().Select(fila => new RolEmpresaModel
                {
                    EmpresaId = fila["EmpresaId"].ToString(),
                    Nombre = fila["Nombre"].ToString(),
                    Estado = Convert.ToBoolean(fila["Estado"]),
                }).ToList<RolEmpresaModel>();
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
        public ResponseModel<List<RolEmpresaModel>> GetEmpresasAsignadas(string rolId)
        {
            ResponseModel<List<RolEmpresaModel>> res = new ResponseModel<List<RolEmpresaModel>>();
            List<RolEmpresaModel> lst = new List<RolEmpresaModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetEmpresasAsignadas(rolId).Tables[0];
                lst = dt.AsEnumerable().Select(fila => new RolEmpresaModel
                {
                    EmpresaId = fila["EmpresaId"].ToString(),
                    Nombre = fila["Nombre"].ToString(),
                    db = fila["DB_SAE"].ToString(),
                    sufix = fila["Sufijo"].ToString()
                }).ToList<RolEmpresaModel>();
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
        public ResponseModel<object> ActionRolEmpresa(string action, RolEmpresaModel model)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            try
            {
                DataTable dt = new DataTable();
                dt = _data.ActionRolEmpresa(action,
                                    model.RolId,
                                    model.EmpresaId).Tables[0];

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
