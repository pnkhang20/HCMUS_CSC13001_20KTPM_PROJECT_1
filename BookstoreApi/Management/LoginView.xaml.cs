using Amazon.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
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
using Newtonsoft.Json;
using Management.Models;

namespace Management.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageTextBlock.Text = string.Empty;

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://example.com/"); // Replace with your API base address
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var loginRequest = new { usr = EmailTextBox.Text, pwd = PasswordBox.Password };
            var response = await httpClient.GetAsync("https://localhost:7122/api/Users");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(responseContent);
                var matchingUser = users.FirstOrDefault(user => user.UserName == loginRequest.usr);

                if (matchingUser != null && BCrypt.Net.BCrypt.Verify(loginRequest.pwd, matchingUser.Password))
                {
                    MessageBox.Show("Successfully Logged In!");
                    // The password is correct, so login was successful
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    // Display an error message to the user
                    MessageBox.Show("Invalid email or password", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Display an error message to the user
                MessageBox.Show($"Failed to login {response.ReasonPhrase}");
            }
        }
    }
}

