using ORD_SAE.DAO;
using ORD_SAE.MODEL;
using ORD_SAE.SERVICE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for Autorizaciones.xaml
    /// </summary>
    public partial class Autorizaciones : Window
    {
        UserModel userModel = new UserModel();
        PurshaseOrderModel _purshaseOrderModel = new PurshaseOrderModel();
        WorkLogModel workLogModel = new WorkLogModel();
        List<PurshaseOrderModel> _lstPurchaseOrders = new List<PurshaseOrderModel>();
        string OrderId = string.Empty;
        string email = string.Empty;
        List<string> lstOrderId = new List<string>();
        List<ResponseModel<object>> responseList = new List<ResponseModel<object>>();
        string _company = string.Empty;
        string isAprobed = string.Empty;
        string Empresa = string.Empty;
        public string _motivo = string.Empty;
        DaoConnection daoConnection = new DaoConnection();

        public Autorizaciones(UserModel userModel)
        {
            InitializeComponent();
            this.userModel = userModel;
            //_company = daoConnection.Company();
            GetRolEmpresa(userModel.RolId);
            hideButtons(userModel.EsAdministrador);
        }

        private void GetRolEmpresa(string rolId)
        {
            RolEmpresaModel rolEmpresa = new RolEmpresaModel();
            var response = rolEmpresa.GetEmpresasAsignadas(rolId);
            if (response.Success)
            {
                cbRolEmpresas.ItemsSource = response.Object;
                cbRolEmpresas.DisplayMemberPath = "Nombre";
            }
            else
            {
                MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void sendMail_Do(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var responseMail = MailService.SendMail(OrderId.Trim(), isAprobed, Empresa, email, _motivo);
            e.Result = responseMail;
        }
        private void hideButtons(bool EsAdministrador)
        {
            if (!EsAdministrador)
            {
                btnAutorizar.Visibility = Visibility.Hidden;
                btnRechazar.Visibility = Visibility.Hidden;
            }
        }


        private void sendMail_Completed(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as ResponseModel<object>;
            if (result.Success)
            {
                lblMailStatus.Content = "Correo enviado correctamente";
                lblMailStatus.Foreground = Brushes.Black;
            }
            else
            {
                lblMailStatus.Content = result.Message;
                lblMailStatus.Foreground = Brushes.Red;
            }
        }
        private void sendBulkMail_Do(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            foreach (var item in lstOrderId)
            {
                var responseMail = MailService.SendMail(item.Trim(), isAprobed, Empresa, email);
                responseList.Add(responseMail);
            }
            e.Result = responseList;
        }
        private void sendBulkMail_Completed(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as List<ResponseModel<object>>;
            foreach (var item in result)
            {
                if (item.Success)
                {
                    lblMailStatus.Content = "Correos enviados correctamente";
                }
                else
                {
                    MessageBox.Show(item.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //GetPurshaseOrders();
        }
        private void GetPurshaseOrders(string empresaId, string db, string sufix)
        {
            var response = _purshaseOrderModel.GetOrders(empresaId, db, sufix, this.userModel.EsAdministrador);
            if (response.Success)
            {
                _lstPurchaseOrders = response.Object.Where(x => x.State == "P").ToList();
                dataGrid.ItemsSource = _lstPurchaseOrders;
            }
            else
            {
                MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            if (cbRolEmpresas.SelectedIndex >= 0)
            {
                var selected = (RolEmpresaModel)cbRolEmpresas.SelectedItem;
                GetPurshaseOrders(selected.EmpresaId, selected.db, selected.sufix);
            }
        }
        private void SendPurshaseOrderConfirmation(PurshaseOrderModel model, bool isAproved = true)
        {
            var selected = (RolEmpresaModel)cbRolEmpresas.SelectedItem;
            var responsePurshase = _purshaseOrderModel.ActionPurchaseOrder(model, "UpdateOrder" + isAprobed, selected.EmpresaId, selected.db, selected.sufix);

            if (!isAproved)
            {
                CancelReason r = new CancelReason(this);
                r.DataContext = this;
                r.ShowDialog();
            }

            if (responsePurshase.Success)
            {
                workLogModel.ActionWorkLog("Insert" + isAprobed, model.IdOrder.Trim(), userModel.Id, model.IdSupplier);
                GetPurshaseOrders(selected.EmpresaId, selected.db, selected.sufix);
                OrderId = model.IdOrder;
                email = model.SellerEmail;
                BackgroundWorker sendMail = new BackgroundWorker();
                sendMail.DoWork += sendMail_Do;
                lblMailStatus.Content = "Correo se esta enviando en segundo plano";
                sendMail.RunWorkerCompleted += sendMail_Completed;
                sendMail.RunWorkerAsync();
                MessageBox.Show("Orden procesada exitosamente", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(responsePurshase.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SendListPurshaseOrderConfirmation(List<PurshaseOrderModel> model)
        {
            var selected = (RolEmpresaModel)cbRolEmpresas.SelectedItem;
            int i = 0;
            int j = 0;
            foreach (var item in model.Where(x => x.Selected))
            {
                var responsePurshase = _purshaseOrderModel.ActionPurchaseOrder(item, "UpdateOrder" + isAprobed, selected.EmpresaId, selected.db, selected.sufix);
                if (responsePurshase.Success)
                {
                    workLogModel.ActionWorkLog("Insert" + isAprobed, item.IdOrder.Trim(), userModel.Id, item.IdSupplier);
                    lstOrderId.Add(item.IdOrder);
                    i++;
                }
                else
                {
                    j++;
                }
            }
            if (j > 0)
            {
                GetPurshaseOrders(selected.EmpresaId, selected.db, selected.sufix);
                MessageBox.Show("Error al aprobar compras, para mayor informacion consulte la bitacora ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (i > 0)
                {
                    GetPurshaseOrders(selected.EmpresaId, selected.db, selected.sufix);
                    BackgroundWorker sendBulkMail = new BackgroundWorker();
                    sendBulkMail.DoWork += sendBulkMail_Do;
                    lblMailStatus.Content = "Correos se estan enviando en segundo plano";
                    sendBulkMail.RunWorkerCompleted += sendBulkMail_Completed;
                    sendBulkMail.RunWorkerAsync();
                    MessageBox.Show("Ordenes procesadas exitosamente", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var selected = (PurshaseOrderModel)button.DataContext;
            isAprobed = "Aprobada";
            Empresa = selected.Empresa;
            SendPurshaseOrderConfirmation(selected);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var selected = (PurshaseOrderModel)button.DataContext;
            isAprobed = "Rechazada";
            Empresa = selected.Empresa;
            SendPurshaseOrderConfirmation(selected, false);
        }
        private void chkTodas_Checked(object sender, RoutedEventArgs e)
        {
            List<PurshaseOrderModel> AuxlstPurchaseOrders = new List<PurshaseOrderModel>();
            foreach (var item in _lstPurchaseOrders)
            {
                item.Selected = true;
                AuxlstPurchaseOrders.Add(item);
            }
            _lstPurchaseOrders = AuxlstPurchaseOrders;
            dataGrid.ItemsSource = AuxlstPurchaseOrders;
        }

        private void chkTodas_Unchecked(object sender, RoutedEventArgs e)
        {
            List<PurshaseOrderModel> AuxlstPurchaseOrders = new List<PurshaseOrderModel>();
            foreach (var item in _lstPurchaseOrders)
            {
                item.Selected = false;
                AuxlstPurchaseOrders.Add(item);
            }
            _lstPurchaseOrders = AuxlstPurchaseOrders;
            dataGrid.ItemsSource = AuxlstPurchaseOrders;
        }

        private void btnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            var ordenes = dataGrid.ItemsSource as List<PurshaseOrderModel>;
            isAprobed = "Aprobada";
            Empresa = ordenes.FirstOrDefault().Empresa;
            if (ordenes.Count() > 0)
                SendListPurshaseOrderConfirmation(ordenes);
        }

        private void btnRechazar_Click(object sender, RoutedEventArgs e)
        {
            var ordenes = dataGrid.ItemsSource as List<PurshaseOrderModel>;
            isAprobed = "Rechazada";
            Empresa = ordenes.FirstOrDefault().Empresa;
            if (ordenes.Count() > 0)
                SendListPurshaseOrderConfirmation(ordenes);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var selectedEmpresa = (RolEmpresaModel)cbRolEmpresas.SelectedItem;
            var button = (Button)sender;
            var selected = (PurshaseOrderModel)button.DataContext;
            OrdenDetalle ordenDetalle = new OrdenDetalle(selected, selectedEmpresa.db, selectedEmpresa.sufix);
            ordenDetalle.ShowDialog();

        }

        private void cbRolEmpresas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (RolEmpresaModel)cbRolEmpresas.SelectedItem;
            GetPurshaseOrders(selected.EmpresaId, selected.db, selected.sufix);
        }
    }
}
