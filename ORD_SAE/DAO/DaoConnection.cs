using System.Data.SqlClient;
using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using ORD_SAE.MODEL;
using Newtonsoft.Json;

namespace ORD_SAE.DAO
{
    class DaoConnection
    {
        public static SqlConnection SqConnection()
        {
            var cn = Connection();
            return cn;
        }

        private static SqlConnection Connection()
        {
            HandleCfg _handleCfg = new HandleCfg();
            ConfigurationModel _configurationModel = new ConfigurationModel();
            var json = _handleCfg.ReadCfg(_configurationModel.path);
            _configurationModel = JsonConvert.DeserializeObject<ConfigurationModel>(json);
            string connection = string.Empty;
            connection = @"Server=" + _configurationModel.Key.Instancia.DesencriptarDatos() + ";Database=ORDENES_COMPRA;User Id=" + _configurationModel.User + "; Password=" + _configurationModel.Password + ";";
            return new SqlConnection(connection);
        }
        //public string Company()
        //{
        //    HandleCfg _handleCfg = new HandleCfg();
        //    ConfigurationModel _configurationModel = new ConfigurationModel();
        //    var json = _handleCfg.ReadCfg(_configurationModel.path);
        //    _configurationModel = JsonConvert.DeserializeObject<ConfigurationModel>(json);
        //    return _configurationModel.BdSae.Substring(10,2);
        //}

    }
}
