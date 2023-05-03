using LiveCharts;
using LiveCharts.Wpf;
using Management.Cores;
using Mangement.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Management.ViewModels
{
    class StatisticViewModel : ObservableObject
    {
        private const string dailyRevenueApi = "https://localhost:7122/api/Orders/revenue/day?";
        private const string monthlyRevenueApi = "https://localhost:7122/api/Orders/revenue/day?";
        private const string yearlyRevenueApi = "https://localhost:7122/api/Orders/revenue/day?";

        private DateTime _fromDate = DateTime.Today.AddDays(-7);
        public DateTime FromDate { get { return _fromDate; } set { _fromDate = value; OnPropertyChanged(); } }

        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate { get { return _toDate; } set { _toDate = value; OnPropertyChanged(); } }

        private string _selectedGroupByOption = "Daily";
        public string SelectedGroupByOption { get { return _selectedGroupByOption; } set { _selectedGroupByOption = value; OnPropertyChanged(); } }

        private List<string> _groupByOptions = new List<string> { "Daily", "Weekly", "Monthly" };
        public List<string> GroupByOptions { get { return _groupByOptions; } set { _groupByOptions = value; OnPropertyChanged(); } } 

        private SeriesCollection _revenueValues;
        public SeriesCollection RevenueValues { get {  return _revenueValues; } set { _revenueValues = value; OnPropertyChanged(); } }

        private Func<decimal, string> _currencyLabelFormatter;
        public Func<decimal, string> CurrencyLabelFormatter { get { return _currencyLabelFormatter; } set { _currencyLabelFormatter = value; OnPropertyChanged(); } }

        private Func<decimal, string> _dateLabelFormatter;
        public Func<decimal, string> DateLabelFormatter { get { return _dateLabelFormatter; } set {_dateLabelFormatter = value;OnPropertyChanged();} }

        private ICommand _generateChartCommand;
        public ICommand GenerateChartCommand
        {
            get
            {
                if (_generateChartCommand == null)
                {
                    _generateChartCommand = new RelayCommand(async (param) =>
                    {
                        string apiUrl = dailyRevenueApi;
                        string queryString = $"fromDate={FromDate.ToString("yyyy-MM-dd")}&toDate={ToDate.ToString("yyyy-MM-dd")}";
                        string apiUrlWithQuery = $"{apiUrl}{queryString}";

                        using (HttpClient client = new HttpClient())
                        {
                            HttpResponseMessage response = await client.GetAsync(apiUrlWithQuery);

                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = await response.Content.ReadAsStringAsync();
                                List<RevenueByDay> revenueData = JsonConvert.DeserializeObject<List<RevenueByDay>>(jsonResponse);

                                // Update RevenueValues property with the fetched data
                                RevenueValues = new SeriesCollection
                            {
                                new ColumnSeries
                                {
                                    Title = "Revenue",
                                    Values = new ChartValues<double>((IEnumerable<double>)revenueData.Select(rd => rd.TotalRevenue)),
                                }
                            };
                            }
                        }
                    });
                }
                return _generateChartCommand;
            }
        }

        public StatisticViewModel()
        {

        }
    }
}