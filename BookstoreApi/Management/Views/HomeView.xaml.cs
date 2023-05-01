using Management.ViewModels;
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

namespace Management.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            
        }
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                // Scroll down
                if (listProductsView.Items.Count > 0)
                {
                    listProductsView.ScrollIntoView(listProductsView.Items[listProductsView.Items.Count - 1]);
                }
            }
            else if (e.Delta > 0)
            {
                // Scroll up
                if (listProductsView.Items.Count > 0)
                {
                    listProductsView.ScrollIntoView(listProductsView.Items[0]);
                }
            }
        }
    }
}
