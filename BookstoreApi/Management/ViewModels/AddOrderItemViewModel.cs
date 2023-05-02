using Management.Cores;
using Management.Models;
using Management.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;

namespace Management.ViewModels
{
    class AddOrderItemViewModel : ObservableObject
    {

        private const string OrderApiUrl = "https://localhost:7122/api/Orders";
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

        public AddOrderItemViewModel(ObservableCollection<Book> booksList, Order selectedOrder)
        {
            SelectedOrder = selectedOrder;
            // Exclude books that are already in the order items list of the selected order
            var excludedBooks = new HashSet<string>(SelectedOrder.OrderItemsList.Select(item => item.Book.Id));
            BooksList = new ObservableCollection<Book>(booksList.Where(book => !excludedBooks.Contains(book.Id)));
        }

        private ICommand _addNewOrderItemCommand;
        public ICommand AddNewOrderItemCommand
        {
            get
            {
                if (_addNewOrderItemCommand == null)
                {
                    _addNewOrderItemCommand = new RelayCommand(async (param) =>
                    {
                        // Prompt the user for confirmation before saving changes
                        MessageBoxResult result = MessageBox.Show($"Add {SelectedBook.Title} to cart?", "Confirm Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                                    var newOrderItem = new OrderItem
                                    {
                                        Id = "string",
                                        Book = SelectedBook,
                                        Quantity = 1
                                    };
                                    SelectedOrder.OrderItemsList.Add(newOrderItem);
                                    OnPropertyChanged(nameof(OrderItems)); // add this line to reload the order items
                                    // Use the SelectedBook object (which is a clone of the original book) to make the PUT request
                                    HttpResponseMessage response = await client.PutAsJsonAsync(url, SelectedOrder);
                                    // Check if the response is successful
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // Get the parent window of the AddOrderItemView
                                        var parentWindow = Application.Current.Windows.OfType<EditOrderView>().FirstOrDefault();
                                        // Get the view model of the parent window
                                        var parentViewModel = parentWindow.DataContext as EditOrderViewModel;
                                        parentViewModel.LoadOrderItems();
                                        //var editOrderView = (OrderViewModel)mainViewModel.LoadOrder();
                                        MessageBox.Show($"{SelectedBook.Title} added to cartsuccessfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                        Window curWin = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                                        curWin?.Close();
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
                return _addNewOrderItemCommand;
            }
        }
    }
}
