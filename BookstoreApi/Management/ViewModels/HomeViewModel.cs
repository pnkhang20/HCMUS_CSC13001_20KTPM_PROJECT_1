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
using System.ComponentModel;

namespace Management.ViewModels
{
    class HomeViewModel:ObservableObject
    {
        private const string BookApiUrl = "https://localhost:7122/api/Books";
        private readonly HttpClient httpClient = new HttpClient();
        public ObservableCollection<Book> Books { get; set; } = new ObservableCollection<Book>();

     
        public int _total = 0;
        public int Total
        {
            get { return _total; }
            set
            {
                _total = value;
            }
        }
        public HomeViewModel()
        {
          
            LoadBooks();

        }

        public async Task LoadBooks()
        {
            try
            {
               

                var urlBuilder = new StringBuilder(BookApiUrl);
                var response = await httpClient.GetAsync(urlBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {

                    var content = await response.Content.ReadAsStringAsync();
                    var books = JsonConvert.DeserializeObject<List<Book>>(content);
               
                    Total = books.Count;

                    Books.Clear();
                    foreach (var book in books)
                    {
                        
                        if (book.Quantity >= 0 && book.Quantity < 5)
                        {
                            Books.Add(book);
                           
                            
                        }
                        if (Books.Count == 5) break;
                    }

                   for (int i = 0; i< Books.Count; i++)
                    {
                        for (int j = i+1; j < Books.Count; j++)
                        {
                            if (Books[j].Quantity <= Books[i].Quantity)
                            {
                                (Books[i], Books[j]) = (Books[j], Books[i]);

                            }
                        }
                    }
                   
                }
            }
            catch (Exception ex) { }
        }
    }
}
