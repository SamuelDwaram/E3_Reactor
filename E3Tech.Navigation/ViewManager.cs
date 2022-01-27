using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.Navigation
{
    public class ViewManager : IViewManager
    {
        private readonly ObservableCollection<string> views;

        public ViewManager()
        {
            views = new ObservableCollection<string>();
        }

        public void AddView(string viewName)
        {
            views.Add(viewName);
        }

        public ObservableCollection<string> GetRegisteredViews()
        {
            return views;
        }
    }
}
