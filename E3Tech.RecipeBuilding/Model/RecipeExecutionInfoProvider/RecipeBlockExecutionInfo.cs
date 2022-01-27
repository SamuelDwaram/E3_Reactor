using Prism.Mvvm;

namespace E3Tech.RecipeBuilding.Model.RecipeExecutionInfoProvider
{
    public class RecipeBlockExecutionInfo : BindableBase
    {
        private string _fieldDeviceIdentifier;
        public string FieldDeviceIdentifier
        {
            get => _fieldDeviceIdentifier;
            set => SetProperty(ref _fieldDeviceIdentifier, value);
        }

        private string _batchIdentifier;
        public string BatchIdentifier
        {
            get => _batchIdentifier;
            set => SetProperty(ref _batchIdentifier, value);
        }

        private string _startTime;
        public string StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private string _endTime;
        public string EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        private string _duration;
        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _executionMessage;
        public string ExecutionMessage
        {
            get => _executionMessage;
            set => SetProperty(ref _executionMessage, value);
        }

        private string _timeStamp;
        public string TimeStamp
        {
            get => _timeStamp;
            set => SetProperty(ref _timeStamp, value);
        }
    }
}
