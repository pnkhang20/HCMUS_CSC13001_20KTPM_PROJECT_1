using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Management.Cores;
using Management.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Net.Http.Json;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using System.IO;
using System.Windows.Media.Imaging;

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

        public AddProductViewModel(ObservableCollection<Category> categories)
        {
            Categories = categories;
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

                        // Confirm with the user that they want to save changes
                        var result = MessageBox.Show("Are you sure you want to save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            // Check if the cover image is set
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

                                // Upload the cover image to Azure Blob Storage and get the URL
                                string imageUrl = await UploadImage(CoverImg);

                                // Set the Cover property of the new book to the URL of the uploaded blob
                                NewBook.Cover = imageUrl;
                                NewBook.Id = "string";
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
