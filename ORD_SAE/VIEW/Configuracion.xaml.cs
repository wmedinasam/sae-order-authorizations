using Newtonsoft.Json;
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
    /// Interaction logic for Configuracion.xaml
    /// </summary>
    public partial class Configuracion : Window
    {
        public Configuracion(UserModel user)
        {
            InitializeComponent();
            if (user.isLogin == null)
            {
                GetConfigurations();
                GetSystemConfigurations();
            }
        }
        ConfigurationModel _configurationModel = new ConfigurationModel();
        SystemConfugurationModel _systemConfugurationModel = new SystemConfugurationModel();
        private void GetConfigurations()
        {
            var response = _configurationModel.GetConfiguration();
            if (response.Success)
            {
                if (response.Object != null)
                {
                    var result = response.Object;
                    txtUsuario.Text = result.User;
                    pwdContraseña.Password = result.Password;
                    txtLlave.Text = JsonConvert.SerializeObject(result.Key).EncriptarDatos();
                    txtBaseDatos.Text = result.BdSae;
                }
            }
            else
            {

                MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void GetSystemConfigurations()
        {
            var response = _systemConfugurationModel.GetConfiguration();
            if (response.Success)
            {
                if (response.Object != null)
                {
                    var result = response.Object;
                    txtCorreoEmisor.Text = result.Where(x => x.Key == "MailFrom").FirstOrDefault().Value;
                    pwdCorreoEmisor.Password = result.Where(x => x.Key == "MailPassword").FirstOrDefault().Value;
                    txtAsunto.Text = result.Where(x => x.Key == "MailSubject").FirstOrDefault().Value.DesencriptarDatos();
                    txtCuerpo.Text = result.Where(x => x.Key == "MailBody").FirstOrDefault().Value.DesencriptarDatos();
                }
            }
            else
            {

                MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void BtnGuardarSAE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigurationModel configurationModel = new ConfigurationModel();
                configurationModel.User = txtUsuario.Text;
                configurationModel.Password = pwdContraseña.Password;
                configurationModel.BdSae = txtBaseDatos.Text;
                string key = txtLlave.Text.DesencriptarDatos();
                configurationModel.Key = JsonConvert.DeserializeObject<KeyModel>(key);
                if (configurationModel.Key.Sistema == "Autorizaciones")
                {
                    var response = _configurationModel.SaveConfiguration(configurationModel);
                    if (response.Success)
                    {
                        MessageBox.Show("Configuraciones Guardadas Correctamente", "CONFIGURACIONES", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Error en el token", "Estas haciendo algo mal", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                SystemConfugurationModel mailFrom = new SystemConfugurationModel();
                SystemConfugurationModel mailPassword = new SystemConfugurationModel();
                SystemConfugurationModel mailSubject = new SystemConfugurationModel();
                SystemConfugurationModel mailBody = new SystemConfugurationModel();

                mailFrom.Key = "MailFrom";
                mailFrom.Value = txtCorreoEmisor.Text;
                var mailFromExists = _systemConfugurationModel.GetConfiguration().Object.Any(x => x.Key == "MailFrom") ? "Update" : "Insert";
                var responseMailFrom = _systemConfugurationModel.ActionConfiguration(mailFrom, mailFromExists);

                mailPassword.Key = "MailPassword";
                mailPassword.Value = pwdCorreoEmisor.Password;
                var mailPasswordExists = _systemConfugurationModel.GetConfiguration().Object.Any(x => x.Key == "MailPassword") ? "Update" : "Insert";
                _systemConfugurationModel.ActionConfiguration(mailPassword, mailPasswordExists);

                mailSubject.Key = "MailSubject";
                mailSubject.Value = txtAsunto.Text.EncriptarDatos();
                var mailSubjectExists = _systemConfugurationModel.GetConfiguration().Object.Any(x => x.Key == "MailSubject") ? "Update" : "Insert";
                _systemConfugurationModel.ActionConfiguration(mailSubject, mailSubjectExists);

                mailBody.Key = "MailBody";
                mailBody.Value = txtCuerpo.Text.EncriptarDatos();
                var mailBodyExists = _systemConfugurationModel.GetConfiguration().Object.Any(x => x.Key == "MailBody") ? "Update" : "Insert";
                _systemConfugurationModel.ActionConfiguration(mailBody, mailBodyExists);
            }
            catch (Exception ex )
            {
                MessageBox.Show(ex.Message, "ERROR DE TOKEN", MessageBoxButton.OK, MessageBoxImage.Error); 
            }

        }
    }
}
