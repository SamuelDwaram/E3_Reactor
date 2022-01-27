using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3Tech.Navigation
{
    public interface IViewManager
    {
        ObservableCollection<string> GetRegisteredViews();
        void AddView(string v);
    }
}
