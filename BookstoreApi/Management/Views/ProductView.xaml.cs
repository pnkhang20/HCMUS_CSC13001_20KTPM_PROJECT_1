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
    /// Interaction logic for ProductView.xaml
    /// </summary>
    public partial class ProductView : UserControl
    {
        public ProductView()
        {
            InitializeComponent();
        }
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta < 0)
            {
                // Scroll down
                ListViewProducts.ScrollIntoView(ListViewProducts.Items[ListViewProducts.Items.Count - 1]);
            }
            else if (e.Delta > 0)
            {
                // Scroll up
                ListViewProducts.ScrollIntoView(ListViewProducts.Items[0]);
            }
        }

    }
}
