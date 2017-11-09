using System;
using System.Collections.Generic;
using System.Linq;
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

namespace codepush_wpf
{
    /// <summary>
    /// Interaction logic for UserInfo.xamlB
    /// </summary>
    public partial class UserInfo : Window
    {
        public string Answer {
            get { return TokenTextBox.Text; }
            set
            {
                Properties.Settings.Default.AppToken = TokenTextBox.Text = value;
            }
        }
        public UserInfo()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (RememberMeCheck.IsChecked == true && !string.IsNullOrEmpty(TokenTextBox.Text))
            {
                Properties.Settings.Default.AppToken = TokenTextBox.Text;
                Properties.Settings.Default.Save();
            }
            DialogResult = true;

            Close();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            TokenTextBox.Text = Clipboard.GetText();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;

            this.Close();
        }

        private void OnCheckChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RememberMe = RememberMeCheck.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RememberMeCheck.IsChecked = Properties.Settings.Default.RememberMe;
        }
    }
}
