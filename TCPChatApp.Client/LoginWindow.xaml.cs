using System;
using System.Windows;

namespace TCPChatApp.Client
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            // TODO: Send registration request to the server
            //       Validate response, show success/failure message
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            // TODO: Send login request to the server
            //       If success: open main chat window
            //       If fail: show error message
            MainWindow chatWindow = new MainWindow();
            chatWindow.Show();

            // Close the login window
            this.Close();
        }
    }
}
