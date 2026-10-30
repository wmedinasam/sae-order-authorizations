using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    class SystemConfugurationModel
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        private readonly DataAction _data = new DataAction();
        public ResponseModel<List<SystemConfugurationModel>> GetConfiguration()
        {
            ResponseModel<List<SystemConfugurationModel>> res = new ResponseModel<List<SystemConfugurationModel>>();
            List<SystemConfugurationModel> lst = new List<SystemConfugurationModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetConfiguration().Tables[0];
                lst = dt.AsEnumerable().Select(fila => new SystemConfugurationModel
                {
                    Id = fila["ID"].ToString(),
                    Key = fila["KEY"].ToString(),
                    Value = fila["VALUE"].ToString()
                }).ToList<SystemConfugurationModel>();
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
        public ResponseModel<object> ActionConfiguration(SystemConfugurationModel model, string action)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            try
            {
                DataTable dt = new DataTable();
                dt = _data.ActionConfiguration(action,
                                      model.Id,
                                      model.Key,
                                      model.Value).Tables[0];
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
