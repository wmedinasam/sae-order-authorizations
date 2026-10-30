using Newtonsoft.Json;
using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    public class UserModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Nick { get; set; }
        public string Password { get; set; }
        public string State { get; set; }
        public string Mail { get; set; }
        public bool SendMail { get; set; }
        public DateTime ActivatedDate { get; set; }
        public string RolId { get; set; }
        public string Rol { get; set; }
        public bool EsAdministrador { get; set; }
        public string UserActivates { get; set; }
        public string isLogin { get; set; }

        private readonly DataAction _data = new DataAction();

        public bool isModelValid(UserModel model)
        {
            var res = !(String.IsNullOrEmpty(model.FullName) || String.IsNullOrEmpty(model.Nick) || String.IsNullOrEmpty(model.Mail) || String.IsNullOrEmpty(model.Password) || String.IsNullOrEmpty(model.RolId));
            return res;
        }

        public ResponseModel<List<UserModel>> GetUser()
        {
            ResponseModel<List<UserModel>> res = new ResponseModel<List<UserModel>>();
            List<UserModel> lst = new List<UserModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetUser().Tables[0];
                lst = dt.AsEnumerable().Select(fila => new UserModel
                {
                    Id = fila["ID"].ToString(),
                    FullName = fila["FULL_NAME"].ToString(),
                    Nick = fila["NICK_NAME"].ToString(),
                    Password = fila["PASSWORD"].ToString(),
                    Mail = fila["EMAIL"].ToString(),
                    SendMail = Convert.ToBoolean(fila["SEND_MAIL"]),
                    RolId = fila["RolId"].ToString(),
                    Rol = fila["Rol"].ToString(),
                    EsAdministrador = Convert.ToBoolean(fila["EsAdministrador"])                    
                }).ToList<UserModel>();
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
        public ResponseModel<object> ActionUser(UserModel model, string action)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            try
            {
                DataTable dt = new DataTable();
                dt = _data.ActionUser(action,
                                      model.Id,
                                      model.FullName,
                                      model.Nick,
                                      model.Password,
                                      model.Mail,
                                      model.SendMail,
                                      model.RolId,
                                      model.UserActivates).Tables[0];
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
