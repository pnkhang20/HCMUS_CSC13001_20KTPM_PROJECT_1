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
using System.Net;
using System.Configuration;
using System.Security.Cryptography;

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
            httpClient.BaseAddress = new Uri("https://localhost:7122/api/Users"); // Replace with your API base address
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
                    // Save the username and password if the user checked the "Remember Me" checkbox
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        if (RememberMeCheckBox.IsChecked == true)
                        {
                            // Store the login information in the credential manager
                            // var cm = new { Target = "MyApp", UserName = loginRequest.usr, Password = loginRequest.pwd };

                            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(
                      ConfigurationUserLevel.None);
                            config.AppSettings.Settings["Username"].Value = EmailTextBox.Text;

                            // Ma hoa mat khau
                            var passwordInBytes = Encoding.UTF8.GetBytes(PasswordBox.Password);
                            var entropy = new byte[20];
                            using (var rng = RandomNumberGenerator.Create())
                            {
                                rng.GetBytes(entropy);
                            }

                            var cypherText = ProtectedData.Protect(
                                passwordInBytes,
                                entropy,
                                DataProtectionScope.CurrentUser
                            );

                            var passwordIn64 = Convert.ToBase64String(cypherText);
                            var entropyIn64 = Convert.ToBase64String(entropy);
                            config.AppSettings.Settings["Password"].Value = passwordIn64;
                            config.AppSettings.Settings["Entropy"].Value = entropyIn64;

                            config.Save(ConfigurationSaveMode.Full);
                            System.Configuration.ConfigurationManager.RefreshSection("appSettings");



                        }
                        //var credentialCache = new CredentialCache();
                        //credentialCache.Add(new Uri(httpClient.BaseAddress.ToString()), "Basic", new NetworkCredential(loginRequest.usr, loginRequest.pwd));
                    }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string username = System.Configuration.ConfigurationManager.AppSettings["Username"]!;
            string passwordIn64 = System.Configuration.ConfigurationManager.AppSettings["Password"]!;
            string entropyIn64 = System.Configuration.ConfigurationManager.AppSettings["Entropy"]!;

            if (passwordIn64.Length != 0)
            {
                byte[] entropyInBytes = Convert.FromBase64String(entropyIn64);
                byte[] cypherTextInBytes = Convert.FromBase64String(passwordIn64);

                byte[] passwordInBytes = ProtectedData.Unprotect(cypherTextInBytes,
                    entropyInBytes,
                    DataProtectionScope.CurrentUser
                );

                string password = Encoding.UTF8.GetString(passwordInBytes);

                EmailTextBox.Text = username;
                PasswordBox.Password = password;
            }



        }
    }
}

