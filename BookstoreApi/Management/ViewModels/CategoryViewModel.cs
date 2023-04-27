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

namespace Management.ViewModels
{
    class CategoryViewModel:ObservableObject
    {
        private const string CategoryApiUrl = "https://localhost:7122/api/Categories";
        private readonly HttpClient httpClient = new HttpClient();


        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set { selectedCategory = value; OnPropertyChanged(); }
        }

        private Category _selectedCategoryCopy;

        public Category SelectedCategoryCopy
        {
            get { return _selectedCategoryCopy; }
            set
            {
                if (_selectedCategoryCopy != value)
                {
                    _selectedCategoryCopy = value;
                    OnPropertyChanged(nameof(SelectedCategoryCopy));
                }
            }
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
                        var msgBoxResult = MessageBox.Show("Are you sure you want to edit this category?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgBoxResult == MessageBoxResult.Yes)
                        {
                            // Check if the cover image is set
                            if (string.IsNullOrEmpty(SelectedCategory.CategoryName))
                            {
                                MessageBox.Show("Please enter a category name.");
                                return;
                            }
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri("https://localhost:7122/");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                
                                HttpResponseMessage response = await client.PutAsJsonAsync($"api/Categories/{SelectedCategory.Id}", SelectedCategory);
                                if (response.IsSuccessStatusCode)
                                {
                                    var newProductVM = new ProductViewModel();
                                    
                                    // Update the original book with the changes made in the SelectedBook object
                                    // Display a success message to the user
                                    MessageBox.Show("Successfully edited the category!");                                    
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
                        var selectedCategory = param as Category;
                        if (selectedCategory == null)
                        {
                            return;
                        }

                        var msgBoxResult = MessageBox.Show("Are you sure you want to delete this category?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (msgBoxResult == MessageBoxResult.Yes)
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri("https://localhost:7122/");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                HttpResponseMessage response = await client.DeleteAsync($"api/Categories/{selectedCategory.Id}");
                                if (response.IsSuccessStatusCode)
                                {
                                    // Display a success message to the user
                                    MessageBox.Show("Successfully deleted the category!");
                                    // Remove the category from the list
                                    LoadCategory();
                                }
                                else
                                {
                                    // Display an error message to the user
                                    MessageBox.Show($"Failed to delete the category! {response.ReasonPhrase}");
                                }
                            }
                        }
                    });
                }
                return _deleteCategoryCommand;
            }
        }


        public CategoryViewModel()
        {
            LoadCategory();            
        }
    }
}
