using System.ComponentModel;

namespace Management.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Id { get; set; }

        public Book Book { get; set; }

        public int Quantity { get; set; }
    }
}
