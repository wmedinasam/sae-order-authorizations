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
    /// Lógica de interacción para RolEmpresa.xaml
    /// </summary>
    public partial class RolEmpresa : Window
    {
        string _rolId = string.Empty;
        public RolEmpresa(string rolId)
        {
            InitializeComponent();
            GetRolEmpresa(rolId);
            _rolId = rolId;
        }
        private void GetRolEmpresa(string rolId)
        {
            RolEmpresaModel rolEmpresa = new RolEmpresaModel();
            var response = rolEmpresa.GetRolEmpresa(rolId);
            if (response.Success)
            {
                grdEmpresa.ItemsSource = response.Object;
            }
            else
            {
                MessageBox.Show("Problemas al cargar rol empresas " + response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var selected = sender as CheckBox;
            if (selected.Tag != null)
            {
                RolEmpresaModel rolEmpresa = new RolEmpresaModel();
                rolEmpresa.RolId = _rolId;
                rolEmpresa.EmpresaId = selected.Tag.ToString();
                var response = rolEmpresa.ActionRolEmpresa("Insert", rolEmpresa);
                if (response.Success)
                {
                    MessageBox.Show("Se asigno correctamente la empresa al Rol", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Algo salio mal " + response.Message, "Error del sistema", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var selected = sender as CheckBox;
            if (selected.Tag != null)
            {
                RolEmpresaModel rolEmpresa = new RolEmpresaModel();
                rolEmpresa.RolId = _rolId;
                rolEmpresa.EmpresaId = selected.Tag.ToString();
                var response = rolEmpresa.ActionRolEmpresa("Delete", rolEmpresa);
                if (response.Success)
                {
                    MessageBox.Show("Se elimino asignacion a la empresa correctamente", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Algo salio mal " + response.Message, "Error del sistema", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
