using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts;
using Management.Cores;
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
    class RevenueViewModel : ObservableObject
    {
        private const string dailyRevenueApi = "https://localhost:7122/api/Orders/revenue/day?";
        private const string monthlyRevenueApi = "https://localhost:7122/api/Orders/revenue/month?";
        private const string yearlyRevenueApi = "https://localhost:7122/api/Orders/revenue/year?";

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

        private SeriesCollection _revenueValues;
        public SeriesCollection RevenueValues { get { return _revenueValues; } set { _revenueValues = value; OnPropertyChanged(); } }

        private Func<decimal, string> _currencyLabelFormatter;
        public Func<decimal, string> CurrencyLabelFormatter { get { return _currencyLabelFormatter; } set { _currencyLabelFormatter = value; OnPropertyChanged(); } }

        private List<string> _dateLabelFormatter;
        public List<string> DateLabelFormatter { get { return _dateLabelFormatter; } set { _dateLabelFormatter = value; OnPropertyChanged(); } }
        public Double maxRevenue = 0;


        private ICommand _generateChartCommand;
        public ICommand GenerateChartCommand
        {
            get
            {
                if (_generateChartCommand == null)
                {
                    _generateChartCommand = new RelayCommand(async (param) =>
                    {
                        string apiUrl = string.Empty;
                        string queryString = $"fromDate={FromDate.AddDays(-1).ToString("yyyy-MM-dd")}&toDate={ToDate.AddDays(1).ToString("yyyy-MM-dd")}";

                        switch (SelectedGroupByOption)
                        {
                            case "Daily":
                                apiUrl = dailyRevenueApi;
                                break;
                            case "Monthly":
                                apiUrl = monthlyRevenueApi;
                                break;
                            case "Yearly":
                                apiUrl = yearlyRevenueApi;
                                break;
                        }

                        string apiUrlWithQuery = $"{apiUrl}{queryString}";

                        using (HttpClient client = new HttpClient())
                        {
                            HttpResponseMessage response = await client.GetAsync(apiUrlWithQuery);

                            if (response.IsSuccessStatusCode)
                            {
                                string jsonResponse = await response.Content.ReadAsStringAsync();
                                List<dynamic> revenueData = JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse);

                                // Update RevenueValues property with the fetched data
                                RevenueValues = new SeriesCollection();
                                DateLabelFormatter = new List<string>();
                                switch (SelectedGroupByOption)
                                {
                                    case "Daily":
                                        foreach (var revenue in revenueData)
                                        {

                                            RevenueByDay revenueByDay = JsonConvert.DeserializeObject<RevenueByDay>(revenue.ToString());
                                            List<string> labels = new List<string> { revenueByDay.Date.ToString("yy-M-d") };
                                            // Create a new ColumnSeries for each day
                                            maxRevenue = 100;
                                            ColumnSeries daySeries = new ColumnSeries
                                            {
                                                Title = revenueByDay.Date.ToString("yy-MM-d"),
                                                Values = new ChartValues<double> { revenueByDay.TotalRevenue },
                                                LabelPoint = point => $"{point.Y:C}"
                                            };
                                            //DateLabelFormatter.Add(revenueByDay.Date.ToString("yy-M-d"));
                                            RevenueValues.Add(daySeries);
                                        }
                                        break;
                                    case "Monthly":

                                        foreach (var revenue in revenueData)
                                        {
                                            RevenueByMonth revenueByMonth = JsonConvert.DeserializeObject<RevenueByMonth>(revenue.ToString());
                                            maxRevenue = 500;
                                            // Create a new ColumnSeries for each month
                                            ColumnSeries monthSeries = new ColumnSeries
                                            {
                                                Title = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(revenueByMonth.Month)} {revenueByMonth.Year}",
                                                Values = new ChartValues<double> { (double)revenueByMonth.TotalRevenue },
                                                LabelPoint = point => $"{point.Y:C}"

                                            };
                                            RevenueValues.Add(monthSeries);
                                        }
                                        break;
                                    case "Yearly":
                                        foreach (var revenue in revenueData)
                                        {
                                            RevenueByYear revenueByYear = JsonConvert.DeserializeObject<RevenueByYear>(revenue.ToString());
                                            maxRevenue = 1000;
                                            // Create a new ColumnSeries for each year
                                            ColumnSeries yearSeries = new ColumnSeries
                                            {
                                                Title = revenueByYear.Year.ToString(),
                                                Values = new ChartValues<double> { (double)revenueByYear.TotalRevenue },
                                                LabelPoint = point => $"{point.Y:C}"
                                            };
                                            RevenueValues.Add(yearSeries);
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
        public RevenueViewModel()
        {
            // Set default selected group by option
            SelectedGroupByOption = "Daily";

            // Load chart
            GenerateChartCommand.Execute(null);
        }
    }
}