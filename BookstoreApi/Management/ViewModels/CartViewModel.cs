using System;
using System.Collections.Generic;
using System.Linq;
using Management.Models;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Management.Cores;

namespace Management.ViewModels
{
    class CartViewModel: ObservableObject
    {
        public ObservableCollection<Book> OrderList { get; } = new ObservableCollection<Book>();

        public CartViewModel(ObservableCollection<Book> orderList=null) {
            if (orderList != null)
            {
                OrderList = orderList;
            }
        }
    }

}
