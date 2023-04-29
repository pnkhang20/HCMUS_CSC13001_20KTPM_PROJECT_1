using Management.Cores;
using Management.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;
using System.Windows.Input;
using Management.Views;

namespace Management.ViewModels
{
    class ManagementViewModel: ObservableObject
    {
        private const string OrderApiUrl = "https://localhost:7122/api/Orders";
        private readonly HttpClient httpClient = new HttpClient();
        public ObservableCollection<Order> OrdersList { get; } = new ObservableCollection<Order>();
        private Order selectedOrder;
        public Order SelectedOrder
        {
            get { return selectedOrder; }
            set { selectedOrder = value; OnPropertyChanged(); }
        }

    
        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set { searchText = value; OnPropertyChanged(); }
        }


        private ICommand _addOrderCommand;
        public ICommand AddOrderCommand
        {
            get
            {
                if (_addOrderCommand == null)
                {
                    _addOrderCommand = new RelayCommand(async (param) =>
                    {
                        var addOrderVM = new AddOrderViewModel();
                        var addOrderWindow = new AddOrderView();
                        addOrderWindow.DataContext = addOrderVM;
                        addOrderWindow.ShowDialog();
                       // await LoadBooks();
                    });
                }
                return _addOrderCommand;
            }
        }




        public ManagementViewModel()
        {
           
            LoadOrders();


           // SelectedOrder = OrdersList.FirstOrDefault();

        }

        public async Task LoadOrders(string dateStart = null, string dateEnd = null, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                //Page = pageNumber;

                var urlBuilder = new StringBuilder(OrderApiUrl);
                //urlBuilder.Append("/search");

         

                
                // Add query parameters for pagination
                //if (pageNumber != 0)
                //{
                //    if (urlBuilder.ToString().Contains("?"))
                //    {
                //        urlBuilder.AppendFormat("&pageNumber={0}&pageSize={1}", pageNumber, pageSize);
                //    }
                //    else
                //    {
                //        urlBuilder.AppendFormat("?pageNumber={0}&pageSize={1}", pageNumber, pageSize);
                //    }
                //}


                var response = await httpClient.GetAsync(urlBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {

                    var content = await response.Content.ReadAsStringAsync();
                    var orders = JsonConvert.DeserializeObject<List<Order>>(content);
                    //HasNextPage = books.Count == pageSize;
                    //HasPrevPage = Page > 1;

                    OrdersList.Clear();
                    foreach (var order in orders)
                    {
                        OrdersList.Add(order);
                    }
                }
            }
            catch (Exception ex) { }
        }

      
    }
}
