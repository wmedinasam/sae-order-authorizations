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
    /// Interaction logic for Usuarios.xaml
    /// </summary>
    public partial class Usuarios : Window
    {
        public Usuarios(UserModel user)
        {
            InitializeComponent();
        }
        RolModel _rolModel = new RolModel();
        UserModel _userModel = new UserModel();
        List<UserModel> _lstUser = new List<UserModel>();
        string _id = string.Empty;
        private void GetRol()
        {
           var response =  _rolModel.GetRol();
            if (response.Success)
            {
                cmbRol.ItemsSource = response.Object;
                cmbRol.SelectedValuePath = "Id";
                cmbRol.DisplayMemberPath = "Nombre";
            }
            else
            {
                MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //List<RolModel> rolModel = new List<RolModel>() 
            //{ new RolModel { Id = "1", Description = "Admin" },
            //new RolModel { Id = "1", Description = "User" }};
            //cmbRol.ItemsSource = rolModel;
            //cmbRol.SelectedValuePath = "Id";
            //cmbRol.DisplayMemberPath = "Description";

        }
        private void GetUser()
        {
            var response = _userModel.GetUser();
            if (response.Success)
            {
                _lstUser = response.Object;
                dataGrid.ItemsSource = _lstUser;
            }
            else
            {
                MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private bool ValidaPassword()
        {
            bool result = false;
           
            //Que lleve un Simbolo
            if (pwdPassword.Password.Contains("@") || pwdPassword.Password.Contains("#") ||
                pwdPassword.Password.Contains("$") || pwdPassword.Password.Contains("%") ||
                pwdPassword.Password.Contains("&") || pwdPassword.Password.Contains("+") || pwdPassword.Password.Contains("*"))
            {
                result = false;
            }
            else
            {
                result = true;
            }
            //Que lleve Numeros
            if (pwdPassword.Password.Contains("1") || pwdPassword.Password.Contains("2") ||
                pwdPassword.Password.Contains("3") || pwdPassword.Password.Contains("4") ||
                pwdPassword.Password.Contains("5") || pwdPassword.Password.Contains("6") ||
                pwdPassword.Password.Contains("7") || pwdPassword.Password.Contains("8") ||
                pwdPassword.Password.Contains("9") || pwdPassword.Password.Contains("0"))
            {
                result = false;
            }
            else
            {
                result = true;
            }
            //Logitud de Contraseña
            if (pwdPassword.Password.Length < 8)
            {
                result = true;
            }
            return result;
        }

        private void EditUser(UserModel userModel)
        {
            dataGrid.IsEnabled = true;
            btn_Add.IsEnabled = false;
            grdNuevo.IsEnabled = true;
            _id = userModel.Id;
            txtNombre.Text = userModel.FullName;
            txtUsuario.Text = userModel.Nick;
            pwdPassword.Password = userModel.Password.DesencriptarDatos();
            pwdPasswordConfirm.Password= userModel.Password.DesencriptarDatos();
            txtCorreo.Text = userModel.Mail;
            cmbRol.SelectedValue = userModel.RolId;
            if (userModel.SendMail)
            {
                rbtnSi.IsChecked = true;
            }
            else
            {
                rbtnNo.IsChecked = true;
            }
        }
        private void ClearScreen()
        {
            _id = string.Empty;
            txtNombre.Text = string.Empty;
            txtUsuario.Text = string.Empty;
            pwdPassword.Password = string.Empty;
            pwdPasswordConfirm.Password = string.Empty;
            txtCorreo.Text = string.Empty;
            cmbRol.SelectedIndex = -1;
            rbtnSi.IsChecked = false;
            rbtnNo.IsChecked = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetRol();
            GetUser();
            grdNuevo.IsEnabled = false;
            dataGrid.IsEnabled = true;
            btn_Add.IsEnabled = true;
        }
        private void PwdPasswordConfirm_LostFocus(object sender, RoutedEventArgs e)
        {
            if (pwdPasswordConfirm.Password == String.Empty)
            {

            }
            else
            {
                if (pwdPassword.Password != pwdPasswordConfirm.Password)
                {
                    MessageBox.Show("La contraseña no coincide", "ERROR PASSWORD", MessageBoxButton.OK, MessageBoxImage.Error);
                    pwdPasswordConfirm.Clear();
                }
            }

        }

        private void PwdPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (pwdPassword.Password == String.Empty)
            {

            }
            else
            {
                if (ValidaPassword())
                {
                    MessageBox.Show("La contraseña no cumple la complejidad.\nCaracteristicas:\n1. Mayor o Igual a 8 Caracteres.\n2. Contener un Simbolo (@#$%&*+).\n3. Contener Numeros (1, 2, 3...)", "ERROR CONTRASEÑA", MessageBoxButton.OK, MessageBoxImage.Error);
                    pwdPassword.Clear();
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var select = (UserModel)dataGrid.SelectedItem;
            EditUser(select);
        }

        private void BtnGuardarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarNull())
            {
                var action = _id == string.Empty ? "Insert" : "Update";
                UserModel userModel = new UserModel();
                userModel.Id = _id;
                userModel.FullName = txtNombre.Text;
                userModel.Nick = txtUsuario.Text;
                userModel.Password = pwdPassword.Password.EncriptarDatos();
                userModel.Mail = txtCorreo.Text;
                userModel.RolId = cmbRol.SelectedValue.ToString();
                userModel.SendMail = rbtnSi.IsChecked.Value;
                var response = _userModel.ActionUser(userModel, action);
                if (response.Success)
                {
                    GetUser();
                    ClearScreen();
                    MessageBox.Show("Usuario Guardado Correctamente!", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
                    Window_Loaded(null, null);
                }
                else
                {
                    MessageBox.Show(response.Message, "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Existen campos obligatorios, por favor ingrese los datos!", "ERROR DE SISTEMA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }

        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            ClearScreen();

            grdNuevo.IsEnabled = true;
            dataGrid.IsEnabled = false;
        }

        private bool ValidarNull()
        {
            bool result = true;
            if (String.IsNullOrEmpty(txtNombre.Text))
            {
                txtNombre.BorderBrush = Brushes.Red;
                result = true;
            }
            else
            //{
            //    txtNombre.BorderBrush = Brushes.Black;
            //    result = false;
            //}
            if (String.IsNullOrEmpty(txtUsuario.Text))
            {
                txtUsuario.BorderBrush = Brushes.Red;
                result = true;
            }
            else
            //{
            //     txtUsuario.BorderBrush = Brushes.Black;
            //    result = false;
            //}
            if (String.IsNullOrEmpty(txtCorreo.Text))
            {
                txtCorreo.BorderBrush = Brushes.Red;
                result = true;
            }
            else
            //{
            //    txtCorreo.BorderBrush = Brushes.Black;
            //    result = false;
            //}
            if (String.IsNullOrEmpty(pwdPassword.Password))
            {
                pwdPassword.BorderBrush = Brushes.Red;
                result = true;
            }
            else
            //{
            //    pwdPassword.BorderBrush = Brushes.Black;
            //    result = false;
            //}
            if (cmbRol.SelectedIndex == -1 || String.IsNullOrEmpty(cmbRol.Text))
            {
                cmbRol.BorderBrush = Brushes.Red;
                result = true;
            }
            else
            //{
            //    cmbRol.BorderBrush = Brushes.Black;
            //    result = false;
            //}
            if (rbtnSi.IsChecked == false && rbtnNo.IsChecked == false)
            {
                rbtnSi.Foreground = Brushes.Red;
                rbtnNo.Foreground = Brushes.Red;
                result = true;
            }
            else
            {
                rbtnSi.Foreground = Brushes.Black;
                rbtnNo.Foreground = Brushes.Black;
                cmbRol.BorderBrush = Brushes.Black;
                txtNombre.BorderBrush = Brushes.Black;
                pwdPassword.BorderBrush = Brushes.Black;
                txtCorreo.BorderBrush = Brushes.Black;
                txtUsuario.BorderBrush = Brushes.Black;
                result = false;
            }
            return result; 
        }
    }
}
