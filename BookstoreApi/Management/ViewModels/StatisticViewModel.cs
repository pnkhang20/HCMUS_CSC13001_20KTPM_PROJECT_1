using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts;
using Management.Cores;
using Management.Models;
using Mangement.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Management.ViewModels
{
    class StatisticViewModel : ObservableObject
    {
        private const string totalSoldApi = "https://localhost:7122/api/Orders/sold?";

        private DateTime _fromDate = DateTime.Today.AddDays(-7);
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                switch (SelectedGroupByOption)
                {
                    case "Daily":
                        if (value > ToDate)
                        {
                            ToDate = value;
                        }
                        else if ((ToDate - value).TotalDays > 6)
                        {
                            ToDate = value.AddDays(6);
                        }
                        break;
                    case "Monthly":
                        if (value > ToDate)
                        {
                            ToDate = value;
                        }
                        else if ((ToDate - value).TotalDays > 180)
                        {
                            ToDate = value.AddMonths(6);
                        }
                        break;
                    case "Yearly":
                        if (value > ToDate)
                        {
                            ToDate = value;
                        }
                        else if ((ToDate - value).TotalDays > 1825)
                        {
                            ToDate = value.AddYears(5);
                        }
                        break;
                    default:
                        break;
                }
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                switch (SelectedGroupByOption)
                {
                    case "Daily":
                        if (value < FromDate)
                        {
                            FromDate = value;
                        }
                        else if ((value - FromDate).TotalDays > 6)
                        {
                            FromDate = value.AddDays(-6);
                        }
                        break;
                    case "Monthly":
                        if (value < FromDate)
                        {
                            FromDate = value;
                        }
                        else if ((value - FromDate).TotalDays > 180)
                        {
                            FromDate = value.AddMonths(-6);
                        }
                        break;
                    case "Yearly":
                        if (value < FromDate)
                        {
                            FromDate = value;
                        }
                        else if ((value - FromDate).TotalDays > 1825)
                        {
                            FromDate = value.AddYears(-5);
                        }
                        break;
                    default:
                        break;
                }
                _toDate = value;
                OnPropertyChanged();
            }
        }

        private string _selectedGroupByOption = "Daily";
        public string SelectedGroupByOption
        {
            get { return _selectedGroupByOption; }
            set
            {
                _selectedGroupByOption = value;
                OnPropertyChanged(); 
            }
        }

        private List<string> _groupByOptions = new List<string> { "Daily", "Monthly", "Yearly" };
        public List<string> GroupByOptions { get { return _groupByOptions; } set { _groupByOptions = value; OnPropertyChanged(); } } 

        private SeriesCollection _statisticValues;
        public SeriesCollection StatisticValues { get {  return _statisticValues; } set { _statisticValues = value; OnPropertyChanged(); } }

        private Func<int, string> _currencyLabelFormatter;
        public Func<int, string> CurrencyLabelFormatter { get { return _currencyLabelFormatter; } set { _currencyLabelFormatter = value; OnPropertyChanged(); } }

        private List<string> _dateLabelFormatter;
        public List<string> DateLabelFormatter { get { return _dateLabelFormatter; } set {_dateLabelFormatter = value;OnPropertyChanged();} }
        public int maxBook = 0;


        private ICommand _generateChartCommand;
        public ICommand GenerateChartCommand
        {
            get
            {
                if (_generateChartCommand == null)
                {
                    _generateChartCommand = new RelayCommand(async (param) =>
                    {
                        string apiUrl = totalSoldApi;
                        string queryString = $"fromDate={FromDate.ToString("yyyy-MM-dd")}&toDate={ToDate.ToString("yyyy-MM-dd")}";

                        string apiUrlWithQuery = $"{apiUrl}{queryString}";

                        using (HttpClient client = new HttpClient())
                        {
                            HttpResponseMessage response = await client.GetAsync(apiUrlWithQuery);

                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = await response.Content.ReadAsStringAsync();
                                List<dynamic> bookSoldList = JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse);

                                // Update RevenueValues property with the fetched data
                                StatisticValues = new SeriesCollection();
                                DateLabelFormatter = new List<string>();
                                switch (SelectedGroupByOption)
                                {
                                    case "Daily":
                                        foreach (var bookSold in bookSoldList)
                                        {
 
                                            BookSold bookSoldByDay = JsonConvert.DeserializeObject<BookSold>(bookSold.ToString());
                                            List<string> labels = new List<string> { bookSoldByDay.BookName };
                                            // Create a new ColumnSeries for each day
                                            maxBook = 10;
                                            ColumnSeries daySeries = new ColumnSeries
                                            {
                                                Title = bookSoldByDay.BookName,
                                                Values = new ChartValues<int> { bookSoldByDay.Sold},                                                
                                            };
                                            //DateLabelFormatter.Add(revenueByDay.Date.ToString("yy-M-d"));
                                            StatisticValues.Add(daySeries);
                                        }
                                        break;
                                    case "Monthly":

                                        foreach (var bookSold in bookSoldList)
                                        {
                                            BookSold bookSoldByMonth = JsonConvert.DeserializeObject<BookSold>(bookSold.ToString());
                                            List<string> labels = new List<string> { bookSoldByMonth.BookName };
                                            maxBook = 30;
                                            // Create a new ColumnSeries for each month
                                            ColumnSeries monthSeries = new ColumnSeries
                                            {
                                                Title = bookSoldByMonth.BookName,
                                                Values = new ChartValues<int> { bookSoldByMonth.Sold },                                                
                                            };
                                            StatisticValues.Add(monthSeries);
                                        }
                                        break;
                                    case "Yearly":
                                        foreach (var bookSold in bookSoldList)
                                        {
                                            BookSold bookSoldByYear = JsonConvert.DeserializeObject<BookSold>(bookSold.ToString());
                                            maxBook = 50;
                                            // Create a new ColumnSeries for each year
                                            ColumnSeries yearSeries = new ColumnSeries
                                            {
                                                Title = bookSoldByYear.BookName,
                                                Values = new ChartValues<int> { bookSoldByYear.Sold }
                                            };
                                            StatisticValues.Add(yearSeries);
                                        }
                                        break;
                                }                                
                            }
                        }

                    });
                }

                return _generateChartCommand;
            }
        }
        public void ReloadChart()
        {
            GenerateChartCommand.Execute(null);
        }

        public StatisticViewModel()
        {
            // Set default selected group by option
            SelectedGroupByOption = "Daily";

            // Load chart
            GenerateChartCommand.Execute(null);
        }
    }
}