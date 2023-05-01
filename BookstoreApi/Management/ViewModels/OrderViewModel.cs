using Management.Cores;
using Management.Models;
using Management.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Management.ViewModels
{
    class OrderViewModel : ObservableObject
    {

        private const string BookApiUrl = "https://localhost:7122/api/Books";
        private const string CategoryApiUrl = "https://localhost:7122/api/Categories";
        private const string OrderApiUrl = "https://localhost:7122/api/Orders";
        private readonly HttpClient httpClient = new HttpClient();

        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get { return _selectedOrder; }
            set { _selectedOrder = value; OnPropertyChanged(); }
        }

        private ICommand _removeOrderCommand;
        public ICommand RemoveOrderCommand
        {
            get
            {
                if (_removeOrderCommand == null)
                {
                    _removeOrderCommand = new RelayCommand(async (param) =>
                    {
                        if (SelectedOrder != null)
                        {
                            var messageBoxResult = MessageBox.Show($"Are you sure you want to delete this orders?", "Delete Order", MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    HttpResponseMessage response = await httpClient.DeleteAsync($"{OrderApiUrl}/{SelectedOrder.Id}");

                                    if (response.IsSuccessStatusCode)
                                    {
                                        Orders.Remove(SelectedOrder);
                                        SelectedOrder = null;
                                    }
                                    else
                                    {
                                        if (SelectedOrder.OrderIsDone == true)
                                        {
                                            var msgBox = MessageBox.Show($"This ordered is already marked as done so you cannot delete it!", "Delete Order");
                                        }
                                    } 
                                        
                                }
                                catch (Exception ex)
                                {
                                    // Handle error
                                }
                            }
                        }
                    });
                }
                return _removeOrderCommand;
            }
        }

        private ICommand _editOrderCommand;
        public ICommand EditOrderCommand
        {
            get
            {
                if (_editOrderCommand == null)
                {
                    _editOrderCommand = new RelayCommand(async (param) =>
                    {
                        // Check if the selected order is done
                        if (SelectedOrder.OrderIsDone)
                        {
                            MessageBox.Show("This order is already marked as done and cannot be edited.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var editOrderVM = new EditOrderViewModel(SelectedOrder);
                        var editOrderWindow = new EditOrderView();
                        editOrderWindow.DataContext = editOrderVM;
                        editOrderWindow.ShowDialog();
                        await LoadOrders();
                    });
                }
                return _editOrderCommand;
            }
        }

        //private ICommand _addOrderCommand;
        //public ICommand AddOrderCommand
        //{
        //    get
        //    {
        //        if (_addOrderCommand == null)
        //        {
        //            _addOrderCommand = new RelayCommand(async (param) =>
        //            {
        //                var addOrderVM = new AddOrderViewModel();
        //                var addOrderWindow = new AddProductView();
        //                addOrderWindow.DataContext = addOrderVM;
        //                addOrderWindow.ShowDialog();
        //                //await LoadBooks();
        //            });
        //        }
        //        return _addOrderCommand;
        //    }
        //}


        public async Task LoadOrders()
        {
            try
            {
                Orders.Clear();                
                var response = await httpClient.GetAsync(OrderApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var orders = JsonConvert.DeserializeObject<List<Order>>(content);

                    foreach (var order in orders)
                    {
                        var existingOrder = Orders.FirstOrDefault(b => b.Id == order.Id);
                        if (existingOrder == null)
                        {
                            Orders.Add(order);
                        }
                        else
                        {
                            int index = Orders.IndexOf(existingOrder);
                            Orders[index] = order;
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
            }
        }


        public OrderViewModel()
        {
            LoadOrders();
        }
    }

}