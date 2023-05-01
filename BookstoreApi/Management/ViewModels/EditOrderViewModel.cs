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
using System.Text;
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

                                        // Update the original book with the changes made in the SelectedBook object
                                        // Display a success message to the user                                                                                
                                        var mainWindow = Application.Current.MainWindow;
                                        var mainViewModel = (MainViewModel)mainWindow.DataContext;
                                        var orderView = (OrderViewModel)mainViewModel.OrderVM;
                                        orderView.LoadOrders();                                        
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

        public EditOrderViewModel(Order selectedOrder)
        {
            SelectedOrder = selectedOrder;
            OrderItems = new ObservableCollection<OrderItem>(SelectedOrder.OrderItemsList);
        }
    }
}
