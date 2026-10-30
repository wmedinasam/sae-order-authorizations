using ORD_SAE.MODEL;
using ORD_SAE.Service;
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
    /// Interaction logic for LoginAccess.xaml
    /// </summary>
    public partial class LoginAccess : Window
    {
        UserModel model = new UserModel();
        public LoginAccess()
        {
            InitializeComponent();
        }
        ConfigurationModel _configurationModel = new ConfigurationModel();
        HandleCfg _handleCfg = new HandleCfg();
        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            if (!(String.IsNullOrEmpty(txtUsuario.Text) || String.IsNullOrEmpty(pwPassword.Password)))
            {
                var response = model.GetUser();
                if (response.Success)
                {
                    if (response.Object != null)
                    {
                        var result = response.Object.Where(x => x.Nick == txtUsuario.Text && x.Password == pwPassword.Password.EncriptarDatos()).FirstOrDefault();
                        if(result!=null)
                        {
                            MenuPrincipal w = new MenuPrincipal(result);
                            w.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Usuario/contraseña incorrecto", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        }
                    }
                    else
                    {
                        MessageBox.Show("Catalogo Usuario Vacio", "ERROR USUARIO", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(response.Message, "ERROR USUARIO", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Debe ingresar su usuario y su contraseña", "ERROR USUARIO", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_handleCfg.CfgExists(_configurationModel.path))
            {
                model.isLogin = "Login";
                Configuracion w = new Configuracion(model);
;                w.ShowDialog();
                if (!_handleCfg.CfgExists(_configurationModel.path))
                {
                    App.Current.Shutdown();
                }
            }
        }
    }
}
