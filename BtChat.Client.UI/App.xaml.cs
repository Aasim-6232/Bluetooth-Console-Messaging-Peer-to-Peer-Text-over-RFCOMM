using System.Windows;

namespace BtChat.Client.UI
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Show login window
            var login = new LoginWindow();
            bool? result = login.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(login.UserName))
            {
                // Show main chat window
                var main = new MainWindow(login.UserName);
                MainWindow = main; // Set MainWindow so app stays alive
                main.Show();
            }
            else
            {
                // Explicitly shutdown if login fails
                Shutdown();
            }
        }
    }
}
