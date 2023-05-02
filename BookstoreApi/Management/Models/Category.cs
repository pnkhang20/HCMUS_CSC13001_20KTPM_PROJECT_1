using System.ComponentModel;

namespace Management.Models
{
    public class Category : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Id { get; set; } = "string";
        public string CategoryName { get; set; } = null!;

    }
}
