using ORD_SAE.MODEL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ORD_SAE.Service
{
    class HandleCfg
    {
        public ResponseModel<object> SaveCfg(string text, string path)
        {
            ResponseModel<object> response = new ResponseModel<object>();
            try
            {
                StreamWriter sw = new StreamWriter(path, false);
                sw.WriteLine(text.EncriptarDatos());
                sw.Close();
                response.Success = true;
                
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Success = false;
            }
            return response;
        }
        public string ReadCfg(string path)
        {
            string line = string.Empty;
            try
            {
                StreamReader sr = new StreamReader(path);
                line = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return line.DesencriptarDatos();
        }
        public  bool CfgExists(string path)
        {
            return File.Exists(path);
        }
    }
}
