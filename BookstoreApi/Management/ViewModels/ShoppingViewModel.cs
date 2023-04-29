﻿using Management.Cores;
using System.Collections.ObjectModel;
using System.Net.Http;
using Management.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using Management.Views;

namespace Management.ViewModels
{
    class ShoppingViewModel: ObservableObject
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

        private string _minPrice;
        public string MinPrice
        {
            get { return _minPrice; }
            set { _minPrice = value; OnPropertyChanged(); }
        }

        private string _maxPrice;
        public string MaxPrice
        {
            get { return _maxPrice; }
            set { _maxPrice = value; OnPropertyChanged(); }
        }

        public bool HasNextPage;
        public bool HasPrevPage;
        public int _page;
        public int Page
        {
            get { return _page; }
            set { _page = value; OnPropertyChanged(); }
        }

        private int _totalPageCount;
        public int TotalPageCount
        {
            get { return _totalPageCount; }
            set { _totalPageCount = value; OnPropertyChanged(); }
        }


        private ICommand _nextPageCommand;
        public ICommand NextPageCommand
        {
            get
            {
                if (_nextPageCommand == null)
                {
                    _nextPageCommand = new RelayCommand(
                        async (param) =>
                        {
                            if (HasNextPage)
                            {
                                await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, Page + 1);
                            }
                        },
                        (param) => HasNextPage
                    );
                }

                return _nextPageCommand;
            }
        }

        private ICommand _previousPageCommand;
        public ICommand PrevPageCommand
        {
            get
            {
                if (_previousPageCommand == null)
                {
                    _previousPageCommand = new RelayCommand(
                        async (param) =>
                        {
                            if (HasPrevPage)
                            {
                                await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, Page - 1);
                            }
                        },
                        (param) => HasPrevPage
                    );
                }
                return _previousPageCommand;
            }
        }

        public event EventHandler<Book> OrderPlaced;

        private ICommand _addIntoCartCommand;
        public ICommand AddIntoCartCommand
        {
            get
            {
                if (_addIntoCartCommand == null)
                {

                    //var addOrderViewModel = new AddOrderViewModel(selectedBook);
                    _addIntoCartCommand = new RelayCommand(
                          async (param) =>
                          {
                              OrderPlaced?.Invoke(this, selectedBook);

                          }
                    );

                }
                return _addIntoCartCommand;
            }
        }

        public ShoppingViewModel()
        {
            Page = 1;
            LoadBooks();
            LoadCategory();
            SelectedBook = Books.FirstOrDefault();
            // Wire up the SearchText property
            this.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(SearchText))
                {
                    await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice);
                }
            };
            this.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(MinPrice))
                {
                    await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice);
                }
            };

            this.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(MaxPrice))
                {
                    await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice);
                }
            };
        }

        public async Task LoadBooks(string searchText = null, Category category = null, string minPrice = null, string maxPrice = null, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                Page = pageNumber;

                var urlBuilder = new StringBuilder(BookApiUrl);
                urlBuilder.Append("/search");

                // Add query parameters for search text and category filtering, if applicable
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    urlBuilder.AppendFormat("?name={0}", HttpUtility.UrlEncode(searchText));
                }

                if (category != null && category.CategoryName != "All Category")
                {
                    if (urlBuilder.ToString().Contains("?"))
                    {
                        urlBuilder.AppendFormat("&categoryName={0}", HttpUtility.UrlEncode(category.CategoryName));
                    }
                    else
                    {
                        urlBuilder.AppendFormat("?categoryName={0}", HttpUtility.UrlEncode(category.CategoryName));
                    }
                }

                if (!string.IsNullOrWhiteSpace(minPrice))
                {
                    if (urlBuilder.ToString().Contains("?"))
                    {
                        urlBuilder.AppendFormat("&minPrice={0}", HttpUtility.UrlEncode(minPrice));
                    }
                    else
                    {
                        urlBuilder.AppendFormat("?minPrice={0}", HttpUtility.UrlEncode(minPrice));
                    }
                }

                if (!string.IsNullOrWhiteSpace(maxPrice))
                {
                    if (urlBuilder.ToString().Contains("?"))
                    {
                        urlBuilder.AppendFormat("&maxPrice={0}", HttpUtility.UrlEncode(maxPrice));
                    }
                    else
                    {
                        urlBuilder.AppendFormat("?maxPrice={0}", HttpUtility.UrlEncode(maxPrice));
                    }
                }
                // Add query parameters for pagination
                if (pageNumber != 0)
                {
                    if (urlBuilder.ToString().Contains("?"))
                    {
                        urlBuilder.AppendFormat("&pageNumber={0}&pageSize={1}", pageNumber, pageSize);
                    }
                    else
                    {
                        urlBuilder.AppendFormat("?pageNumber={0}&pageSize={1}", pageNumber, pageSize);
                    }
                }


                var response = await httpClient.GetAsync(urlBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {

                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonConvert.DeserializeObject<List<Book>>(content);
                    HasNextPage = books.Count == pageSize;
                    HasPrevPage = Page > 1;

                    Books.Clear();
                    foreach (var book in books)
                    {
                        Books.Add(book);
                    }
                }
            }
            catch (Exception ex) { }
        }

        public async Task LoadCategory()
        {
            try
            {
                Categories.Clear();
                var allCategory = new Category() { CategoryName = "All Category" };
                Categories.Add(allCategory);
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
                    SelectedCategory = allCategory;
                }
            }
            catch (Exception ex)
            {
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
                        await LoadBooks(SearchText, SelectedCategory);
                    });
                }
                return categorySelectionChangedCommand;
            }
        }
    }
}
