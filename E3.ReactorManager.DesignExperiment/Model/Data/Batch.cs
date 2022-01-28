using Prism.Mvvm;
using System;

namespace E3.ReactorManager.DesignExperiment.Model.Data
{
    public class Batch : BindableBase
    {
        /// <summary>
        /// Unique for every batch
        /// </summary>
        private string _identifier;
        public string Identifier
        {
            get { return _identifier; }
            set
            {
                _identifier = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Describes the BatchName
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Describes the Batch number
        /// </summary>
        private string _number;
        public string Number
        {
            get { return _number; }
            set
            {
                _number = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Stage of Batch
        /// </summary>
        private string _stage;
        public string Stage
        {
            get => _stage;
            set
            {
                _stage = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Describes the Scientist conducting the batch
        /// </summary>
        private string _scientistName;
        public string ScientistName
        {
            get { return _scientistName; }
            set
            {
                _scientistName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Describes the field device identifier
        /// in which the batch was being conducted
        /// </summary>
        private string _fieldDeviceIdentifier;
        public string FieldDeviceIdentifier
        {
            get { return _fieldDeviceIdentifier; }
            set
            {
                _fieldDeviceIdentifier = value;
                RaisePropertyChanged();
            }
        }

        private string _fieldDeviceLabel;
        public string FieldDeviceLabel
        {
            get => _fieldDeviceLabel;
            set
            {
                _fieldDeviceLabel = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// HC Identifier
        /// </summary>
        private string _hcIdentifier;
        public string HCIdentifier
        {
            get { return _hcIdentifier; }
            set
            {
                _hcIdentifier = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Stirrer Identifier
        /// </summary>
        private string _stirrerIdentifier;
        public string StirrerIdentifier
        {
            get { return _stirrerIdentifier; }
            set
            {
                _stirrerIdentifier = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Dosing Pump Usage(Yes or No)
        /// </summary>
        private string _dosingPumpUsage;
        public string DosingPumpUsage
        {
            get { return _dosingPumpUsage; }
            set
            {
                _dosingPumpUsage = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Describes the chemical database which was being 
        /// used for the batch
        /// </summary>
        private string _chemicalDatabaseIdentifier;
        public string ChemicalDatabaseIdentifier
        {
            get { return _chemicalDatabaseIdentifier; }
            set
            {
                _chemicalDatabaseIdentifier = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Describes if there is any image of the reaction
        /// </summary>
        private byte[] _imageOfReaction;
        public byte[] ImageOfReaction
        {
            get { return _imageOfReaction; }
            set
            {
                _imageOfReaction = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Describes any comments entered by the scientist 
        /// before starting the batch
        /// </summary>
        private string _comments;
        public string Comments
        {
            get { return _comments; }
            set
            {
                _comments = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// describes the state of the Batch
        /// </summary>
        private BatchState _state;
        public BatchState State
        {
            get => _state;
            set
            {
                _state = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// describes when the batch was started
        /// </summary>
        private DateTime _timeStarted;
        public DateTime TimeStarted
        {
            get { return _timeStarted; }
            set
            {
                _timeStarted = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// describes when the batch was completed
        /// </summary>
        private DateTime _timeCompleted;
        public DateTime TimeCompleted
        {
            get { return _timeCompleted; }
            set
            {
                _timeCompleted = value;
                RaisePropertyChanged();
            }
        }
    }
}
