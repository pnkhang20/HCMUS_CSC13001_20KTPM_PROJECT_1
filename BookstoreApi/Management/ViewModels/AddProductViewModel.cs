using Management.Cores;
using Management.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Net.Http.Json;

namespace Management.ViewModels
{
    class AddProductViewModel : ObservableObject
    {
        private Book _newBook;
        public Book NewBook
        {
            get { return _newBook; }
            set { _newBook = value; OnPropertyChanged(); }
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
        public AddProductViewModel(ObservableCollection<Category> categories)
        {
            Categories = new ObservableCollection<Category>(categories.Skip(1));
            NewBook = new Book();
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
                        //// Check if the cover image is set
                        //if (string.IsNullOrEmpty(NewBook.Cover))
                        //{
                        //    MessageBox.Show("Please select a cover image.");
                        //    return;
                        //}

                        // Confirm with the user that they want to save changes
                        var result = MessageBox.Show("Are you sure you want to save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Make a POST request to update the book
                            using (HttpClient client = new HttpClient())
                            {
                                client.BaseAddress = new Uri("https://localhost:7122/");
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                // Use the SelectedBook object (which is a clone of the original book) to make the PUT request
                                Book toParse = NewBook;
                                HttpResponseMessage response = await client.PostAsJsonAsync($"api/Books/", NewBook);

                                if (response.IsSuccessStatusCode)
                                {
                                    // Update the original book with the changes made in the SelectedBook object
                                    // Display a success message to the user
                                    MessageBox.Show("Successfully added new product!");
                                    // Close the current window and update the parent view

                                    Window parentWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                                    parentWindow?.Close();
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
                return _addProductCommand;
            }

        }

        private string _coverImg;
        public string CoverImg
        {
            get { return _coverImg; } 
            set { _coverImg = value; OnPropertyChanged(); }
        }
        private ICommand _selectCoverCommand;
        public ICommand SelectCoverCommand
        {
            get
            {
                if (_selectCoverCommand == null)
                {
                    _selectCoverCommand = new RelayCommand(
                        async (param) =>
                        {
                            OpenFileDialog openFileDialog = new OpenFileDialog();
                            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
                            // Upload service: imguploadwpf@quiet-dryad-385006.iam.gserviceaccount.com
                            if (openFileDialog.ShowDialog() == true)
                            {
                                string imagePath = openFileDialog.FileName;
                                CoverImg = imagePath;
                                
                                // Upload the image to a hosting service and get the URL
                                //string imageUrl = await UploadImage(imagePath);
                                //NewBook.Cover = imageUrl;
                            }
                        }
                    );
                }

                return _selectCoverCommand;
            }
        }

       


    }
}
