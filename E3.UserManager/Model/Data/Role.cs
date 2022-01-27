using System.Collections.Generic;

namespace E3.UserManager.Model.Data
{
    /// <summary>
    /// Modules Accessable through Roles
    /// </summary>
    public class Role
    {
        public Role()
        {

        }

        public Role(string roleName)
        {
            Name = roleName;
        }

        public string Name { get; set; }

        public IList<string> ModulesAccessable { set; get; }
    }
}
