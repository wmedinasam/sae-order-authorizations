using Newtonsoft.Json;
using ORD_SAE.MODEL;
using ORD_SAE.Service;
using ORD_SAE.SERVICE;
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
    /// Interaction logic for MenuPrincipal.xaml
    /// </summary>
    public partial class MenuPrincipal : Window
    {
        ConfigurationModel _configurationModel = new ConfigurationModel();
        UserModel user = new UserModel();
        public MenuPrincipal(UserModel user)
        {
            InitializeComponent();
            GetMenu(user.EsAdministrador);
            lblLogedUser.Content = "Usuario: " + user.FullName;
            GetConfigurations();
            this.user = user;
            lblVersion.Content = "Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
        }
        private void GetConfigurations()
        {
            var response = _configurationModel.GetConfiguration();
            if (response.Success)
            {
                if (response.Object != null)
                {
                    var result = response.Object;
                    lblRegistro.Content = "Nombre: "+result.Key.NombreCliente + ", Nit:" + result.Key.Nit;
                }
            }
            else
            {

                MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void GetMenu(bool rol)
        {
            if (rol)
            {
                List<MenuModel> lst = new List<MenuModel>()
                { new MenuModel{Display = "Autorizaciones",  Image = LoadImage("check.png"), Url = "ORD_SAE.VIEW.Autorizaciones"  },
                new MenuModel{Display = "Empresas",  Image = LoadImage("company.png"), Url = "ORD_SAE.VIEW.Empresas"  },
                new MenuModel{Display = "Roles",  Image = LoadImage("la-red.png"), Url = "ORD_SAE.VIEW.Roles"  },
                new MenuModel{Display = "Usuarios",  Image = LoadImage("jefe.png"), Url = "ORD_SAE.VIEW.Usuarios"  },
                new MenuModel{Display = "Bitacora",  Image = LoadImage("timing.png"), Url = "ORD_SAE.VIEW.Bitacora"  },
                new MenuModel{Display = "Configuraciones",  Image = LoadImage("configuraciones.png"), Url = "ORD_SAE.VIEW.Configuracion"  }};
                ListMenu.ItemsSource = lst;
            }
            else
            {
                List<MenuModel> lst = new List<MenuModel>()
                { new MenuModel{Display = "Autorizaciones",  Image = LoadImage("check-list.png"), Url = "ORD_SAE.VIEW.Autorizaciones"  }};
                ListMenu.ItemsSource = lst;
            }
        }
        private BitmapImage LoadImage(string filename)
        {
            return new BitmapImage(new Uri("pack://application:,,,/PICTURES/" + filename));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var url = sender as Button;
            string objectToInstantiate = url.Tag.ToString();
            var objectType = Type.GetType(objectToInstantiate);
            var instantiatedObject = Activator.CreateInstance(objectType, user) as Window;
            instantiatedObject.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
