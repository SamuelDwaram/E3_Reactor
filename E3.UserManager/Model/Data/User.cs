using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace E3.UserManager.Model.Data
{
    /// <summary>
    /// User Class
    /// </summary>
    public class User : BindableBase
    {
        /// <summary>
        /// User ID(this field is the primary key for Users Table and foreign key for Credentials Table)
        /// </summary>
        private string _userId;
        public string UserID
        {
            get => _userId;
            set
            {
                _userId = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Name of User
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Designation of User
        /// </summary>
        private string _designation = string.Empty;
        public string Designation
        {
            get => _designation;
            set
            {
                _designation = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Roles of User
        /// </summary>
        private IList<Role> _roles;
        public IList<Role> Roles
        {
            get => _roles ?? (_roles = new List<Role>());
            set
            {
                _roles = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Date when user is created
        /// </summary>
        private DateTime _createdDate;
        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Date when user details like Password are modified
        /// </summary>
        private DateTime _modifiedDate;
        public DateTime ModifiedDate
        {
            get { return _modifiedDate; }
            set 
            {
                if (value != default)
                {
                    CheckForPasswordExpiry(value);
                }
                SetProperty(ref _modifiedDate, value); 
            }
        }

        /// <summary>
        /// Current Status of User (Enabled or Disabled)
        /// </summary>
        private UserStatus _currentStatus;
        public UserStatus CurrentStatus
        {
            get => _currentStatus;
            set
            {
                _currentStatus = value;
                RaisePropertyChanged();
            }
        }

        public int DaysRemainingInPasswordExpiry { get; set; }

        private bool _passwordExpired;
        public bool PasswordExpired
        {
            get => _passwordExpired;
            set => SetProperty(ref _passwordExpired, value);
        }

        private void CheckForPasswordExpiry(DateTime lastModifiedDate)
        {
            PasswordExpired = DateTime.Now.Subtract(lastModifiedDate).Days >= 30;
            DaysRemainingInPasswordExpiry = 30 - DateTime.Now.Subtract(lastModifiedDate).Days;
        }
    }
}
