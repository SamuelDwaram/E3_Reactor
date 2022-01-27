using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace E3.UserManager.ViewModels
{
    public class LiveDateTimeViewModel : BindableBase
    {
        private readonly Timer timer = new Timer(1000);

        public LiveDateTimeViewModel()
        {
            timer.Elapsed += Timer_Elapsed;
            Task.Factory.StartNew(new Action(() => timer.Start()));
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LiveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        #region Properties
        private string _liveDateTime;
        public string LiveDateTime
        {
            get => _liveDateTime ?? (_liveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            set => SetProperty(ref _liveDateTime, value);
        }
        #endregion
    }
}
