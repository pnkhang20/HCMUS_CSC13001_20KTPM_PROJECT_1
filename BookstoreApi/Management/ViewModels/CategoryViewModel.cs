using Management.Cores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Management.Models;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Windows;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Management.Views;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using Azure;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Management.ViewModels
{
    class CategoryViewModel:ObservableObject
    {
        private ProductView _productView;
        private const string CategoryApiUrl = "https://localhost:7122/api/Categories";
        private readonly HttpClient httpClient = new HttpClient();

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get { return _selectedCategory; }
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        private string _editedCategoryName;
        public string EditedCategoryName
        {
            get { return _editedCategoryName; }
            set { _editedCategoryName = value; OnPropertyChanged(); }
        }

        private string _newCategoryText;
        public string NewCategoryText
        {
            get { return _newCategoryText; }
            set { _newCategoryText = value; OnPropertyChanged(); }
        }
        private async Task LoadCategory()
        {
            try
            {
                var response = await httpClient.GetAsync(CategoryApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var categories = JsonConvert.DeserializeObject<List<Category>>(content);

                    // Clear the existing categories list
                    Categories.Clear();

                    // Add the updated categories from the server
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }


        private ICommand _editCategoryCommand;
        public ICommand EditCategoryCommand
        {
            get
            {
                if (_editCategoryCommand == null)
                {
                    _editCategoryCommand = new RelayCommand(async (param) =>
                    {
                        // Check if the SelectedCategory is null before proceeding
                        if (SelectedCategory == null)
                        {
                            MessageBox.Show("Please select a category to edit.");
                            return;
                        }
                        var msgBoxResult = MessageBox.Show("Are you sure you want to edit this category?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgBoxResult == MessageBoxResult.Yes)
                        {                            
                            if (string.IsNullOrEmpty(EditedCategoryName))
                            {
                                MessageBox.Show("Please enter a category name.");
                                return;
                            }
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri("https://localhost:7122/");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                SelectedCategory.CategoryName = EditedCategoryName;
                                HttpResponseMessage response = await client.PutAsJsonAsync($"api/Categories/{SelectedCategory.Id}", SelectedCategory);
                                if (response.IsSuccessStatusCode)
                                {
                                    var newProductVM = new ProductViewModel();
                                    
                                    // Update the original book with the changes made in the SelectedBook object
                                    // Display a success message to the user
                                    MessageBox.Show("Successfully edited the category!");
                                    EditedCategoryName = string.Empty;
                                    var mainWindow = Application.Current.MainWindow;
                                    var mainViewModel = (MainViewModel)mainWindow.DataContext;
                                    var productView = (ProductViewModel)mainViewModel.ProductVM;
                                    productView.LoadCategory();
                                    productView.LoadBooks();    
                                    await LoadCategory();
                                }
                                else
                                {
                                    // Display an error message to the user
                                    MessageBox.Show($"Failed to add new product! {response.ReasonPhrase}");
                                }
                            }
                        }
                    });
                }
                return _editCategoryCommand;
            }
        
        }

        private ICommand _deleteCategoryCommand;
        public ICommand DeleteCategoryCommand
        {
            get
            {
                if (_deleteCategoryCommand == null)
                {
                    _deleteCategoryCommand = new RelayCommand(async (param) =>
                    {
                        if (SelectedCategory != null)
                        {
                            var msgBoxResult = MessageBox.Show("Are you sure you want to delete this category?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (msgBoxResult == MessageBoxResult.Yes)
                            {
                                using (HttpClient client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://localhost:7122/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    HttpResponseMessage response = await client.DeleteAsync($"api/Categories/{SelectedCategory.Id}");
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // Display a success message to the user
                                        MessageBox.Show("Successfully deleted the category!");
                                        // Remove the category from the list
                                        var mainWindow = Application.Current.MainWindow;
                                        var mainViewModel = (MainViewModel)mainWindow.DataContext;
                                        var productView = (ProductViewModel)mainViewModel.ProductVM;
                                        productView.LoadCategory();
                                        productView.LoadBooks();
                                        await LoadCategory();
                                    }
                                    else
                                    {
                                        // Display an error message to the user
                                        MessageBox.Show($"Failed to delete the category! {response.ReasonPhrase}");
                                    }
                                }
                            }
                        }
                    });
                }
                return _deleteCategoryCommand;
            }
        }

        private ICommand _addNewCategoryCommand;
        public ICommand AddNewCategoryCommand
        {
            get
            {
                if (_addNewCategoryCommand == null)
                {
                    _addNewCategoryCommand = new RelayCommand(async (param) =>
                    {
                  
                        // Validate that the EditedCategoryName is not empty
                        if (string.IsNullOrEmpty(NewCategoryText))
                        {
                            MessageBox.Show("Please enter a category name!");
                            return;
                        }
                        // Check if the category already exists in the list
                        if (Categories.Any(c => c.CategoryName == NewCategoryText))
                        {
                            MessageBox.Show("This category already exists!");
                            return;
                        }

                        var msgBoxResult = MessageBox.Show("Are you sure you want to add this category?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgBoxResult == MessageBoxResult.Yes)
                        {
                            // Create a new category object with the EditedCategoryName
                            Category newCategory = new Category() { CategoryName = NewCategoryText };

                            // Send a POST request to add the new category
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri("https://localhost:7122/");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                HttpResponseMessage response = await client.PostAsJsonAsync("api/Categories", newCategory);
                                if (response.IsSuccessStatusCode)
                                {
                                    // Display a success message to the user
                                    MessageBox.Show("Successfully added a new category!");
                                    // Clear the EditedCategoryName property
                                    NewCategoryText = string.Empty;
                                    // Reload the categories list
                                    var mainWindow = Application.Current.MainWindow;
                                    var mainViewModel = (MainViewModel)mainWindow.DataContext;
                                    var productView = (ProductViewModel)mainViewModel.ProductVM;
                                    productView.LoadCategory();
                                    productView.LoadBooks();                                    
                                    await LoadCategory();
                                }
                                else
                                {
                                    // Display an error message to the user
                                    MessageBox.Show($"Failed to add a new category! {response.ReasonPhrase}");
                                }
                            }
                        }
                    });
                }               
                return _addNewCategoryCommand;
            }
        }


        private ICommand _importNewCategoryCommand;
        public ICommand ImportNewCategoryCommand
        {
            get
            {
                if (_importNewCategoryCommand == null)
                {
                    _importNewCategoryCommand = new RelayCommand(async (param) =>
                    {

                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                        if (openFileDialog.ShowDialog() == true)
                        {
                            string filename = openFileDialog.FileName;
                            var document = SpreadsheetDocument.Open(filename, false);
                            var wbPart = document.WorkbookPart!;
                            var sheets = wbPart.Workbook.Descendants<Sheet>()!;

                            var sheet = sheets.FirstOrDefault(s => s.Name == "Categories");
                            var wsPart = (WorksheetPart)(wbPart!.GetPartById(sheet.Id!));
                            var cells = wsPart.Worksheet.Descendants<Cell>();

                            int row = 2;
                            Cell categoryCell;
                            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                            do
                            {
                                categoryCell = cells.FirstOrDefault(
                                    c => c?.CellReference == $"A{row}"
                                )!;

                                if (categoryCell?.InnerText.Length > 0)
                                {



                                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault()!;

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

                                            var response = await client.PostAsJsonAsync("api/Categories", newCategory);
                                            if (response.IsSuccessStatusCode)
                                            {

                                                MessageBox.Show("Successfully added a new category!");
                                                // Clear the EditedCategoryName property
                                                NewCategoryText = string.Empty;
                                                // Reload the categories list
                                                var mainWindow = Application.Current.MainWindow;
                                                var mainViewModel = (MainViewModel)mainWindow.DataContext;
                                                var productView = (ProductViewModel)mainViewModel.ProductVM;
                                                productView.LoadCategory();
                                                productView.LoadBooks();
                                                await LoadCategory();
                                            }
                                            else
                                            {
                                                // Display an error message to the user
                                                MessageBox.Show($"Failed to add a new category! {response.ReasonPhrase}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Display an error message to the user
                                        MessageBox.Show("This category already exists!");
                                    }

                                    
                                }

                                row++;

                            } while (categoryCell?.InnerText.Length > 0);

                        }
                    });
                }
                return _importNewCategoryCommand;
            }
        }
        
        public CategoryViewModel()
        {            
            LoadCategory();            
        }
    }
}
