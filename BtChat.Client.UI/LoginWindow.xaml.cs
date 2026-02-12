using System.Windows;

namespace BtChat.Client.UI
{
    public partial class LoginWindow : Window
    {
        public string UserName { get; private set; } = string.Empty;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter your name.", "BtChat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UserName = UsernameTextBox.Text.Trim();
            DialogResult = true; // this closes the window and returns control to App.xaml.cs
        }
    }
}
