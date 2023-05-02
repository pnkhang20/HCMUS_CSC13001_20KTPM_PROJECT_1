using Management.Cores;

namespace Management.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand ProductViewCommand { get; set; }
        public RelayCommand ManagementViewCommand { get; set; }
        public RelayCommand CategoryViewCommand { get; set; }
        public RelayCommand OrderViewCommand { get; set; }
        public RelayCommand StatisticViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public ProductViewModel ProductVM { get; set; }
        public CategoryViewModel CategoryVM { get; set; }
        public OrderViewModel OrderVM { get; set; }
        public StatisticViewModel StatisticVM { get; set; }

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
        public void ReloadProductView()
        {
            ProductVM.LoadCategory();
            ProductVM.LoadBooks();
        }
        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            ProductVM = new ProductViewModel();
            CategoryVM = new CategoryViewModel();
            OrderVM = new OrderViewModel();
            StatisticVM = new StatisticViewModel();
            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(obj =>
            {
                CurrentView = HomeVM;
            });

            ProductViewCommand = new RelayCommand(obj =>
            {
                CurrentView = ProductVM;
            });

            CategoryViewCommand = new RelayCommand(obj =>
            {
                CurrentView = CategoryVM;
            });

            OrderViewCommand = new RelayCommand(obj =>
            {
                CurrentView = OrderVM;
            });

            StatisticViewCommand = new RelayCommand(obj =>
            {
                CurrentView = StatisticVM;
            });
        }
    }
}
