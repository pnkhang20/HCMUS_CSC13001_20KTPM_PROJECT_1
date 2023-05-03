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
    class EditOrderViewModel : ObservableObject
    {
        private const string OrderApiUrl = "https://localhost:7122/api/Orders";
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

        private ObservableCollection<Book> _booksList;
        public ObservableCollection<Book> BooksList
        {
            get { return _booksList; }
            set { _booksList = value; OnPropertyChanged(); }
        }

        private OrderItem _selectedOrderItem;
        public OrderItem SelectedOrderItem
        {
            get { return _selectedOrderItem; }
            set { _selectedOrderItem = value; OnPropertyChanged(); }
        }

        public EditOrderViewModel(Order selectedOrder)
        {
            SelectedOrder = selectedOrder;
            SelectedOrderItem = SelectedOrder.OrderItemsList.FirstOrDefault();
            OrderItems = new ObservableCollection<OrderItem>(SelectedOrder.OrderItemsList);
            LoadBooksAsync();
        }

        private ICommand _deleteOrderItemCommand;
        public ICommand DeleteOrderItemCommand
        {
            get
            {
                if (_deleteOrderItemCommand == null)
                {
                    _deleteOrderItemCommand = new RelayCommand(async (param) =>
                    {
                        // Prompt the user for confirmation before saving changes
                        MessageBoxResult result = MessageBox.Show($"Delete this item?", "Confirm Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {

                            SelectedOrder.OrderItemsList.Remove(SelectedOrderItem);
                            // Reload the OrderItems collection to update the UI
                            OrderItems = new ObservableCollection<OrderItem>(SelectedOrder.OrderItemsList);
                            try
                            {
                                // Send a PUT request to update the order
                                using (HttpClient client = new HttpClient())
                                {
                                    // Set the ID parameter in the URL
                                    string url = $"{OrderApiUrl}/{SelectedOrder.Id}";
                                    // Serialize the edited order as JSON and send the PUT request
                                    client.BaseAddress = new Uri("https://localhost:7122/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    // Use the SelectedBook object (which is a clone of the original book) to make the PUT request
                                    HttpResponseMessage response = await client.PutAsJsonAsync(url, SelectedOrder);
                                    // Check if the response is successful
                                    if (response.IsSuccessStatusCode)
                                    {
                                        MessageBox.Show($"Item removed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                        OrderItems.Remove(SelectedOrderItem); // Remove the item from the collection
                                        OnPropertyChanged(nameof(OrderItems)); // add this line to reload the order items
                                        var editOrderViewModel = new EditOrderViewModel(SelectedOrder);
                                        var editOrderView = (EditOrderView)Application.Current.Windows.OfType<EditOrderView>().FirstOrDefault();

                                        LoadOrderItems();
                                        OnPropertyChanged(nameof(OrderItems)); // Notify the view to update the collection

                                        if (editOrderView != null)
                                        {
                                            editOrderView.DataContext = editOrderViewModel;
                                        }
                                    }
                                    else
                                    {
                                        string errorMessage = $"Failed to save changes. Error message: {response.ReasonPhrase}";
                                        MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An error occurred while saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    });
                }
                return _deleteOrderItemCommand;
            }
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
                        var addOrderItemVM = new AddOrderItemViewModel(BooksList, SelectedOrder);
                        var addOrderItemWindow = new AddOrderItemView();
                        addOrderItemWindow.DataContext = addOrderItemVM;
                        addOrderItemWindow.ShowDialog();
                        OnPropertyChanged(nameof(OrderItems)); // add this line to reload the order items
                    });
                }
                OnPropertyChanged(nameof(OrderItems)); // add this line to reload the order items
                return _addNewOrderItemCommand;

            }
        }

        private ICommand _saveEditedOrder;
        public ICommand SaveEditedOrder
        {
            get
            {
                if (_saveEditedOrder == null)
                {
                    _saveEditedOrder = new RelayCommand(async (param) =>
                    {
                        // Prompt the user for confirmation before saving changes
                        MessageBoxResult result = MessageBox.Show("Are you sure you want to save these changes?", "Confirm Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                // Send a PUT request to update the order
                                using (HttpClient client = new HttpClient())
                                {
                                    // Set the ID parameter in the URL
                                    string url = $"{OrderApiUrl}/{SelectedOrder.Id}";
                                    // Serialize the edited order as JSON and send the PUT request
                                    client.BaseAddress = new Uri("https://localhost:7122/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    // Use the SelectedBook object (which is a clone of the original book) to make the PUT request
                                    HttpResponseMessage response = await client.PutAsJsonAsync(url, SelectedOrder);
                                    // Check if the response is successful
                                    if (response.IsSuccessStatusCode)
                                    {
                                        var newOrderVM = new OrderViewModel();
                                        var mainWindow = Application.Current.MainWindow;
                                        var mainViewModel = (MainViewModel)mainWindow.DataContext;
                                        //var orderView = (OrderViewModel)mainViewModel.OrderVM;
                                        //orderView.LoadOrders();                                        
                                        Window parentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                                        parentWindow?.Close();
                                        MessageBox.Show("Changes saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        string errorMessage = $"Failed to save changes. Error message: {response.ReasonPhrase}";
                                        MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An error occurred while saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    });
                }
                return _saveEditedOrder;
            }
        }

        private ICommand _markOrderAsDoneCommand;
        public ICommand MarkOrderAsDoneCommand
        {
            get
            {
                if (_markOrderAsDoneCommand == null)
                {
                    _markOrderAsDoneCommand = new RelayCommand(async (param) =>
                    {
                        // Prompt the user for confirmation before saving changes
                        MessageBoxResult result = MessageBox.Show("Are you sure to mark this order as done? This cannot be undone", "Confirm Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                // Send a PUT request to update the order
                                using (HttpClient client = new HttpClient())
                                {
                                    SelectedOrder.OrderIsDone = true;
                                    // Set the ID parameter in the URL
                                    string url = $"{OrderApiUrl}/{SelectedOrder.Id}";
                                    // Serialize the edited order as JSON and send the PUT request
                                    client.BaseAddress = new Uri("https://localhost:7122/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    // Use the SelectedBook object (which is a clone of the original book) to make the PUT request
                                    HttpResponseMessage response = await client.PutAsJsonAsync(url, SelectedOrder);
                                    // Check if the response is successful
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // Update the original book with the changes made in the SelectedBook object
                                        // Display a success message to the user                                                                                
                                        var mainWindow = Application.Current.MainWindow;
                                        var mainViewModel = (MainViewModel)mainWindow.DataContext;
                                        var orderView = (OrderViewModel)mainViewModel.OrderVM;
                                        orderView.LoadOrders();
                                        Window parentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                                        parentWindow?.Close();
                                        MessageBox.Show("Changes saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        string errorMessage = $"Failed to save changes. Error message: {response.ReasonPhrase}";
                                        MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"An error occurred while saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    });
                }
                return _markOrderAsDoneCommand;
            }
        }

        public async Task LoadBooksAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync("https://localhost:7122/api/Books");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var books = JsonConvert.DeserializeObject<List<Book>>(json);
                    BooksList = new ObservableCollection<Book>(books);
                }
                else
                {

                }
            }
        }
        public async void LoadOrderItems()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"{OrderApiUrl}/{SelectedOrder.Id}";
                    client.BaseAddress = new Uri("https://localhost:7122/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var orderJson = await response.Content.ReadAsStringAsync();
                        SelectedOrder = JsonConvert.DeserializeObject<Order>(orderJson);
                        OrderItems = new ObservableCollection<OrderItem>(SelectedOrder.OrderItemsList);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the order items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
