using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using test.Commands;

namespace test
{
    public class BookViewModel : INotifyPropertyChanged
    {
        private const string ApiUrl = "https://localhost:7122/api/books";
        //private const int UpdateInterval = 5000; // 5 seconds

        private readonly HttpClient httpClient = new HttpClient();
        private readonly Timer updateTimer = new Timer();

        public ObservableCollection<Book> Books { get; } = new ObservableCollection<Book>();

        private Book selectedBook;
        public Book SelectedBook
        {
            get { return selectedBook; }
            set { selectedBook = value; OnPropertyChanged(); }
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
                            var messageBoxResult = MessageBox.Show($"Are you sure you want to delete {SelectedBook.BookName}?", "Delete Book", MessageBoxButton.YesNo);
                            if (messageBoxResult == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    HttpResponseMessage response = await httpClient.DeleteAsync($"{ApiUrl}/{SelectedBook.Id}");

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

        public BookViewModel()
        {
            LoadBooks();
            SelectedBook = Books.FirstOrDefault();
            //updateTimer.Interval = UpdateInterval;
            //updateTimer.Elapsed += async (sender, args) => await LoadBooks();
            //updateTimer.Start();
        }

        private async Task LoadBooks()
        {
            try
            {
                var response = await httpClient.GetAsync(ApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonConvert.DeserializeObject<List<Book>>(content);

                    foreach (var book in books)
                    {
                        var existingBook = Books.FirstOrDefault(b => b.Id == book.Id);
                        if (existingBook == null)
                        {
                            Books.Add(book);
                        }
                        else
                        {
                            int index = Books.IndexOf(existingBook);
                            Books[index] = book;
                        }
                    }

                    var removedBooks = Books.Where(b => !books.Any(bb => bb.Id == b.Id)).ToList();
                    foreach (var book in removedBooks)
                    {
                        Books.Remove(book);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
