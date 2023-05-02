using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Management.Cores;
using Management.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

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
                    return new ObservableCollection<Category>(_parsedCategories.Where(c => c.CategoryName != "All Category"));
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

        private string _coverImg;
        public string CoverImg
        {
            get { return _coverImg; }
            set { _coverImg = value; OnPropertyChanged(); }
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

                            if (SelectedCategory == null)
                            {
                                MessageBox.Show("Category cannot be empty.");
                                return;
                            }

                            if (SelectedBook.Price == null)
                            {
                                MessageBox.Show("Price cannot be empty.");
                                return;
                            }

                            if (SelectedBook.Title == null)
                            {
                                MessageBox.Show("Title cannot be empty.");
                                return;
                            }

                            if (SelectedBook.Author == null)
                            {
                                MessageBox.Show("Author cannot be empty.");
                                return;
                            }

                            // Confirm with the user that they want to save changes
                            var result = MessageBox.Show("Are you sure you want to save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                // Make a PUT request to update the book

                                if (string.IsNullOrEmpty(CoverImg))
                                {
                                    //MessageBox.Show("Please select a cover image.");
                                    //return;

                                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                                    CoverImg = Path.Combine(baseDirectory, "Images\\default-thumbnail.jpg");

                                }
                                // Make a POST request to update the book
                               

                                    using (HttpClient client = new HttpClient())
                                {
                                    client.BaseAddress = new Uri("https://localhost:7122/");
                                    client.DefaultRequestHeaders.Accept.Clear();
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    string imageUrl = await UploadImage(CoverImg);

                                    // Set the Cover property of the new book to the URL of the uploaded blob
                                    SelectedBook.Cover = imageUrl;

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


        public EditProductViewModel(Book selectedBook, ObservableCollection<Category> categories)
        {
            SelectedBook = selectedBook.Clone();
            Categories = categories;
            CoverImg = SelectedBook.Cover;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedBook.Category.Id);
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
                            // Upload service: imagehost@wpfproject-385006.iam.gserviceaccount.com
                            if (openFileDialog.ShowDialog() == true)
                            {
                                string imagePath = openFileDialog.FileName;
                                CoverImg = imagePath;
                            }
                        }
                    );
                }

                return _selectCoverCommand;
            }
        }

        private async Task<string> UploadImage(string imagePath)
        {
            // Get the connection string and container name from app settings or a configuration file
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=kpwpf;AccountKey=AUW9ZL+TCxx0aj6GF5/DC3wRz1oHUgVMJuxbRfrJk+JFEec9xxF9mrK5wrtmXA0MpHLrV0xm9wyF+AStoAFX0w==;EndpointSuffix=core.windows.net";
            string containerName = "covers";

            // Create the blob service client and get a reference to the container
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it does not exist
            if (!containerClient.Exists())
            {
                await containerClient.CreateAsync();
            }

            // Create a reference to the blob and upload the image
            BlobClient blobClient = containerClient.GetBlobClient(Path.GetFileName(imagePath));
            using (FileStream stream = File.OpenRead(imagePath))
            {
                var blobUploadOptions = new BlobUploadOptions()
                {
                    HttpHeaders = new BlobHttpHeaders()
                    {
                        ContentType = "image/jpeg"
                    }
                };
                await blobClient.UploadAsync(stream, blobUploadOptions);
            }
            // Return the URL of the uploaded blob
            return blobClient.Uri.ToString();
        }

    }
}
