using System.Windows;

namespace TCPChatApp.Client
{
    public partial class MessageUserWindow : Window
    {
        public string MessageText { get; private set; }
        public string Recipient { get; private set; }

        public MessageUserWindow(string username)
        {
            InitializeComponent();
            Recipient = username;
            Title = $"Message {username}";
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            MessageText = MessageTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}