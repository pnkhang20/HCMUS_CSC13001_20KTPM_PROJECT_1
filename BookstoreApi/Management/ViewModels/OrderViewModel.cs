using Management.Cores;
using Management.Models;
using Management.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Management.ViewModels
{
    class OrderViewModel : ObservableObject
    {

        private const string OrderApiUrl = "https://localhost:7122/api/Orders";
        private readonly HttpClient httpClient = new HttpClient();

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get { return _startDate; }
            set { _startDate = value; OnPropertyChanged(); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get { return _endDate; }
            set { _endDate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Order> Orders { get; } = new ObservableCollection<Order>();

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get { return _selectedOrder; }
            set { _selectedOrder = value; OnPropertyChanged(); }
        }

        public OrderViewModel()
        {
            LoadOrders();
        }
        private ICommand _filterOrdersCommand;
        public ICommand FilterOrdersCommand
        {
            get
            {
                if (_filterOrdersCommand == null)
                {
                    _filterOrdersCommand = new RelayCommand(param =>
                    {
                        LoadOrders(StartDate, EndDate);
                    });
                }
                return _filterOrdersCommand;
            }
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


        private ICommand _addOrderCommand;
        public ICommand AddOrderCommand
        {
            get
            {
                if (_addOrderCommand == null)
                {
                    _addOrderCommand = new RelayCommand(async (param) =>
                    {
                        var messageBoxResult = MessageBox.Show($"Create a new order?", "Create new Order", MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            var to_add = new Order
                            {
                                Id = "string", // Set the ID to an empty string or a default value
                                OrderItemsList = new ObservableCollection<OrderItem>(), // Initialize the order items list to an empty list
                                ShippingAddress = "string",
                                CustomerName = "string",
                                CustomerPhone = "string",
                                OrderIsDone = false,
                                TotalPrice = 0,
                                OrderedDate = DateTime.UtcNow
                            };

                            try
                            {
                                using (HttpClient client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://localhost:7122/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    HttpResponseMessage response = await httpClient.PostAsJsonAsync(OrderApiUrl, to_add);
                                    if (response.IsSuccessStatusCode)
                                    {

                                        // Update the original book with the changes made in the SelectedBook object
                                        // Display a success message to the user                                        
                                        MessageBox.Show("Successfully added new order! Please edit it later on!");
                                        // Close the current window and update the parent view
                                        LoadOrders();

                                    }
                                    else
                                    {
                                        // Display an error message to the user
                                        MessageBox.Show($"Failed to add new product! {response.ReasonPhrase}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                    });
                }
                return _addOrderCommand;
            }
        }


        public async Task LoadOrders(DateTime? startDate = null, DateTime? endDate = null)
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
                        // Filter the orders based on the selected date range
                        if ((startDate == null || order.OrderedDate.AddDays(1) >= startDate.Value) &&
                            (endDate == null || order.OrderedDate.AddDays(-1) <= endDate.Value))
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
            }
            catch (Exception ex)
            {
            }
        }




    }

}