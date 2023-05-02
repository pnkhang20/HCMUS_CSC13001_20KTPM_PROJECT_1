using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

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
