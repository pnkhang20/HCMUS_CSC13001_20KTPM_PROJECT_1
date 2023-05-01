using Management.Cores;
using Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Management.ViewModels
{
    class AddOrderItemViewModel : ObservableObject
    {        
        private ObservableCollection<Book> _booksList;
        public ObservableCollection<Book> BooksList
        {
            get { return _booksList; }
            set { _booksList = value; OnPropertyChanged(); }
        }
        private Book _selectedBook;
        public Book SelectedBook
        {
            get { return _selectedBook; }
            set { _selectedBook = value; OnPropertyChanged(); }
        }
        private Order _selectedOrder;

        public Order SelectedOrder
        {
            get { return _selectedOrder; }
            set { _selectedOrder = value; OnPropertyChanged(); }
        }

        private ObservableCollection<OrderItem> _orderItems;
        public ObservableCollection<OrderItem> OrderItems
        {
            get { return _orderItems; }
            set { _orderItems = value; OnPropertyChanged(); }
        }

        private ICommand _addNewOrderItemCommand;
        public ICommand AddNewOrderItemCommand
        {
            get
            {
                if (_addNewOrderItemCommand == null)
                {
                    _addNewOrderItemCommand = new RelayCommand(param =>
                    {

                    });
                }
                return _addNewOrderItemCommand;
            }
        }
        public AddOrderItemViewModel(ObservableCollection<Book> booksList, Order selectedOrder)
        {
            BooksList = booksList; 
            SelectedOrder = selectedOrder;
        }
    }
}
