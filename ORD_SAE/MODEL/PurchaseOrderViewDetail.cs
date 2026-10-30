using ORD_SAE.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ORD_SAE.MODEL
{
    public class PurchaseOrderViewDetail
    {
        public string IdProd { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }

        public double ExchangeRate { get; set; }

        public string Currency { get; set; }

        public double Price { get; set; }

        public double VAT { get; set; }

        public double Disscount { get; set; }

        public double TotalItem { get; set; }
        
        public double QuotedPrice { get; set; }

        public double QuotedTotal { get; set; }

        public string QuotedCurrency { get; set; }

        public double QuotedExchangeRate { get; set; }

        public double CurrentStock { get; set; }

        public double ActualPrice { get; set; }

        public double Edge { get; set; } // Margen



        private readonly DataAction _data = new DataAction();

        public ResponseModel<List<PurchaseOrderViewDetail>> GetOrderDetail(PurshaseOrderModel order, string db, string sufix)
        {
            ResponseModel<List<PurchaseOrderViewDetail>> res = new ResponseModel<List<PurchaseOrderViewDetail>>();
            List<PurchaseOrderViewDetail> lst = new List<PurchaseOrderViewDetail>();
            bool success = true;
            string message = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                dt = _data.GetOrderDetail(order.IdOrder, db, sufix).Tables[0];
                lst = dt.AsEnumerable().Select(fila => new PurchaseOrderViewDetail
                {
                    IdProd = fila["IdProd"].ToString(),
                    Description = fila["Description"].ToString(),
                    Quantity = Convert.ToInt32(fila["Quantity"].ToString()),
                    ExchangeRate = Convert.ToDouble(fila["ExchangeRate"].ToString()),
                    Currency = fila["Currency"].ToString(),
                    Price = Convert.ToDouble(fila["Price"].ToString()),
                    VAT = Convert.ToDouble(fila["VAT"].ToString()),
                    Disscount = Convert.ToDouble(fila["Disscount"].ToString()),
                    TotalItem = Convert.ToDouble(fila["TotalItem"].ToString()),
                    QuotedPrice = Convert.ToDouble(fila["QuotedPrice"].ToString()),
                    QuotedTotal = Convert.ToDouble(fila["QuotedTotal"].ToString()),
                    QuotedCurrency = fila["QuotedCurrency"].ToString(),
                    QuotedExchangeRate = Convert.ToDouble(fila["QuotedExchangeRate"].ToString()),
                    CurrentStock = Convert.ToDouble(fila["CurrentStock"].ToString()),
                    ActualPrice = Convert.ToDouble(fila["ActualPrice"].ToString()),
                    Edge = Convert.ToDouble(fila["Edge"].ToString())
                }).ToList<PurchaseOrderViewDetail>();
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
