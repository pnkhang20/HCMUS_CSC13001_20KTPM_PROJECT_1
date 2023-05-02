using LiveCharts;
using Management.Cores;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

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

                }
                return _generateChartCommand;
            }
        }

        public StatisticViewModel()
        {

        }
    }
}