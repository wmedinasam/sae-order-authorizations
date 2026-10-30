using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    public class PurchaseOrderView
    {
        public string IdOrder { get; set; }
        public string Supplier { get; set; }
        public double Total { get; set; }
        public string DateOrder { get; set; }


        private readonly DataAction _data = new DataAction();

        public ResponseModel<List<PurchaseOrderView>> GetOrderHead(PurshaseOrderModel order, string db, string sufix)
        {
            ResponseModel<List<PurchaseOrderView>> res = new ResponseModel<List<PurchaseOrderView>>();
            List<PurchaseOrderView> lst = new List<PurchaseOrderView>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetOrderHead(order.IdOrder, db, sufix).Tables[0];
                lst = dt.AsEnumerable().Select(fila => new PurchaseOrderView
                {
                    IdOrder = fila["IdOrder"].ToString(),
                    Supplier = fila["Supplier"].ToString(),
                    Total = Convert.ToDouble(fila["Total"].ToString()),
                    DateOrder = fila["DateOrder"].ToString()
                }).ToList<PurchaseOrderView>();
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

       
    }
}
