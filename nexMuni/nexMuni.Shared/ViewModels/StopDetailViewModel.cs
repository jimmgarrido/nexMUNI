using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using nexMuni.DataModels;
using nexMuni.Helpers;
using System.ComponentModel;
using System.Linq;

namespace nexMuni.ViewModels
{
    public class StopDetailViewModel : INotifyPropertyChanged
    {
        private Stop _selectedStop;
        private string _noTimes;

        public ObservableCollection<Route> Routes { get; private set; }
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

        private Task _initialize;
        private string url;

        private StopDetailViewModel() { }

        public StopDetailViewModel(Stop stop)
        {
            Routes = new ObservableCollection<Route>();
            SelectedStop = stop;
        }

        public async Task LoadTimes()
        {

#if WINDOWS_PHONE_APP
            StatusBar systemTray = StatusBar.GetForCurrentView();
            systemTray.ProgressIndicator.Text = "Getting Arrival Times";
            systemTray.ProgressIndicator.ProgressValue = null;
#endif

            string[] splitRoutes = SelectedStop.Routes.Split(',');
            splitRoutes[0] = " " + splitRoutes[0];

            url = WebRequests.GetMulitPredictionUrl(SelectedStop.StopTags);
            List<Route> routeList = await PredictionHelper.GetPredictionTimesAsync(url);

            if (routeList.Count == 0) NoTimesText = "No busses at this time";
            else
            {
                foreach (Route r in routeList)
                {
                    NoTimesText = "";
                    Routes.Add(r);
                }
            }

#if WINDOWS_PHONE_APP
            systemTray.ProgressIndicator.ProgressValue = 0;
            systemTray.ProgressIndicator.Text = "nexMUNI";
#endif
        }

        public async Task RefreshTimes()
        {
            Routes.Clear();
            List<Route> routeList = await PredictionHelper.GetPredictionTimesAsync(url);

            foreach (Route r in routeList)
            {
                Routes.Add(r);
            }
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
