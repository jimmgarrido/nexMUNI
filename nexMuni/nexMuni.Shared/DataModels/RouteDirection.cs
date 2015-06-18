using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace nexMuni.DataModels
{
    public class RouteDirection : INotifyPropertyChanged
    {
        public string Title { get; private set; }
        public string Times
        {
            get
            {
                return _times;
            }
            private set
            {
                _times = value;
                NotifyPropertyChanged("Times");
            }
        }

        private string _times;

        public RouteDirection(string directionTitle, string directionTimes)
        {
            if (directionTitle.Contains("via Downtown")) Title = directionTitle.Replace("via Downtown", "");
            else if (directionTitle.Contains("&amp;")) Title = directionTitle.Replace("&amp;", "&");
            else if (directionTitle.Contains("Daly City BART Station")) Title = directionTitle.Replace("( Daly City BART Station 4pm-7pm)", "DC BART");
            else Title = directionTitle;

            if (directionTimes[0].Equals('0')) Times = directionTimes.Insert(1, "<1").Remove(0,1);
            else Times = directionTimes;
        }

        public void SetTimes(string times)
        {
            if (times[0].Equals('0')) Times = times.Insert(1, "<1").Remove(0, 1);
            else Times = times;
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
