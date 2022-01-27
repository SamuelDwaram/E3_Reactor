using Prism.Mvvm;

namespace E3.UserManager.Model.Data
{
    /// <summary>
    /// Credential
    /// </summary>
    public class Credential : BindableBase
    {
        /// <summary>
        /// User Id 
        /// </summary>
        private string _userId;
        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Name of User(used for login)
        /// </summary>
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Password of User(used for login)
        /// </summary>
        private string _passwordHash;
        public string PasswordHash
        {
            get => _passwordHash;
            set
            {
                _passwordHash = value;
                RaisePropertyChanged();
            }
        }
    }
}
