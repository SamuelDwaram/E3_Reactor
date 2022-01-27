delete from dbo.UsersAndRoles
delete from dbo.Credentials
delete from dbo.Users

insert into dbo.Users values('1', 'E3 Tech', 'Installer', 'Active', GETDATE(), GETDATE())
insert into dbo.Credentials values('1', 'e3', 'e3')
insert into dbo.UsersAndRoles values('1', 'Admin')