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
    /// Interaction logic for Roles.xaml
    /// </summary>
    public partial class Roles : Window
    {
        RolModel _rolModel = new RolModel();
        string _Id = string.Empty;
        public Roles(UserModel user)
        {
            InitializeComponent();
            LoadProperties(false);
            GetRoles();
        }
        private void GetRoles()
        {
            var response = _rolModel.GetRol();
            if (response.Success)
            {
                dgRoles.ItemsSource = response.Object;
            }
            else
            {
                MessageBox.Show("Problemas al cargar roles " + response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void chkEstado_Checked(object sender, RoutedEventArgs e)
        {
            if (chkEstado.IsChecked == true)
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
            if (chkEstado.IsChecked == true)
            {
                chkEstado.Content = "ACTIVO";
            }
            else
            {
                chkEstado.Content = "INACTIVO";
            }
        }

        private void txtNombre_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text))
            {

            }
        }

        private void txtNombre_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Prohibit space
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        void LoadProperties(bool IsEnable)
        {
            _Id = string.Empty;
            txtNombre.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            chkEsAdministrador.IsChecked = false;
            chkEstado.IsChecked = false;
            if (IsEnable)
            {
                txtNombre.IsEnabled = true;
                txtDescripcion.IsEnabled = true;
                chkEsAdministrador.IsEnabled = true;
                chkEstado.IsEnabled = true;
                btnGuardar.IsEnabled = true;
                gprbx.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
            }
            else
            {
                txtNombre.IsEnabled = false;
                txtDescripcion.IsEnabled = false;
                chkEsAdministrador.IsEnabled = false;
                chkEstado.IsEnabled = false;
                btnGuardar.IsEnabled = false;
                gprbx.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDEDEDE"));

            }

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            LoadProperties(true);
            var select = (RolModel)dgRoles.SelectedItem;
            _Id = select.Id;
            txtNombre.Text = select.Nombre;
            txtDescripcion.Text = select.Descripcion;
            chkEsAdministrador.IsChecked = select.EsAdministrador;
            chkEstado.IsChecked = select.Estado;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtDescripcion.Text) || String.IsNullOrEmpty(txtNombre.Text))
            {
                MessageBox.Show("Debe ingresar los datos obligatorios", "ERROR ROLES", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else
            {
                var action = _Id == string.Empty ? "Insert" : "Update";
                RolModel model = new RolModel();
                model.Id = _Id;
                model.Nombre = txtNombre.Text;
                model.Descripcion = txtDescripcion.Text;
                model.EsAdministrador = chkEsAdministrador.IsChecked.Value;
                model.Estado = chkEstado.IsChecked.Value;
                var response = _rolModel.ActionRole(model, action);
                if (response.Success)
                {
                    MessageBox.Show("Rol guardada correctamente");
                    GetRoles();
                    LoadProperties(false);
                }
                else
                {
                    MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            LoadProperties(true);
        }

        private void chkEsAdministrador_Checked(object sender, RoutedEventArgs e)
        {
            if (chkEsAdministrador.IsChecked.Value)
            {
                chkEsAdministrador.Content = "SI";
            }
            else
            {
                chkEsAdministrador.Content = "NO";
            }
        }

        private void chkEsAdministrador_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkEsAdministrador.IsChecked.Value)
            {
                chkEsAdministrador.Content = "SI";
            }
            else
            {
                chkEsAdministrador.Content = "NO";
            }
        }

        private void btnAsignar_Click(object sender, RoutedEventArgs e)
        {
            var select = (RolModel)dgRoles.SelectedItem;
            RolEmpresa rolEmpresa = new RolEmpresa(select.Id);
            rolEmpresa.ShowDialog();
        }
    }
}
