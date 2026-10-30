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
    /// Interaction logic for CancelReason.xaml
    /// </summary>
    public partial class CancelReason : Window
    {
        Autorizaciones autorizaciones;
        string reason = string.Empty;

        public CancelReason(Autorizaciones auto)
        {
            InitializeComponent();
            this.autorizaciones = auto;
        }

        private void btnGrabarMotivo_Click(object sender, RoutedEventArgs e)
        {
            reason = txtMotivo.Text;
            autorizaciones._motivo = reason;
            this.Close();
        }
    }
}
