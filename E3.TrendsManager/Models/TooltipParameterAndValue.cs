using Prism.Mvvm;

namespace E3.TrendsManager.Models
{
    public class TooltipParameterAndValue : BindableBase
    {
        private string _parameterName;
        public string ParameterName
        {
            get => _parameterName;
            set => SetProperty(ref _parameterName, value);
        }

        private string _parameterValue;
        public string ParameterValue
        {
            get => _parameterValue;
            set => SetProperty(ref _parameterValue, value);
        }
    }
}
