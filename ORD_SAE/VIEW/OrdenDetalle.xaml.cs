using ORD_SAE.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ORD_SAE.VIEW
{
    /// <summary>
    /// Interaction logic for OrdenDetalle.xaml
    /// </summary>
    public partial class OrdenDetalle : Window
    {
        public OrdenDetalle(PurshaseOrderModel order, string db, string sufix)
        {
            InitializeComponent();
            PurchaseOrderView head = new PurchaseOrderView();
            PurchaseOrderViewDetail detail = new PurchaseOrderViewDetail();
           var respHead= head.GetOrderHead(order, db, sufix);
           var respDetail= detail.GetOrderDetail(order, db, sufix);
            if (respHead.Success && respDetail.Success)
            {
                LoadData(respHead.Object, respDetail.Object);
            }
            else
            {
                MessageBox.Show("Error: " + respHead.Message + " " + respDetail.Message, "Error Cargar Orden", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void minButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void LoadData(List<PurchaseOrderView> head,List<PurchaseOrderViewDetail> detail)
        {
            foreach (var item in head)
            {
                txtIdOrder.Text = item.IdOrder;
                txtSupplier.Text = item.Supplier;
                txtTotal.Text = item.Total.ToString("N4");
                txtDate.Text = Convert.ToDateTime(item.DateOrder).ToShortDateString();
            }
            dgDetail.ItemsSource = detail;
        }
    }
}
