using ORD_SAE.MODEL;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Empresas.xaml
    /// </summary>
    public partial class Empresas : Window
    {
        EmpresaModel _empresaModel = new EmpresaModel();
        string _Id = string.Empty;
        public Empresas(UserModel user)
        {
            InitializeComponent();
            LoadProperties(false);
            GetEmpresas();
        }
        private void GetEmpresas()
        {
            var response = _empresaModel.GetEmpresas();
            if (response.Success)
            {
                dgEmpresas.ItemsSource = response.Object;
            }
            else
            {
                MessageBox.Show("Problemas al cargar empresas " + response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void chkEstado_Checked(object sender, RoutedEventArgs e)
        {
            if (chkEstado.IsChecked.Value == true)
            {
                chkEstado.Content = "ACTIVO";
            }
            else
            {
                chkEstado.Content = "INACTIVO";
            }
        }

        private void chkEstado_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkEstado.IsChecked.Value == true)
            {
                chkEstado.Content = "ACTIVO";
            }
            else
            {
                chkEstado.Content = "INACTIVO";
            }
        }
        void LoadProperties(bool IsEnable)
        {
            _Id = string.Empty;
            txtNombreEmpresa.Text = string.Empty;
            txtDireccionEmpresa.Text = string.Empty;
            txtEmpreSAE.Text = string.Empty;
            txtSufijo.Text = string.Empty;
            chkEstado.Content = string.Empty;
            chkEstado.IsChecked = false;
            if (IsEnable)
            {
                txtNombreEmpresa.IsEnabled = true;
                txtDireccionEmpresa.IsEnabled = true;
                txtEmpreSAE.IsEnabled = true;
                txtSufijo.IsEnabled = true;
                chkEstado.IsEnabled = true;
                btnGuardar.IsEnabled = true;
                gprBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            }
            else
            {
                txtNombreEmpresa.IsEnabled = false;
                txtDireccionEmpresa.IsEnabled = false;
                txtEmpreSAE.IsEnabled = false;
                txtSufijo.IsEnabled = false;
                chkEstado.IsEnabled = false;
                btnGuardar.IsEnabled = false;
                gprBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEDEDE"));
            }
        }

        private void btnNueva_Click(object sender, RoutedEventArgs e)
        {
            LoadProperties(true);
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtNombreEmpresa.Text) || String.IsNullOrEmpty(txtDireccionEmpresa.Text) || String.IsNullOrEmpty(txtEmpreSAE.Text) || String.IsNullOrEmpty(txtSufijo.Text))
            {
                MessageBox.Show("Debe ingresar los datos obligatorios", "ERROR ROLES", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                EmpresaModel model = new EmpresaModel();
                var action = _Id == string.Empty ? "Insert" : "Update";
                model.Id = _Id;
                model.Nombre = txtNombreEmpresa.Text;
                model.Direccion = txtDireccionEmpresa.Text;
                model.DB_SAE = txtEmpreSAE.Text;
                model.Sufijo = txtSufijo.Text;
                model.Estado = chkEstado.IsChecked.Value;
                var response = _empresaModel.ActionEmpresa(model, action);
                if (response.Success)
                {
                    MessageBox.Show("Empresa guardada correctamente");
                    GetEmpresas();
                    LoadProperties(false);
                }
                else
                {
                    MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            LoadProperties(true);
            var select = (EmpresaModel)dgEmpresas.SelectedItem;
            _Id = select.Id;
            txtNombreEmpresa.Text = select.Nombre;
            txtDireccionEmpresa.Text = select.Direccion;
            txtEmpreSAE.Text = select.DB_SAE;
            txtSufijo.Text = select.Sufijo;
            chkEstado.IsChecked = select.Estado;
            if (select.Estado)
            {
                chkEstado.Content = "ACTIVO";
            }
            else
            {
                chkEstado.Content = "INACTIVO";
            }
        }

        private void bntCancelar_Click(object sender, RoutedEventArgs e)
        {
            LoadProperties(false);
        }
    }
}

