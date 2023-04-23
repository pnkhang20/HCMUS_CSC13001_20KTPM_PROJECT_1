using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace test
{

        public class BookViewModel : BaseViewModel
        {
            private const string ApiUrl = "https://localhost:7122/api/books";

            private readonly HttpClient httpClient = new HttpClient();

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
        }
    }
