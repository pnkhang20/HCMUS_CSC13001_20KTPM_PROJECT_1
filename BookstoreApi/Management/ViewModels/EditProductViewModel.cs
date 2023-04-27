using Management.Cores;
using Management.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    class EditProductViewModel : ObservableObject
    {
        private Book selectedBook;
        public Book SelectedBook
        {
            get { return selectedBook; }
            set
            {
                selectedBook = value;
                OnPropertyChanged(nameof(SelectedBook));
            }
        }
        private ObservableCollection<Category> _parsedCategories;
        public ObservableCollection<Category> Categories
        {
            get { return _parsedCategories; }
            set
            {
                _parsedCategories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }
        // Filter out the AllCategory object from the Categories collection
        public ObservableCollection<Category> FilteredCategories
        {
            get
            {
                if (_parsedCategories == null)
                {
                    return null;
                }
                else
                {
                    return new ObservableCollection<Category>(_parsedCategories.Where(c => c.Id != null));
                }
            }
        }

        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }


        private ICommand _saveChangesCommand;
        public ICommand SaveChangesCommand
        {
            get
            {
                if (_saveChangesCommand == null)
                {
                    _saveChangesCommand = new RelayCommand(
                        async (param) =>
                        {
                            // Confirm with the user that they want to save changes
                            var result = MessageBox.Show("Are you sure you want to save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                // Make a PUT request to update the book
                                using (HttpClient client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://localhost:7122/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    SelectedBook.Category = SelectedCategory;
                                    // Use the SelectedBook object (which is a clone of the original book) to make the PUT request
                                    HttpResponseMessage response = await client.PutAsJsonAsync($"api/Books/{SelectedBook.Id}", SelectedBook);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        // Update the original book with the changes made in the SelectedBook object
                                        // Display a success message to the user
                                        MessageBox.Show("Changes saved successfully!");
                                        // Close the current window and update the parent view
                                    
                                        Window parentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                                        parentWindow?.Close();                                    
                                    }
                                    else
                                    {
                                        // Display an error message to the user
                                        MessageBox.Show($"Failed to save changes. {response.ReasonPhrase}");
                                    }
                                }
                            }
                        },
                        (param) =>
                        {
                            // Enable the command only if the SelectedBook object is not null and valid
                            return SelectedBook != null;
                        }
                    );
                }

                return _saveChangesCommand;
            }
        }
        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(
                        (param) =>
                        {
                            var result = MessageBox.Show("Are you sure you want to discard changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                // Close the window
                                Window parentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                                parentWindow?.Close();
                            }
                        }
                    );
                }

                return _cancelCommand;
            }
        }


        public EditProductViewModel(Book selectedBook, ObservableCollection<Category> categories )
        {
            SelectedBook = selectedBook.Clone();
            Categories = categories;                    
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedBook.Category.Id);
        }

    }
}
