using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Management.Models
{
    public class Book : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Cover { get; set; }

        public Category Category { get; set; }
     
        public event PropertyChangedEventHandler? PropertyChanged;

        public Book Clone()
        {
            return new Book
            {
                Id = this.Id,
                Title = this.Title,
                Author = this.Author,                
                Price = this.Price,
                Quantity = this.Quantity,
                Category = new Category
                {
                    Id = this.Category.Id,
                    CategoryName = this.Category.CategoryName
                },
                Cover = this.Cover
            };
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Author))
            {
                return false;
            }

            if (Price == null)
            {
                return false;
            }

            if (Category == null || Category.Id == null)
            {
                return false;
            }
            return true;
        }

    }

}
