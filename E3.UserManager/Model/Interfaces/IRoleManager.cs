using E3.UserManager.Model.Data;
using System.Collections.Generic;

namespace E3.UserManager.Model.Interfaces
{
    public interface IRoleManager
    {
        /// <summary>
        /// Add Role
        /// </summary>
        /// <param name="role"></param>
        void AddRole(Role role);

        /// <summary>
        /// Add Role
        /// </summary>
        /// <param name="role"></param>
        void UpdateRole(Role role);

        void AddAccessibleModuleToRole(string roleName, string newModuleName);
        void RemoveAccessibleModuleToRole(string roleName, string newModuleName);

        /// <summary>
        /// Delete Role
        /// </summary>
        /// <param name="role"></param>
        void DeleteRole(Role role);

        /// <summary>
        /// Get all roles in System
        /// </summary>
        /// <returns></returns>
        IList<Role> GetAllRoles();

        /// <summary>
        /// Assign role to User
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        void AssignRole(User user, Role role);

        /// <summary>
        /// returns list of roles assigned to the user
        /// </summary>
        /// <param name="userId"></param>
        IList<Role> GetAssignedRolesOfUser(string userId);

        /// <summary>
        /// returns list of modules accessible by the role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        IList<string> GetAccessibleModulesByRole(string roleName);

        /// <summary>
        /// Returns permission for accessing module by the user
        /// </summary>
        /// <param name="assignedRolesToUser"></param>
        /// <returns></returns>
        bool CanAccessModule(IList<Role> assignedRolesToUser, string moduleName);
    }
}
