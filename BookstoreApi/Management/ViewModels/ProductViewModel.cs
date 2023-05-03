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
using System.Web;
using Microsoft.Win32;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Azure;
using System.Windows;
using System.Windows.Input;



namespace Management.ViewModels
{
    class ProductViewModel : ObservableObject
    {

        private const string BookApiUrl = "https://localhost:7122/api/Books";
        private const string CategoryApiUrl = "https://localhost:7122/api/Categories";
        private readonly HttpClient httpClient = new HttpClient();
        public ObservableCollection<Book> Books { get; set; } = new ObservableCollection<Book>();


        public ObservableCollection<Book> AppearBooks { get; set; } = new ObservableCollection<Book>();
        private Book _selectedBook;
        public Book SelectedBook
        {
            get { return _selectedBook; }
            set { _selectedBook = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        private Category _selectedCategory;

        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set { _selectedCategory = value; OnPropertyChanged(); }
        }
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; OnPropertyChanged(); }
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
                          
                            if (CurrentPage + 1 <= TotalPageCount)
                            {
                                CurrentPage += 1;
                                AppearBooks = new ObservableCollection<Book>(Books.Skip((CurrentPage - 1) * 5).Take(5));
                                SelectedPage = CurrentPage - 1;

                                if (CurrentPage == TotalPageCount)
                                {
                                    HasNextPage = false;
                                }
                            }
                        }
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
                            if (CurrentPage - 1 >=1)
                            {
                                CurrentPage -= 1;
                                AppearBooks = new ObservableCollection<Book>(Books.Skip((CurrentPage - 1) * 5).Take(5));
                                SelectedPage = CurrentPage - 1;

                                if (CurrentPage == TotalPageCount)
                                {
                                    HasPrevPage = false;
                                }
                            }
                        }
                       
                    );
                }
                return _previousPageCommand;
            }
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(async (param) =>
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
                                        
                                        await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                                        SelectedBook = null;
                                        SelectedPage = CurrentPage - 1;
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
                return _deleteCommand;
            }
        }

        private ICommand _addProductCommand;
        public ICommand AddProductCommand
        {
            get
            {
                if (_addProductCommand == null)
                {
                    _addProductCommand = new RelayCommand(async (param) =>
                    {
                        var addBookVM = new AddProductViewModel(Categories);
                        var addBookWindow = new AddProductView();
                        addBookWindow.DataContext = addBookVM;
                        addBookWindow.ShowDialog();
                        await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                        SelectedPage = Pages.Count()-1;
                    });
                }
                return _addProductCommand;
            }
        }


        private ICommand _importProductCommand;
        public ICommand ImportProductCommand
        {
            get
            {
                if (_importProductCommand == null)
                {
                    _importProductCommand = new RelayCommand(async (param) =>
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            string filename = openFileDialog.FileName;
                            var document = SpreadsheetDocument.Open(filename, false);
                            var wbPart = document.WorkbookPart!;
                            var sheets = wbPart.Workbook.Descendants<Sheet>()!;

                            var sheet = sheets.FirstOrDefault(s => s.Name == "Products");
                            var wsPart = (WorksheetPart)(wbPart!.GetPartById(sheet.Id!));
                            var cells = wsPart.Worksheet.Descendants<Cell>();

                            int row = 2;
                            Cell idCell;
                            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                            do
                            {
                                idCell = cells.FirstOrDefault(
                                    c => c?.CellReference == $"A{row}"
                                )!;

                                if (idCell?.InnerText.Length > 0)
                                {


                                    string titleId = idCell.InnerText;
                                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault()!;


                                    string title = stringTable.SharedStringTable.ElementAt(int.Parse(titleId)).InnerText;



                                    Cell authorCell = cells.FirstOrDefault(
                                        c => c?.CellReference == $"B{row}"
                                    )!;
                                    string authorId = authorCell!.InnerText;
                                    


                                    string author = stringTable.SharedStringTable.ElementAt(int.Parse(authorId)).InnerText;




                                    Cell priceCell = cells.FirstOrDefault(
                                        c => c?.CellReference == $"C{row}"
                                    )!;
                                    string price = priceCell!.InnerText;
                                   


                                    //string price = stringTable.SharedStringTable.ElementAt(int.Parse(priceId)).InnerText;




                                    Cell quantityCell = cells.FirstOrDefault(
                                        c => c?.CellReference == $"D{row}"
                                    )!;
                                    string quantity = quantityCell!.InnerText;


                                    //string quantity = stringTable.SharedStringTable.ElementAt(int.Parse(quantityId)).InnerText;



                                    Cell categoryCell = cells.FirstOrDefault(
                                        c => c?.CellReference == $"E{row}"
                                    )!;
                                    string categoryId = categoryCell!.InnerText;
                                    

                                    string category = stringTable.SharedStringTable.ElementAt(int.Parse(categoryId)).InnerText;


                                    if (!Categories.Any(c => c.CategoryName == category))
                                    {
                                        // Create a new category object with the EditedCategoryName
                                        Category newCategory = new Category() { CategoryName = category };

                                        // Send a POST request to add the new category
                                        using (HttpClient client = new HttpClient())
                                        {
                                            client.BaseAddress = new Uri("https://localhost:7122/");
                                            client.DefaultRequestHeaders.Accept.Clear();
                                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                            await client.PostAsJsonAsync("api/Categories", newCategory);
                                          
                                        }
                                    }

                                  
                                      

                                        string id = "string";
                                    int temp;

                                    var NewBook = new Book()
                                    {
                                        Id = id,
                                        Title = title,
                                        Author = author,
                                        Price = price,
                                        Quantity = int.TryParse(quantity, out temp) ? temp : 0,
                                        Category = Categories.FirstOrDefault(c => c.CategoryName == category),
                                        Cover = Path.Combine(baseDirectory, "Images\\default-thumbnail.jpg")

                                    };
                                    using (HttpClient client = new HttpClient())
                                    {
                                        client.BaseAddress = new Uri("https://localhost:7122/");
                                        client.DefaultRequestHeaders.Accept.Clear();
                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                        var response = await client.PostAsJsonAsync($"api/Books/", NewBook);

                                        if (response.IsSuccessStatusCode)
                                        {
                                            
                                            MessageBox.Show("Successfully imported new product!");
                                            await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                                            SelectedPage = Pages.Count() - 1;
                                        }
                                    }

                                }

                                
                
                                row++;

                            } while (idCell?.InnerText.Length > 0);

                           


                        }
                    });
                }
                return _importProductCommand;
            }
        }

        private ICommand _editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(async (param) =>
                    {
                        if (SelectedBook != null)
                        {
                            // Create a new EditBookViewModel instance
                            var editBookVM = new EditProductViewModel(SelectedBook, Categories);
                            //Create the EditBookWindow nad set DataContext
                            var editBookWindow = new EditProductView();
                            editBookWindow.DataContext = editBookVM;
                            editBookWindow.ShowDialog();
                            await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                            SelectedPage = CurrentPage - 1;
                        }
                    });
                }
                return _editCommand;
            }
        }
        public ProductViewModel()
        {
            CurrentPage = 1;
    
            LoadBooks();
            LoadCategory();
            SelectedPage = 0;
            updatePagingInfo(CurrentPage);
            
            SelectedBook = Books.FirstOrDefault();
            // Wire up the SearchText property
            this.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(SearchText))
                {
                    await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                    SelectedPage = 0;
                }
            };
            this.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(MinPrice))
                {
                    await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                    SelectedPage = 0;
                }
            };

            this.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(MaxPrice))
                {
                    await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                    SelectedPage = 0;
                }
            };
        }

        public async Task LoadBooks(string searchText = null, Category category = null, string minPrice = null, string maxPrice = null, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                CurrentPage = pageNumber;

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
                    var books = JsonConvert.DeserializeObject<List<Book>>(content);
                    //HasNextPage = books.Count == pageSize;
                    //HasPrevPage = CurrentPage > 1;

                    Books.Clear();
                    foreach (var book in books)
                    {
                        Books.Add(book);
                    }

                    TotalItem = Books.Count();
                    AppearBooks = new ObservableCollection<Book>(Books.Skip((CurrentPage - 1) * pageSize).Take(pageSize));
                    
                        updatePagingInfo(CurrentPage);
                    
                       

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



        private ICommand _categorySelectionChangedCommand;
        public ICommand CategorySelectedCommand
        {
            get
            {
                if (_categorySelectionChangedCommand == null)
                {
                    _categorySelectionChangedCommand = new RelayCommand(async (param) =>
                    {
                        await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice, CurrentPage);
                        SelectedPage = 0;
                    });
                }
                return _categorySelectionChangedCommand;
            }
        }



        public bool HasNextPage;
        public bool HasPrevPage;



        public int _currentPage;
        public int CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged(); }
        }

        private int _totalPageCount;
        public int TotalPageCount
        {
            get { return _totalPageCount; }
            set { _totalPageCount = value; OnPropertyChanged(); }
        }


        private int _totalItem;
        public int TotalItem
        {
            get { return _totalItem; }
            set { _totalItem = value; OnPropertyChanged(); }
        }




        private void updatePagingInfo(int currentPage)
        {
            TotalPageCount = TotalItem / 5 +
                   (TotalItem % 5 == 0 ? 0 : 1);

            // Cập nhật ComboBox

            Pages.Clear();
            CurrentPage = currentPage;
            for (int i = 1; i <= TotalPageCount; i++)
            {
                Pages.Add(i.ToString());
            }

         
        }


        public ObservableCollection<string> Pages { get; set; } = new ObservableCollection<string>();

        private int _selectedPage = 0;

        public int SelectedPage
        {
            get { return _selectedPage; }
            set { _selectedPage = value; OnPropertyChanged(); }
        }

        private ICommand _pageSelectionChangedCommand;
        public ICommand PageSelectedCommand
        {
            get
            {
                if ( _pageSelectionChangedCommand == null)
                {
                    _pageSelectionChangedCommand = new RelayCommand(async (param) =>
                    {

                          
                            CurrentPage = SelectedPage+1;
                            AppearBooks = new ObservableCollection<Book>(Books.Skip((CurrentPage-1) * 5).Take(5));
                           // updatePagingInfo();

                            //await LoadBooks(SearchText, SelectedCategory, MinPrice, MaxPrice,CurrentPage);
                    
                    });
                }
                return _pageSelectionChangedCommand;
            }
        }
    }
}
