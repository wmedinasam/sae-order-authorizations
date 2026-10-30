using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    public class PurshaseOrderModel
    {
        public bool Selected { get; set; }
        public string Id { get; set; }
        public string IdOrder { get; set; }
        public string IdSupplier { get; set; }
        public string Vendor { get; set; }
        public DateTime OrdDate { get; set; }
        public string State { get; set; }
        public string Observations { get; set; }
        public double Total { get; set; }
        public string Empresa { get; set; }
        public string SellerEmail { get; set; }

        public string _bottonAutorizar { get; set; }
        public string _bottonRechazar { get; set; }

        public string motivo { get; set; }

        private readonly DataAction _data = new DataAction();
        public ResponseModel<List<PurshaseOrderModel>> GetOrders(string empresaId, string db, string sufix, bool esAdministrador)
        {
            ResponseModel<List<PurshaseOrderModel>> res = new ResponseModel<List<PurshaseOrderModel>>();
            List<PurshaseOrderModel> lst = new List<PurshaseOrderModel>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetOrders(empresaId, db, sufix).Tables[0];
                lst = dt.AsEnumerable().Select(fila => new PurshaseOrderModel
                {
                    Id = fila["ID"].ToString(),
                    IdOrder = fila["IdOrder"].ToString(),
                    IdSupplier = fila["IdSupplier"].ToString(),
                    Vendor = fila["Vendor"].ToString(),
                    OrdDate = Convert.ToDateTime(fila["OrdDate"].ToString()),
                    State = fila["Status"].ToString(),
                    Observations = fila["Observations"].ToString(),
                    Total = Convert.ToDouble(fila["Total"].ToString()),
                    Empresa = fila["Empresa"].ToString(),
                    SellerEmail = fila["SellerEmail"].ToString(),
                    _bottonAutorizar = esAdministrador ? "Visible" : "Hidden",
                    _bottonRechazar = esAdministrador ? "Visible" : "Hidden"
                }).ToList<PurshaseOrderModel>();
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
        public ResponseModel<object> ActionPurchaseOrder(PurshaseOrderModel model, string action, string empresaId, string db, string sufix)
        {
            ResponseModel<object> res = new ResponseModel<object>();
            try
            {
                DataTable dt = new DataTable();
                dt = _data.ActionPurchaseOrder(action,
                                model.IdOrder,
                                empresaId,
                                db,
                                sufix).Tables[0];
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
