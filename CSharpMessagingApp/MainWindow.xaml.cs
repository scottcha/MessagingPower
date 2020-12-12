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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Odbc;
using System.Diagnostics.Tracing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace CSharpMessagingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string CONNECTION_STRING = @"Driver={ODBC Driver 17 for SQL Server};SERVER=localhost\SQLEXPRESS;Database=MessagingDb;Trusted_Connection=yes;";
        private int LastMsgId = 0;
        private string messages;
        public string Messages {
            get
            {
                return messages;
            }
            set
            {
                messages = value;
                RaisePropertyChanged("Messages");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadAllMessages();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(5000);
                    LoadNewMessages();
                }
            });
        }

        private void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadAllMessages()
        {
            messages = null;
            string queryString = @"SELECT * FROM dbo.ServerMessages";

            OdbcCommand command = new OdbcCommand(queryString);

            using (OdbcConnection connection = new OdbcConnection(CONNECTION_STRING))
            {
                command.Connection = connection;
                connection.Open();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var data = reader["Message"].ToString();
                        var sender = reader["SenderId"].ToString();
                        LastMsgId = Int32.Parse(reader["Id"].ToString()); 
                        Messages += string.Format("{0}: {1}\n", sender, data);
                    }

                }
            }
        }
        private async void LoadNewMessages()
        {
            string queryString = string.Format("SELECT * FROM dbo.ServerMessages WHERE Id > {0}", LastMsgId);

            OdbcCommand command = new OdbcCommand(queryString);

            using (OdbcConnection connection = new OdbcConnection(CONNECTION_STRING))
            {
                command.Connection = connection;
                connection.Open();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var data = reader["Message"].ToString();
                        var sender = reader["SenderId"].ToString();
                        LastMsgId = Int32.Parse(reader["Id"].ToString()); 
                        Messages += string.Format("{0}: {1}\n", sender, data);
                    }

                }
            }
        }
        
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await InsertNewMessage();
            tbCompose.Text = null;
        }

        private async Task InsertNewMessage()
        {
            string queryString = string.Format("INSERT INTO dbo.ServerMessages (Message, SenderId, ReceiverId) VALUES('{0}','{1}','{2}')", tbCompose.Text, "CSharp", "All");
            OdbcCommand command = new OdbcCommand(queryString);

            using (OdbcConnection connection = new OdbcConnection(CONNECTION_STRING))
            {
                command.Connection = connection;
                connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        private async void Test()
        {
            var rand = new Random();
            var letters = "abcdefghijklmnopqrstuvwxyz";
            var randomStringBuilder = new StringBuilder();
            for(int i = 0; i < rand.Next(1,256); i++)
            {
                randomStringBuilder.Append((char)letters[rand.Next(letters.Length)]);
            }
            await this.Dispatcher.Invoke(async () =>
            {
                tbCompose.Text = randomStringBuilder.ToString();
                await InsertNewMessage();
            });
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        { 
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(6000);
                    Test();
                }
            });
        }
    }
}
