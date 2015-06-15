using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using nexMuni.DataModels;
using nexMuni.Helpers;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;

namespace nexMuni.ViewModels
{
    public class StopDetailViewModel : INotifyPropertyChanged
    {
        private Stop _selectedStop;
        private string _noTimes;

        public ObservableCollection<Route> Routes { get; private set; }
        public ObservableCollection<Alert> Alerts { get; private set; }
        public Stop SelectedStop {
            get
            {
                return _selectedStop;
            }
            set
            {
                _selectedStop = value;
                NotifyPropertyChanged("SelectedStop");
            }
        }
        public string NoTimesText {
            get
            {
                return _noTimes;
            }
            set
            {
                _noTimes = value;
                NotifyPropertyChanged("NoTimesText");
            }
        }

        private DispatcherTimer refreshTimer;

        private StopDetailViewModel() { }

        public StopDetailViewModel(Stop stop)
        {
            Routes = new ObservableCollection<Route>();
            Alerts = new ObservableCollection<Alert>();
            SelectedStop = stop;
            refreshTimer = new DispatcherTimer();
            refreshTimer.Tick += TimerDue;
            refreshTimer.Interval = new System.TimeSpan(0, 0, 10);
        }

        public async Task LoadTimes()
        {

#if WINDOWS_PHONE_APP
            StatusBar systemTray = StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Getting Arrival Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif

            //string[] splitRoutes = SelectedStop.Routes.Split(',');
            //splitRoutes[0] = " " + splitRoutes[0];

            var xmlDoc = await WebHelper.GetMulitPredictions(SelectedStop.StopTags);
            List<Route> routeList = await PredictionHelper.ParsePredictionsAsync(xmlDoc);

            if (routeList.Count == 0) NoTimesText = "No busses at this time";
            else
            {
                foreach (Route r in routeList)
                {
                    NoTimesText = "";
                    Routes.Add(r);
                }
            }

            //Get alerts TODO:Move to seperate method
            List<Alert> alertList = await PredictionHelper.ParseAlerts(xmlDoc);
            foreach(Alert a in alertList)
            {
                Alerts.Add(a);
            }

            if(!refreshTimer.IsEnabled) refreshTimer.Start();
            //refreshTimer.Change(30000, Timeout.Infinite);

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMUNI";
#endif
        }

        public async Task RefreshTimes()
        {
            Routes.Clear();
            var xmlDoc = await WebHelper.GetMulitPredictions(SelectedStop.StopTags);
            List<Route> routeList = await PredictionHelper.ParsePredictionsAsync(xmlDoc);

            foreach (Route r in routeList)
            {
                Routes.Add(r);
            }

            //refreshTimer.Change(30000, Timeout.Infinite);
        }

        public async Task AddFavoriteAsync()
        {
            await DatabaseHelper.AddFavoriteAsync(SelectedStop);
        }

        public async Task RemoveFavoriteAsync()
        {
            await DatabaseHelper.RemoveFavoriteAsync(SelectedStop);
        }

        public bool IsFavorite()
        {
            return DatabaseHelper.FavoritesList.Any(f => f.Name == SelectedStop.StopName);
        }

        public void StopTimer()
        {
            refreshTimer.Stop();
        }

        private async void TimerDue(object sender, object e)
        {
            await RefreshTimes();
        }

        #region INotify Methods
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
