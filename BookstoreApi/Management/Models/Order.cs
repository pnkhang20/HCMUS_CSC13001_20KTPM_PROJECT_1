using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Management.Models
{
    public class Order : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id { get; set; } = null!;
        public ObservableCollection<OrderItem> OrderItemsList { get; set; } = null!;
        // Order shipping address       
        public string ShippingAddress { get; set; } = null!;

        // Order customer name               
        public string CustomerName { get; set; } = null!;

        // Order customer phone number               
        public string CustomerPhone { get; set; } = null!;

        // Order Subtotal               
        public decimal TotalPrice { get; set; }

        // Order status               
        public bool OrderIsDone { get; set; } = false;

        // Order date               
        public DateTime OrderedDate { get; set; }

        public Order()
        {
            OrderIsDone = false;
        }
    }
}
