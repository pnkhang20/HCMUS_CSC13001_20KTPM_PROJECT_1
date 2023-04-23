using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace test;

public class Book
{
    public string Id { get; set; }
    public string BookName { get; set; }
    public string Author { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Cover { get; set; }

    //public bool IsSelected { get; set; }

}