using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management.Models
{
    public class Order : INotifyPropertyChanged
    {
        public string Id { get; set; }

        public string ShippingAddress { get; set; }

        public string CustomerName { get; set; }

        public string CustomerPhone { get; set; }

        public double TotalPrice { get; set; }

        public bool OrderIsDone { get; set; } = true;

        public string OrderedDate { get; set; }

        public List<OrderItem> OrderItemsList { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

}
