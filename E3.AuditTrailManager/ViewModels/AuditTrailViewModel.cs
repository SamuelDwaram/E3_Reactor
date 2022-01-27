using E3.AuditTrailManager.Model;
using E3.AuditTrailManager.Model.Data;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E3.AuditTrailManager.ViewModels
{
    public class AuditTrailViewModel : BindableBase, IRegionMemberLifetime, INavigationAware
    {
        private readonly IAuditTrailManager auditTrailManager;
        private readonly IRegionManager regionManager;
        private readonly AuditTrailReportHandler auditTrailReportHandler;
        private readonly TaskScheduler taskScheduler;

        public AuditTrailViewModel(IAuditTrailManager auditTrailManager, IRegionManager regionManager, AuditTrailReportHandler auditTrailReportHandler)
        {
            taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.auditTrailManager = auditTrailManager;
            auditTrailManager.UpdateAuditTrailView += OnUpdateAuditTrailView;
            this.regionManager = regionManager;
            this.auditTrailReportHandler = auditTrailReportHandler;
        }

        private void OnUpdateAuditTrailView(object sender, EventArgs e)
        {
            GetAuditTrail();
        }

        #region Navigation Aware Handlers
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion

        #region Get Audit Trail
        public void GetAuditTrail()
        {
            Task.Factory.StartNew(new Func<IList<AuditEvent>>(GetAuditTrailFromAuditTrailManager))
                .ContinueWith(new Action<Task<IList<AuditEvent>>>(UpdateAuditTrail));
        }

        private void UpdateAuditTrail(Task<IList<AuditEvent>> task)
        {
            if (task.IsCompleted)
            {
                AuditTrail = task.Result;
                RaisePropertyChanged(nameof(AuditTrail));
            }
        }

        private IList<AuditEvent> GetAuditTrailFromAuditTrailManager()
        {
            return auditTrailManager.GetAuditTrail();
        }
        #endregion

        public void Navigate(string screenIdentifier)
        {
            regionManager.RequestNavigate("SelectedViewPane", screenIdentifier);
        }

        #region Commands
        public ICommand GetLastWeekAuditTrailCommand
        {
            get => new DelegateCommand(() => {
                SelectedStartDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
                SelectedEndDate = DateTime.Now;
                RaisePropertyChanged(nameof(SelectedStartDate));
                RaisePropertyChanged(nameof(SelectedEndDate));
                Task.Factory.StartNew(new Func<IList<AuditEvent>>(() => auditTrailManager.GetAuditTrail(SelectedStartDate, SelectedEndDate)))
                    .ContinueWith(new Action<Task<IList<AuditEvent>>>(UpdateAuditTrail));
            });
        }

        public ICommand Get24HoursAuditTrailCommand
        {
            get => new DelegateCommand(() => {
                SelectedStartDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));
                SelectedEndDate = DateTime.Now;
                RaisePropertyChanged(nameof(SelectedStartDate));
                RaisePropertyChanged(nameof(SelectedEndDate));
                Task.Factory.StartNew(new Func<IList<AuditEvent>>(() => auditTrailManager.GetAuditTrail(SelectedStartDate, SelectedEndDate)))
                    .ContinueWith(new Action<Task<IList<AuditEvent>>>(UpdateAuditTrail));
            });
        }

        public ICommand PrintReportCommand
        {
            get => new DelegateCommand(() => {
                auditTrailReportHandler.PrintAuditTrailReport(SelectedStartDate, SelectedEndDate);
            });
        }
        public ICommand LoadAuditTrailCommand
        {
            get => new DelegateCommand(() => Task.Factory.StartNew(new Func<IList<AuditEvent>>(() => auditTrailManager.GetAuditTrail()))
            .ContinueWith(t => {
                AuditTrail.Clear();
                AuditTrail = t.Result;
                RaisePropertyChanged(nameof(AuditTrail));
            }));
        }

        public ICommand LoadAuditTrailPrevSetCommand
        {
            get => new DelegateCommand(() => Task.Factory.StartNew(new Func<IList<AuditEvent>>(() => auditTrailManager.GetAuditTrail(true, false, AuditTrail.First().TimeStamp)))
            .ContinueWith(t => {
                AuditTrail.Clear();
                AuditTrail = t.Result.OrderByDescending(ae => ae.TimeStamp).ToList();   //Special case for loading prev set
                RaisePropertyChanged(nameof(AuditTrail));
            })).ObservesProperty(() => AuditTrail);
        }

        public ICommand LoadAuditTrailNextSetCommand
        {
            get => new DelegateCommand(() => Task.Factory.StartNew(new Func<IList<AuditEvent>>(() => auditTrailManager.GetAuditTrail(false, true, AuditTrail.Last().TimeStamp)))
            .ContinueWith(t => {
                AuditTrail.Clear();
                AuditTrail = t.Result;
                RaisePropertyChanged(nameof(AuditTrail));
            })).ObservesProperty(() => AuditTrail);
        }

        public ICommand NavigateCommand
        {
            get => new DelegateCommand<string>(Navigate);
        }
        #endregion

        #region Properties
        public bool KeepAlive => false;
        public IList<AuditEvent> AuditTrail { get; set; } = new List<AuditEvent>();
        public DateTime SelectedStartDate { get; set; } = DateTime.Now;
        public DateTime SelectedEndDate { get; set; } = DateTime.Now;
        #endregion
    }
}
