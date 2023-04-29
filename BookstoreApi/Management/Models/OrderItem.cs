using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management.Models
{
    public class OrderItem:INotifyPropertyChanged
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public Book BookOrder { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
