using Management.Cores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Management.ViewModels
{
    class MainViewModel:ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand ProductViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public ProductViewModel ProductVM { get; set; }
        

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView;}
            set { 
                _currentView = value; 
                OnPropertyChanged();
                }
        }
        public MainViewModel()
        { 
            HomeVM = new HomeViewModel();
            ProductVM = new ProductViewModel();
            
            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(obj =>
            {
                CurrentView = HomeVM;
            });

            ProductViewCommand = new RelayCommand(obj =>
            {
                CurrentView = ProductVM;
            });


        }
    }
}
