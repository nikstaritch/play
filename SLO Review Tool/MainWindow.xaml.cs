using SloReviewTool;
using SloReviewTool.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace SloReviewTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SloQueryManager queryManager_;
        public string PlaceholderText { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            queryManager_ = new SloQueryManager();

        }

        /// <summary>
        /// Executes Kusto query to obtain service data that includes manual review data.
        /// </summary>
        /// <param name="criteria">Service ID, service name or blank string to return the complete list of services.</param>
        /// <returns>
        /// A list of <see cref="SloRecord"/> objects with a potential <see cref="SloValidationException"/> list that is
        /// returned as a tuple.
        /// </returns>
        public Task<Tuple<List<SloRecord>, List<SloValidationException>>> ExecuteQueryAsync(string criteria)
        {
            string kustoQuery;
            if(string.IsNullOrWhiteSpace(criteria))
            {
                kustoQuery = "GetSloJsonActionItemReport() ";
            }
            else if (GuidEx.IsGuid(criteria))
            {
                kustoQuery = $"GetSloJsonActionItemReport() | where ServiceId == '{criteria.Trim()}' ";
            }
            else if (!GuidEx.IsGuid(criteria))
            {
                kustoQuery = $"GetSloJsonActionItemReport() | where ServiceName contains '{criteria.Trim()}' ";
            }
            else
            {
                kustoQuery = criteria;
            }

            return Task.Run(() => {
                return queryManager_.ExecuteQuery(kustoQuery);
            });

        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            QueryButton.IsEnabled = false;
            QueryStatus.Content = "Executing Query...";
            this.ResultsDataGrid.ItemsSource = null;
            this.ErrorListView.ItemsSource = null;

            try
            {

                var results = await ExecuteQueryAsync(QueryTextBox.Text);

                QueryStatus.Content = $"Query returned {results.Item1.Count + results.Item2.Count} record(s), {results.Item2.Count} failed to parse";
                ListCollectionView sloRecords = new ListCollectionView(results.Item1);
                sloRecords.GroupDescriptions.Add(new PropertyGroupDescription("ServiceGroupName"));
                ResultsDataGrid.ItemsSource = sloRecords;
                ErrorListView.ItemsSource = results.Item2;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

            QueryButton.IsEnabled = true;
        }

        private void ResultsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ResultsDataGrid.CurrentItem == null) return;
            var sloView = new SloView(ResultsDataGrid.CurrentItem as SloRecord, ResultsDataGrid);
            sloView.Show();
        }

        private void ResultsDataGrid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ResultsDataGrid.CurrentItem == null) return;
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt))
            {
                var url = @"https://servicetree.msftcloudes.com/main.html#/ServiceModel/Home/" + (ResultsDataGrid.CurrentItem as SloRecord).ServiceId;
                System.Diagnostics.Process.Start(url);
            }
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            var sloValidator = new SloValidatorView();
            sloValidator.Show();
        }

        public static class GuidEx
        {
            public static bool IsGuid(string value)
            {
                Guid x;
                return Guid.TryParse(value, out x);
            }
        }
    }
}
