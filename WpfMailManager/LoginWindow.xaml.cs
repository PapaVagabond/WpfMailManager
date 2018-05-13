using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using wpfTask1;

namespace WpfMailManager
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, INotifyPropertyChanged
    {
        private bool btnOkEnabled;
        public EmailUser User { get; set; }
        public bool BtnOkEnabled
        {
            get { return btnOkEnabled; }
            set {
                btnOkEnabled = value;
                OnPropertyChanged("BtnOkEnabled");
            }
        }
        public LoginWindow()
        {
            InitializeComponent();
            User = null;
            btnOkEnabled = false;
            BtnOkEnabled = false;
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void btnOkClick(object sender, RoutedEventArgs e)
        {
            EmailUser loggedUser;
            bool success = EmailData.GetUserData(loginBox.Text, passwordBox.Password, out loggedUser);
            if (!success)
                MessageBox.Show((string)Application.Current.FindResource("incorrectLoginData"));
            else
            {
                this.User = loggedUser;
                this.Close();
            }
        }
        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void textBoxChanged(object sender, RoutedEventArgs e)
        {
            if (loginBox.Text.Length > 0 && passwordBox.Password.Length > 0)
                BtnOkEnabled = true;
            else
                BtnOkEnabled = false;
        }
    }
}
