using E3.UserManager.Model.Data;
using System.Collections.Generic;

namespace E3.UserManager.Model.Interfaces
{
    public interface IUserManager
    {
        /// <summary>
        /// Add user to Database
        /// </summary>
        /// <param name="user"></param>
        void AddUser(User user, Credential credentials);

        /// <summary>
        /// Validates password as per given format
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        bool IsValidPasswordFormat(string password);

        /// <summary>
        /// Edit User
        /// </summary>
        /// <param name="user"></param>
        void UpdateUser(string userId, string nameOfUser, string fieldToBeUpdated, string updatedValue);

        /// <summary>
        /// loginState = 0 - User Logged out
        /// loginState = 1 - User Logged In
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="loginState"></param>
        void UpdateUserLoginStatus(string userId, bool loginStateToBeUpdated);

        bool GetUserLoginStatus(string userId);

        /// <summary>
        /// Get all users 
        /// </summary>
        /// <returns></returns>
        IList<User> GetAllUsers();

        /// <summary>
        /// Authenticate User
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        User AuthenticateCredential(Credential credential);

        event UpdateUserInfoInUI UpdateUserInfoInUIEvent;
    }

    public delegate void UpdateUserInfoInUI(User user, string updatedProperty);
}
