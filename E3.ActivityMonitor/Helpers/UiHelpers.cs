using E3.ActivityMonitor.Services;
using System.Configuration;
using System.Timers;
using System.Windows;
using Unity;

namespace E3.ActivityMonitor.Helpers
{
    public class UiHelpers
    {
        private static Timer _timer;
        private static IUnityContainer unityContainer;
        private static bool _timerInitialized = false;

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            unityContainer.Resolve<IActivityMonitor>().InvokeApplicationIsIdle();
        }

        private static void InitializeTimer()
        {
            _timer = new Timer(GetTimeFromIntervalAndIntervalType(ConfigurationManager.AppSettings["AllowedApplicationIdleIntervalType"], ConfigurationManager.AppSettings["AllowedApplicationIdleTime"]));
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
            _timerInitialized = true;
        }

        public static double GetTimeFromIntervalAndIntervalType(string intervalType, string intervalTime)
        {
            double calculatedInterval = 60000;
            switch (intervalType)
            {
                case "Hours":
                    calculatedInterval = double.Parse(intervalTime) * 3600 * 1000;
                    return calculatedInterval > 0 ? calculatedInterval : 60000;
                case "Minutes":
                    calculatedInterval = double.Parse(intervalTime) * 60 * 1000;
                    return calculatedInterval > 0 ? calculatedInterval : 60000;
                case "Seconds":
                    calculatedInterval = double.Parse(intervalTime) * 1000;
                    return calculatedInterval > 60 ? calculatedInterval : 60000;
                default:
                    return calculatedInterval;
            }
        }

        #region PreviewKeyDown
        public static object GetPreviewKeyDown(DependencyObject obj)
        {
            return obj.GetValue(PreviewKeyDownProperty);
        }

        public static void SetPreviewKeyDown(DependencyObject obj, object value)
        {
            obj.SetValue(PreviewKeyDownProperty, value);
        }

        public static readonly DependencyProperty PreviewKeyDownProperty =
            DependencyProperty.RegisterAttached("PreviewKeyDown", typeof(object), typeof(UiHelpers), new PropertyMetadata(PreviewKeyDown));

        private static void PreviewKeyDown(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is Window))
            {
                return;
            }

            if (!_timerInitialized)
            {
                InitializeTimer();
            }

            (uiElement as Window).PreviewKeyDown += (sender, args) => {
                _timer.Stop();
                _timer.Start();
            };
        }
        #endregion

        #region PreviewMouseDown
        public static object GetPreviewMouseDown(DependencyObject obj)
        {
            return obj.GetValue(PreviewMouseDownProperty);
        }

        public static void SetPreviewMouseDown(DependencyObject obj, object value)
        {
            obj.SetValue(PreviewMouseDownProperty, value);
        }

        public static readonly DependencyProperty PreviewMouseDownProperty =
            DependencyProperty.RegisterAttached("PreviewMouseDown", typeof(object), typeof(UiHelpers), new PropertyMetadata(PreviewMouseDown));

        private static void PreviewMouseDown(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is Window))
            {
                return;
            }

            if (!_timerInitialized)
            {
                InitializeTimer();
            }

            (uiElement as Window).PreviewMouseDown += (sender, args) => {
                _timer.Stop();
                _timer.Start();
            };
        }
        #endregion

        #region UnityContainer
        public static IUnityContainer GetUnityContainer(DependencyObject obj)
        {
            return (IUnityContainer)obj.GetValue(UnityContainerProperty);
        }

        public static void SetUnityContainer(DependencyObject obj, IUnityContainer value)
        {
            obj.SetValue(UnityContainerProperty, value);
        }

        public static readonly DependencyProperty UnityContainerProperty =
            DependencyProperty.RegisterAttached("UnityContainer", typeof(IUnityContainer), typeof(UiHelpers), new PropertyMetadata(UnityContainerChanged));

        private static void UnityContainerChanged(DependencyObject uiElement, DependencyPropertyChangedEventArgs e)
        {
            if (!(uiElement is Window))
            {
                return;
            }

            unityContainer = GetUnityContainer(uiElement);
        }
        #endregion
    }
}
