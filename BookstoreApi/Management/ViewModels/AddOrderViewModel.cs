using Management.Cores;
using System;
using System.Collections.Generic;
using Management.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Management.ViewModels
{
    class AddOrderViewModel : ObservableObject
    {
        public RelayCommand ProductViewCommand { get; set; }
        public RelayCommand OrderViewCommand { get; set; }

        public ShoppingViewModel ProductVM { get; set; }
        
        public CartViewModel OrderVM { get; set; }


        private ObservableCollection<Book> _orderList;
        public ObservableCollection<Book> OrderList { 
            get
            {
                return _orderList;
            }
            }
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }


        private void OnOrderPlaced(object sender, Book order)
        {
            if (order != null)
            {
                OrderList.Add(order);
            }
        }
        public AddOrderViewModel()
        {
         
        
            ProductVM = new ShoppingViewModel();
            ProductVM.OrderPlaced += OnOrderPlaced;
            OrderVM = new CartViewModel(OrderList);


            CurrentView = ProductVM;

            ProductViewCommand = new RelayCommand(obj =>
            {
                CurrentView = ProductVM;
            });

            OrderViewCommand = new RelayCommand(obj =>
            {
                CurrentView = OrderVM;
            });


        }
    }
}
