delete from dbo.RolesAndAccessibleModules
go

INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Admin', 'UserManagement')
INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Admin', 'DesignExperiment')
INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Admin', 'ReactorControl')
INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Admin', 'OtherEquipment')
INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Admin', 'ChillerControl')

INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Engineer', 'DesignExperiment')
INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Engineer', 'ReactorControl')
INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Engineer', 'OtherEquipment')
INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Engineer', 'ChillerControl')

INSERT dbo.RolesAndAccessibleModules (RoleName, ModulesAccessible) VALUES ('Supervisor', 'OtherEquipment')

