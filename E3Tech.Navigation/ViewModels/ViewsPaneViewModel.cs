using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using Unity;

namespace E3Tech.Navigation
{
    public class ViewsPaneViewModel : BindableBase
    {
        private readonly IUnityContainer unityContainer;
        private readonly IRegionManager regionManager;
        private readonly IViewManager viewManager;

        public ViewsPaneViewModel(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            this.unityContainer = unityContainer;
            this.regionManager = regionManager;
            viewManager = unityContainer.Resolve<IViewManager>();
            Views = viewManager.GetRegisteredViews();
        }

        private ObservableCollection<string> views;
        public ObservableCollection<string> Views
        {
            get => views;
            set
            {
                views = value;
                RaisePropertyChanged("Views");
            }
        }

        private int selectedView;
        public int SelectedView
        {
            get => selectedView;
            set
            {
                selectedView = value;
                NavigateToSelectedView();
                RaisePropertyChanged("SelectedView");
            }
        }

        private void NavigateToSelectedView()
        {
            regionManager.RequestNavigate("SelectedViewPane", new Uri(Views[SelectedView], UriKind.Relative));
        }
    }
}
