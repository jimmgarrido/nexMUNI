using nexMuni.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace nexMuni.ViewModels
{
    public class TimesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate oneDirection { get; set; }
        public DataTemplate twoDirection { get; set; }
        public DataTemplate threeDirection { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Route routeItem = item as Route;

            if (routeItem.Directions.Count == 1) return oneDirection;
            else if (routeItem.Directions.Count == 2) return twoDirection;
            else if (routeItem.Directions.Count == 3) return threeDirection;
            else return base.SelectTemplateCore(item, container);
        }
    }
}
