using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management.Models
{
    public class OrderItem
    {
        public string Id { get; set; }
        public string Quantity { get; set; }

        public Book Book { get; set; }
    }
}
