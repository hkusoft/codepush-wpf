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
            Properties.Settings.Default.AppToken = TokenTextBox.Text;
            this.DialogResult = true;

            this.Close();
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
    }
}
