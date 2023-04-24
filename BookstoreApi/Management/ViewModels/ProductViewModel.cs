using Management.Cores;
using System.Collections.ObjectModel;
using System.Net.Http;
using Management.Models;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace Management.ViewModels
{
    class ProductViewModel : ObservableObject
    {
        private const string BookApiUrl = "https://localhost:7122/api/Books";
        private const string CategoryApiUrl = "https://localhost:7122/api/Categories";
        private readonly HttpClient httpClient = new HttpClient();
        public ObservableCollection<Book> Books { get; } = new ObservableCollection<Book>();
        private Book selectedBook;
        public Book SelectedBook
        {
            get { return selectedBook; }
            set { selectedBook = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        private Category selectedCategory;
        
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set { selectedCategory = value; OnPropertyChanged(); }
        }
        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set { searchText = value; OnPropertyChanged(); }
        }

        private ICommand deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(async (param) =>
                    {
                        if (SelectedBook != null)
                        {
                            var messageBoxResult = MessageBox.Show($"Are you sure you want to delete {SelectedBook.Title}?", "Delete Book", MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    HttpResponseMessage response = await httpClient.DeleteAsync($"{BookApiUrl}/{SelectedBook.Id}");

                                    if (response.IsSuccessStatusCode)
                                    {
                                        Books.Remove(SelectedBook);
                                        SelectedBook = null;
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
                return deleteCommand;
            }
        }
        public ProductViewModel()
        {
            LoadBooks();
            LoadCategory();
            SelectedBook = Books.FirstOrDefault();
            // Wire up the SearchText property
            this.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(SearchText))
                {                    
                    await LoadBooks(SearchText);
                }
            };
        }

        private async Task LoadBooks(string searchText = null)
        {
            try
            {
                var urlBuilder = new StringBuilder(BookApiUrl);
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    urlBuilder.Append($"/search?name={HttpUtility.UrlEncode(searchText)}&pageNumber=1&pageSize=10");                
                }
                var response = await httpClient.GetAsync(urlBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonConvert.DeserializeObject<List<Book>>(content);

                    // Update the Books collection with the new books
                    Books.Clear();
                    foreach (var book in books)
                    {
                        Books.Add(book);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private async Task LoadCategory()
        {
            try
            {
                // Add all category
                Categories.Add(new Category() { CategoryName = "All" });
                var response = await httpClient.GetAsync(CategoryApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var categories = JsonConvert.DeserializeObject<List<Category>>(content);

                    foreach (var category in categories)
                    {
                        var existingCategory = Categories.FirstOrDefault(b => b.Id == category.Id);
                        if (existingCategory == null)
                        {
                            Categories.Add(category);
                        }
                        else
                        {
                            int index = Categories.IndexOf(existingCategory);
                            Categories[index] = category;
                        }
                    }
                                 
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private async Task LoadBooksForCategory(Category category)
        {
            try
            {
                var urlBuilder = new StringBuilder(BookApiUrl);
                urlBuilder.Append("/search");

                if (category != null && category.CategoryName != "All")
                {
                    // Add query parameters for filtering by category ID
                    urlBuilder.AppendFormat("?categoryName={0}", HttpUtility.UrlEncode(category.CategoryName));
                }
                // Add query parameters for filtering by category ID
                //urlBuilder.AppendFormat("?categoryName={0}", HttpUtility.UrlEncode(category.CategoryName));

                var response = await httpClient.GetAsync(urlBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonConvert.DeserializeObject<List<Book>>(content);

                    // Update the Books collection with the new books
                    Books.Clear();
                    foreach (var book in books)
                    {
                        Books.Add(book);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private ICommand categorySelectionChangedCommand;
        public ICommand CategorySelectedCommand
        {
            get
            {
                if (categorySelectionChangedCommand == null)
                {
                    categorySelectionChangedCommand = new RelayCommand(async (param) =>
                    {
                        // Load the books for the selected category
                        await LoadBooksForCategory(SelectedCategory);
                    });
                }
                return categorySelectionChangedCommand;
            }
        }
    }
}
