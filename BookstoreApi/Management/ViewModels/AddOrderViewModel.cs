﻿using Management.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management.ViewModels
{
    class AddOrderViewModel : ObservableObject
    {
        public RelayCommand ProductViewCommand { get; set; }
        public RelayCommand OrderViewCommand { get; set; }

        public ShoppingViewModel ProductVM { get; set; }
        public CartViewModel OrderVM { get; set; }
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
        public AddOrderViewModel()
        {
         
            ProductVM = new ShoppingViewModel();
            OrderVM = new CartViewModel();
      
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
