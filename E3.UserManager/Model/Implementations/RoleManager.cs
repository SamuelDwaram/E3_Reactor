using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.Interfaces.Framework.Logging;
using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace E3.UserManager.Model.Implementations
{
    public class RoleManager : IRoleManager
    {
        private readonly IDatabaseReader databaseReader;
        private readonly IDatabaseWriter databaseWriter;
        private readonly ILogger logger;

        public RoleManager(IDatabaseWriter databaseWriter, IDatabaseReader databaseReader, ILogger logger)
        {
            this.logger = logger;
            this.databaseReader = databaseReader;
            this.databaseWriter = databaseWriter;
        }

        public void AddRole(Role role)
        {
            logger.Log(LogType.Information, "Adding Role : " + role.Name);
            databaseWriter.AddRole(role.Name, role.ModulesAccessable);
        }

        public void AssignRole(User user, Role role)
        {
            logger.Log(LogType.Information, "Assigning Role : " + role.Name + " to User : " + user.Name);
            databaseWriter.AssignRoleToUser(user.UserID, role.Name);
        }

        public void DeleteRole(Role role)
        {
            logger.Log(LogType.Information, "Deleting Role : " + role.Name);
            databaseWriter.DeleteRole(role.Name);
        }

        public void UpdateRole(Role role)
        {
            logger.Log(LogType.Information, "Updating Role : " + role.Name);
            databaseWriter.UpdateRole(role.Name, role.ModulesAccessable);
        }

        public IList<Role> GetAllRoles()
        {
            IList<Role> availableRoles = new List<Role>();
            foreach (DataRow row in databaseReader.GetAllRoles().AsEnumerable())
            {
                if (availableRoles.Any(role => role.Name == row["RoleName"].ToString()))
                {
                    availableRoles.Where(role => role.Name == row["RoleName"].ToString()).ToList()
                        .ForEach(role => role.ModulesAccessable.Add(row["ModulesAccessible"].ToString()));
                }
                else
                {
                    availableRoles.Add(new Role
                    {
                        Name = row["RoleName"].ToString(),
                        ModulesAccessable = new List<string> {
                            row["ModulesAccessible"].ToString(),
                        },
                    });
                }
            }
            return availableRoles;
        }

        public IList<Role> GetAssignedRolesOfUser(string userId)
        {
            IList<Role> assignedRoles = new List<Role>();
            foreach (DataRow row in databaseReader.GetAssignedRolesOfUser(userId).AsEnumerable())
            {
                if (assignedRoles.Any(role => role.Name == row["RoleName"].ToString()))
                {
                    assignedRoles.Where(role => role.Name == row["RoleName"].ToString()).ToList()
                        .ForEach(role => role.ModulesAccessable.Add(row["ModulesAccessible"].ToString()));
                }
                else
                {
                    assignedRoles.Add(new Role { 
                        Name = row["RoleName"].ToString(),
                        ModulesAccessable = new List<string> {
                            row["ModulesAccessible"].ToString(),
                        },
                    });
                }
            }
            return assignedRoles;
        }

        public IList<string> GetAccessibleModulesByRole(string roleName)
        {
            return (from DataRow row in databaseReader.GetAccessibleModulesByRole(roleName).AsEnumerable()
                    select !string.IsNullOrWhiteSpace(row["ModulesAccessible"].ToString()) ? row["ModulesAccessible"].ToString() : string.Empty).ToList();
        }

        public bool CanAccessModule(IList<Role> assignedRolesToUser, string moduleName)
        {
            foreach (Role role in assignedRolesToUser)
            {
                if (role.ModulesAccessable.Any(module => module == moduleName))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddAccessibleModuleToRole(string roleName, string newModuleName)
        {
            logger.Log(LogType.Information, $"Added Module-{newModuleName} to Role-{roleName}");
            databaseWriter.ExecuteWriteCommand($"insert into dbo.RolesAndAccessibleModules values('{roleName}', '{newModuleName}')", CommandType.Text);
        }

        public void RemoveAccessibleModuleToRole(string roleName, string newModuleName)
        {
            logger.Log(LogType.Information, $"Deleted Module-{newModuleName} to Role-{roleName}");
            databaseWriter.ExecuteWriteCommand($"delete from dbo.RolesAndAccessibleModules where RoleName='{roleName}' and ModulesAccessible='{newModuleName}'", CommandType.Text);
        }
    }
}
