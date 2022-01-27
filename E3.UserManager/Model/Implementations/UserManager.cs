using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using E3.ReactorManager.Interfaces.Framework.Logging;
using System.Text.RegularExpressions;
using E3.ReactorManager.Interfaces.DataAbstractionLayer.Data;
using E3.DialogServices.Model;
using Unity;
using E3.Mediator.Services;
using E3.Mediator.Models;
using System.Windows;

namespace E3.UserManager.Model.Implementations
{
    public class UserManager : IUserManager
    {
        private readonly IDatabaseReader databaseReader;
        private readonly IUnityContainer unityContainer;
        private readonly IDatabaseWriter databaseWriter;
        private readonly IRoleManager roleManager;
        private readonly MediatorService mediatorService;
        private readonly ILogger logger;
        private readonly IDialogServiceProvider dialogServiceProvider;

        public UserManager(IUnityContainer unityContainer, IDatabaseWriter databaseWriter, IDatabaseReader databaseReader, ILogger logger, IRoleManager roleManager, MediatorService mediatorService)
        {
            this.logger = logger;
            this.databaseReader = databaseReader;
            this.unityContainer = unityContainer;
            this.databaseWriter = databaseWriter;
            this.roleManager = roleManager;
            this.mediatorService = mediatorService;
            dialogServiceProvider = unityContainer.Resolve<IDialogServiceProvider>();
        }

        public event UpdateUserInfoInUI UpdateUserInfoInUIEvent;

        public void AddUser(User addedUser, Credential credentials)
        {
            if (GetAllUsers().ToList().Any(user => user.Name.ToLower() == addedUser.Name) || IsCredentialRedundant(credentials))
            {
                dialogServiceProvider.ShowAlert("Redundancy Error", "User Already exist in the system PLEASE LOGIN!!");
            }
            else
            {
                logger.Log(LogType.DatabaseCall, "Adding user : " + addedUser.Name + " to database");
                addedUser.UserID = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                IList<DbParameterInfo> parameters = new List<DbParameterInfo>
                {
                    new DbParameterInfo("@UserID", addedUser.UserID, DbType.String),
                    new DbParameterInfo("@Name", addedUser.Name, DbType.String),
                    new DbParameterInfo("@Designation", addedUser.Designation, DbType.String),
                    new DbParameterInfo("@CurrentStatus", "Active", DbType.String),
                    new DbParameterInfo("@CreatedDate", DateTime.Now, DbType.DateTime),
                    new DbParameterInfo("@Username", credentials.Username, DbType.String),
                    new DbParameterInfo("@PasswordHash", credentials.PasswordHash, DbType.String),
                };
                databaseWriter.ExecuteWriteCommand("AddUser", CommandType.StoredProcedure, parameters);
                foreach (Role role in addedUser.Roles)
                {
                    roleManager.AssignRole(addedUser, role);
                }
            }
        }

        private bool IsCredentialRedundant(Credential credential)
        {
            return databaseReader.ExecuteReadCommand($"select * from dbo.Credentials where Username = '{credential.Username}' or Password = '{credential.PasswordHash}'", CommandType.Text).Rows.Count > 0;
        }

        public User AuthenticateCredential(Credential credential)
        {
            logger.Log(LogType.Information, "Authenticating Credentials " + credential.Username + " and " + credential.PasswordHash);
            return (from DataRow row in databaseReader.AuthenticateCredentials(credential.Username, credential.PasswordHash).AsEnumerable()
                    select new User()
                    {
                        UserID = row["UserID"].ToString(),
                        Name = row["Name"].ToString(),
                        Designation = row["Designation"].ToString(),
                        Roles = roleManager.GetAssignedRolesOfUser(row["UserID"].ToString()),
                        CurrentStatus = (UserStatus)Enum.Parse(typeof(UserStatus), row["CurrentStatus"].ToString()),
                        CreatedDate = DateTime.Parse(row["CreatedDate"].ToString()),
                        ModifiedDate = row["ModifiedDate"] == null ? default : Convert.ToDateTime(row["ModifiedDate"]),
                    }).ToList().FirstOrDefault();
        }

        public IList<User> GetAllUsers()
        {
            logger.Log(LogType.DatabaseCall, "Getting all users in the Database");
            return (from DataRow row in databaseReader.GetAllUsersInTheDatabase().AsEnumerable()
                    select new User()
                    {
                        UserID = row["UserID"].ToString(),
                        Name = row["Name"].ToString(),
                        Designation = row["Designation"].ToString(),
                        Roles = roleManager.GetAssignedRolesOfUser(row["UserID"].ToString()),
                        CurrentStatus = (UserStatus)Enum.Parse(typeof(UserStatus), row["CurrentStatus"].ToString()),
                        CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                        ModifiedDate = row["ModifiedDate"] == null ? default : Convert.ToDateTime(row["ModifiedDate"]),
                    }).ToList();
        }

        public bool IsValidPasswordFormat(string password)
        {
            return Regex.IsMatch(password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$");
        }

        public void UpdateUser(string userId, string nameOfUser, string fieldToBeUpdated, string updatedValue)
        {
            if (fieldToBeUpdated == "Password")
            {
                if (!IsValidPasswordFormat(updatedValue))
                {
                    MessageBox.Show("Invalid Password Format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Check whether Password is in valid format
                    return;
                }
            }
            logger.Log(LogType.DatabaseCall, "Updating " + fieldToBeUpdated + " with " + updatedValue + " of User with Id" + userId);
            databaseWriter.UpdateUser(userId, fieldToBeUpdated, updatedValue);

            //Record audit event for UpdateUser
            User loggedInUser = (User)Application.Current.Resources["LoggedInUser"];
            if (loggedInUser != null)
            {
                mediatorService.NotifyColleagues(InMemoryMediatorMessageContainer.RecordAudit, new object[] {
                    unityContainer, $"Changed {fieldToBeUpdated} of {nameOfUser} {(fieldToBeUpdated.Contains("Password") ?  string.Empty : $"to {updatedValue}" )}", loggedInUser.Name, "UserManagement" });
            }
            else
            {
                //Password changed at login
                mediatorService.NotifyColleagues(InMemoryMediatorMessageContainer.RecordAudit, new object[] {
                    unityContainer, $"Changed {fieldToBeUpdated} of {nameOfUser} {(fieldToBeUpdated.Contains("Password") ?  string.Empty : $"to {updatedValue}" )} at Login Prompt", string.Empty, "UserManagement" });
            }
            UpdateUserInfoInUIEvent?.BeginInvoke(GetAllUsers().First(u => u.UserID == userId), fieldToBeUpdated, null, null);
        }

        public void UpdateUserLoginStatus(string userId, bool loginStateTobeUpdated)
        {
            IList<DbParameterInfo> dbParameters = new List<DbParameterInfo>
            {
                new DbParameterInfo("@UserId", userId, DbType.String),
                new DbParameterInfo("@LoginStateToBeUpdated", loginStateTobeUpdated, DbType.Boolean),
            };
            databaseWriter.ExecuteWriteCommand("UpdateUserLoginStatus", CommandType.StoredProcedure, dbParameters);
        }

        public bool GetUserLoginStatus(string userId)
        {
            DataTable result = databaseReader.ExecuteReadCommand($"select top(1) LoginState from dbo.UserLoginStatus where UserId={userId}", CommandType.Text);
            return result.Rows.Count > 0 && result.AsEnumerable().First().Field<bool>(0);
        }
    }
}
