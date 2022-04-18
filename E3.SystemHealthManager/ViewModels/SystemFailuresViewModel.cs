using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using E3.SystemHealthManager.Services;
using E3.SystemHealthManager.Models;
using Prism.Commands;
using System.Windows.Input;

namespace E3.SystemHealthManager.ViewModels
{
    public class SystemFailuresViewModel : BindableBase
    {
        private readonly ISystemFailuresManager systemFailuresManager;

        public SystemFailuresViewModel(ISystemFailuresManager systemFailuresManager)
        {
            this.systemFailuresManager = systemFailuresManager;
            this.systemFailuresManager.RefreshSystemFailures += SystemFailuresManager_RefreshSystemFailures;
        }

        private void SystemFailuresManager_RefreshSystemFailures(IEnumerable<SystemFailure> systemFailures)
        {
            IList<SystemFailure> updatedSystemFailuresList = new List<SystemFailure>();
            foreach (SystemFailure systemFailure in systemFailures)
            {
                if (SystemFailures.Any(f => f.Id == systemFailure.Id))
                {
                    //Update the systemfailure
                }
                else
                {
                    updatedSystemFailuresList.Add(systemFailure);
                }

                //UPdate the SystemFailures LIst
                SystemFailures = updatedSystemFailuresList;
            }
        }

        public void ViewSystemFailure(object failureId)
        {
            Console.WriteLine(failureId);
        }

        private void LoadSystemFailures()
        {
            Task.Factory.StartNew(new Func<IEnumerable<SystemFailure>>(() => { return systemFailuresManager.GetAll(); }))
                .ContinueWith(new Action<Task<IEnumerable<SystemFailure>>>((t) => SystemFailures = t.Result.ToList()));
        }

        #region Commands
        private ICommand _loadSystemFailuresCommand;
        public ICommand LoadSystemFailuresCommand
        {
            get => _loadSystemFailuresCommand ?? (_loadSystemFailuresCommand = new DelegateCommand(LoadSystemFailures));
            set => SetProperty(ref _loadSystemFailuresCommand, value);
        }

        private ICommand _viewSystemFailureCommand;
        public ICommand ViewSystemFailureCommand
        {
            get => _viewSystemFailureCommand ?? (_viewSystemFailureCommand = new DelegateCommand<object>(ViewSystemFailure));
            set => SetProperty(ref _viewSystemFailureCommand, value);
        }
        #endregion

        #region Properties
        private IList<SystemFailure> _systemFailures;
        public IList<SystemFailure> SystemFailures
        {
            get => _systemFailures ?? (_systemFailures = new List<SystemFailure>());
            set => SetProperty(ref _systemFailures, value);
        }
        #endregion
    }
}
