using ORD_SAE.MODEL;
using System;
using System.Windows;

namespace ORD_SAE.VIEW
{
    /// <summary>
    /// Interaction logic for Bitacora.xaml
    /// </summary>
    public partial class Bitacora : Window
    {
        public Bitacora(UserModel userModel)
        {
            InitializeComponent();
        }

        private void DpHasta_LostFocus(object sender, RoutedEventArgs e)
        {
            if (dpDesde.SelectedDate > dpHasta.SelectedDate)
            {
                MessageBox.Show("La fecha Desde no puede ser mayor a la fecha Hasta!", "ERROR FECHA", MessageBoxButton.OK, MessageBoxImage.Error);
                dpDesde.SelectedDate = null;
                dpHasta.SelectedDate = null;
            }
            else
            {

            }
        }

        private void Limpiar()
        {
            dpDesde.SelectedDate = null;
            dpHasta.SelectedDate = null;
            txtBuscar.Clear();
            txtBuscar.IsEnabled = false;

        }

        private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((dpDesde.SelectedDate != null && dpHasta != null) && (dpDesde.Text != String.Empty && dpHasta.Text != String.Empty))
                {
                    //Activa la Busqueda
                    txtBuscar.IsEnabled = true;
                    WorkLogModel w = new WorkLogModel();
                    //ACA TU CODIGO
                    if (String.IsNullOrEmpty(txtBuscar.Text))
                    {    
                        var data = w.getWorkLog(dpDesde.Text, dpHasta.Text);
                        dataGrid.ItemsSource = data.Object;

                    }
                    else // con busqueda en txt incluida.
                    {
                        var data = w.getWorkLog(dpDesde.Text, dpHasta.Text, txtBuscar.Text);
                        dataGrid.ItemsSource = data.Object;
                    }
                }
                else
                {
                    MessageBox.Show("Alguna de las fechas esta en blanco, debe ingresar ambas", "ERROR FECHAS", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error: " + ex.Message, "ERROR BITACORA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtBuscar.IsEnabled = false;
        }

        private void BtnVerTodo_Click(object sender, RoutedEventArgs e)
        {
            WorkLogModel workLog = new WorkLogModel();
            var response = workLog.getAllWorkLog();
            if (response.Success)
            {
                dataGrid.ItemsSource = response.Object;
            }
            else
            {
                MessageBox.Show("Error al Obtener Log: " + response.Message, "ERROR LOG", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
