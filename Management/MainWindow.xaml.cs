using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Management
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void getBook(object sender, RoutedEventArgs e)
        {
            //string bookName = bookNameTextBox.Text;
            using (var client = new HttpClient())
            {
                var uri = new Uri("https://localhost:7122/api/Books");
                var result = await client.GetAsync(uri);
                var json = result.Content.ReadAsStringAsync().Result;

                MessageBox.Show(json);
            }            
        }
    }
}
