using Newtonsoft.Json;
using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    public class ConfigurationModel
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string BdSae { get; set; }
        public KeyModel Key = new KeyModel();


        public string path = Directory.GetCurrentDirectory()+"\\Configuration.cfg";

        private readonly HandleCfg _data = new HandleCfg();
        public ResponseModel<ConfigurationModel> GetConfiguration()
        {
            ResponseModel<ConfigurationModel> response = new ResponseModel<ConfigurationModel>();
            ConfigurationModel item = new ConfigurationModel();
            bool success = true;
            string message = string.Empty;
            try
            {
                string dt;
                dt = _data.ReadCfg(path);
                item = JsonConvert.DeserializeObject<ConfigurationModel>(dt);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            response.Message = message;
            response.Success = success;
            response.Object = item;
            return response;
        }
        public ResponseModel<object> SaveConfiguration(ConfigurationModel model)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            bool success = true;
            string message = string.Empty;
            try
            {
                var user = JsonConvert.SerializeObject(model);
                _data.SaveCfg(user, path);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            res.Message = message;
            res.Success = success;
            return res;
        }
    }
}
