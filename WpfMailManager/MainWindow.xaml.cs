using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfMailManager;
using wpfTask1;

namespace WpfMailManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private EmailUser userLogged;
        private ObservableCollection<EmailMessage> allMessagesReceived;
        private ObservableCollection<EmailMessage> allMessagesSent;
        private ObservableCollection<EmailMessage> messagesReceived;
        private ObservableCollection<EmailMessage> messagesSent;
        private EmailMessage selectedReceivedMessage;
        private EmailMessage selectedSentMessage;
        private EmailMessage selectedMessage;
        private int tabIndex;

        public ObservableCollection<EmailMessage> MessagesReceived
        {
            get { return messagesReceived; }
            set
            {
                messagesReceived = value;
                OnPropertyChanged("MessagesReceived");
            }
        }
        public ObservableCollection<EmailMessage> MessagesSent
        {
            get { return messagesSent; }
            set
            {
                messagesSent = value;
                OnPropertyChanged("MessagesSent");
            }
        }
        public EmailMessage SelectedMessage
        {
            get { return selectedMessage; }
            set
            {
                selectedMessage = value;
                OnPropertyChanged("SelectedMessage");
            }
        }
        public EmailUser User
        {
            get { return userLogged; }
            set
            {
                userLogged = value;
                if(value != null)
                {
                    allMessagesReceived = User.MessagesReceived;
                    allMessagesSent = User.MessagesSent;
                    MessagesReceived = User.MessagesReceived;
                    MessagesSent = User.MessagesSent;
                    if (MessagesSent == null)
                        MessagesSent = new ObservableCollection<EmailMessage>();
                }
                OnPropertyChanged("User");
            }
        }
        public int TabRecvSentIndex
        {
            get { return tabIndex; }
            set
            {
                tabIndex = value;
                switch(tabIndex)
                {
                    case 0:
                        {
                            SelectedMessage = selectedReceivedMessage;
                            break;
                        }
                    case 1:
                        {
                            SelectedMessage = selectedSentMessage;
                            break;
                        }
                }
                OnPropertyChanged("TabRecvSentIndex");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
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

        #region Handlers

        private void btnLoginClick(object sender, RoutedEventArgs e)
        {
            if(User==null)
            {
                LoginWindow wnd = new LoginWindow();
                wnd.Owner = this;
                Opacity = 0.5;
                wnd.ShowDialog();
                if (wnd.User != null)
                {
                    User = wnd.User;
                }
                Opacity = 1;
            }
            else
            {
                User = null;
                MessagesReceived = null;
                MessagesSent = null;
                SelectedMessage = null;
                searchBox.Text = "";
            }
        }

        private void btnNewEmailClick(object sender, RoutedEventArgs e)
        {
            NewEmailWindow wnd = new NewEmailWindow();
            wnd.Owner = this;
            wnd.Width = 0.8 * this.ActualWidth;
            wnd.Height = 0.8 * this.ActualHeight;
            this.Opacity = 0.5;
            wnd.ShowDialog();
            MessagesSent.Add(wnd.Email);
            this.Opacity = 1;
        }

        private void selectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count>0)
                SelectedMessage = (EmailMessage)e.AddedItems[0];

            switch (tabIndex)
            {
                case 0:
                    {
                        selectedReceivedMessage = SelectedMessage;
                        break;
                    }
                case 1:
                    {
                        selectedSentMessage = SelectedMessage;
                        break;
                    }
            }
        }

        private void tabControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && SelectedMessage != null)
            {
                if (TabRecvSentIndex == 0)
                    MessagesReceived.Remove(SelectedMessage);
                else
                    MessagesSent.Remove(SelectedMessage);
            }
        }

        private void searchBoxChanged(object sender, RoutedEventArgs e)
        {
            if(searchBox.Text.Length==0)
            {
                MessagesReceived = allMessagesReceived;
                MessagesSent = allMessagesSent;
                return;
            }
            string[] words = searchBox.Text.Split(' ');
            ObservableCollection<EmailMessage> listR = new ObservableCollection<EmailMessage>();
            ObservableCollection<EmailMessage> listS = new ObservableCollection<EmailMessage>();

            foreach (string word in words)
                if(word.Length>0)
                {
                    foreach (EmailMessage msg in allMessagesReceived)
                        if ((msg.Title.Contains(word) || msg.Date.Contains(word) || msg.From.Contains(word)) && !listR.Contains(msg))
                            listR.Add(msg);
                    foreach (EmailMessage msg in allMessagesSent)
                        if ((msg.Title.Contains(word) || msg.Date.Contains(word) || msg.To.Contains(word)) && !listS.Contains(msg))
                            listS.Add(msg);
                }
            MessagesReceived = listR;
            MessagesSent = listS;
        }

        private void polishLangChecked(object sender, RoutedEventArgs e) => changeLanguage("pl-PL");

        private void englishLangChecked(object sender, RoutedEventArgs e) => changeLanguage("en-GB");

        #endregion

        private void changeLanguage(string lang)
        {
            ResourceDictionary dictionary;
            Uri source = new Uri(string.Format("/WpfMailManager;component/Resources/Strings.{0}.xaml", lang), UriKind.Relative);
            try
            {
                dictionary = (ResourceDictionary)Application.LoadComponent(source);
            }
            catch (IOException)
            {
                return;
            }
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }

    }

    #region Converters

    public class NullToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as EmailUser) != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FromOrToConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            EmailMessage msg = (EmailMessage)values[0];
            if (msg == null)
                return "";
            int index = (int)values[1];
            if (index == 0)
                return "Email: " + msg.From;
            else
                return "Email: " + msg.To;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
