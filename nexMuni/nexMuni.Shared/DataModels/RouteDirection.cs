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
            Title = directionTitle;
            Times = directionTimes;
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
