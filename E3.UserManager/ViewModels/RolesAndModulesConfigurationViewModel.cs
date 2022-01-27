using E3.UserManager.Model.Data;
using E3.UserManager.Model.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace E3.UserManager.ViewModels
{
    public class RolesAndModulesConfigurationViewModel :  BindableBase
    {
        private readonly IRoleManager roleManager;

        public RolesAndModulesConfigurationViewModel(IRoleManager roleManager)
        {
            this.roleManager = roleManager;
        }

        #region Commands
        public ICommand CloseCommand
        {
            get => new DelegateCommand<UserControl>(uc => {
                Window window = Window.GetWindow(uc);
                if (window != null)
                {
                    window.Close();
                }
            });
        }

        public ICommand LoadedCommand {
            get => new DelegateCommand(() => {
                AvailableRoles = roleManager.GetAllRoles();
                AvailableRoles.ToList().ForEach(role =>
                {
                    role.ModulesAccessable.ToList().ForEach(module =>
                    {
                        if (AvailableModules.Contains(module))
                        {
                            // Skip.
                        }
                        else
                        {
                            AvailableModules.Add(module);
                        }
                    });
                });
                RaisePropertyChanged(nameof(AvailableRoles));
            });
        }

        public ICommand GetAccessibleModulesCommand
        {
            get => new DelegateCommand<string>(roleName => {
                SelectedRole = roleName;
                ModulesAccessibleByRole = roleManager.GetAccessibleModulesByRole(roleName);
                RaisePropertyChanged(nameof(ModulesAccessibleByRole));
            });
        }

        public ICommand AddModuleToSelectedRoleCommand
        {
            get => new DelegateCommand<string>(module => {
                if (string.IsNullOrWhiteSpace(SelectedRole))
                {
                    MessageBox.Show("Please select a role to modify the accessible modules", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    //check if module is already present in ModulesAccessible of SelectedRole
                    if (roleManager.GetAccessibleModulesByRole(SelectedRole).Contains(module))
                    {
                        MessageBox.Show($"Module:{module} is already accessible by the Role:{SelectedRole}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show($"Are you sure you want to add {module} to {SelectedRole}", "Confirmation", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            roleManager.AddAccessibleModuleToRole(SelectedRole, module);
                            GetAccessibleModulesCommand.Execute(SelectedRole);
                            Debug.WriteLine($"Adding {module} to {SelectedRole}");
                            MessageBox.Show("Please LogOut and Login to the System to observe the Modifications.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            Debug.WriteLine($"Failed to add {module} to {SelectedRole}");
                        }
                    }
                }
            });
        }

        public ICommand RemoveModuleToSelectedRoleCommand
        {
            get => new DelegateCommand<string>(module => {
                if (string.IsNullOrWhiteSpace(SelectedRole))
                {
                    MessageBox.Show("Please select a role to modify the accessible modules", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    IList<string> modulesAccessibleByRole = roleManager.GetAccessibleModulesByRole(SelectedRole);
                    //check if module is already present in ModulesAccessible of SelectedRole
                    if (modulesAccessibleByRole.Contains(module))
                    {
                        if (modulesAccessibleByRole.Count == 1)
                        {
                            MessageBox.Show("You cannot remove this module since each AccessLevel must contain atleast one Accessible Module", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        MessageBoxResult result = MessageBox.Show($"Are you sure you want to Remove {module} from {SelectedRole}", "Confirmation", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            roleManager.RemoveAccessibleModuleToRole(SelectedRole, module);
                            GetAccessibleModulesCommand.Execute(SelectedRole);
                            Debug.WriteLine($"Removed {module} to {SelectedRole}");
                            MessageBox.Show("Please LogOut and Login to the System to observe the Modifications.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            Debug.WriteLine($"Failed to add {module} to {SelectedRole}");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Module:{module} is not accessible by the Role:{SelectedRole}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            });
        }
        #endregion

        #region Properties
        private string _selectedRole = string.Empty;
        public string SelectedRole
        {
            get { return _selectedRole; }
            set { SetProperty(ref _selectedRole, value); }
        }
        public IList<Role> AvailableRoles { get; set; } = new List<Role>();
        public ObservableCollection<string> AvailableModules { get; set; } = new ObservableCollection<string>();
        public IList<string> ModulesAccessibleByRole { get; set; } = new List<string>();
        #endregion
    }
}
