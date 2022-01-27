drop table if exists dbo.Users
/* Users */
create table [dbo].[Users]
(
UserID	nvarchar(50) not null primary key,
Name	nvarchar(50),
Designation	nvarchar(50),
CurrentStatus	nvarchar(50),
CreatedDate	datetime,
ModifiedDate datetime
)

drop table if exists dbo.Credentials
/* Credentials */
create table [dbo].[Credentials]
(
UserID	nvarchar(50) foreign key references Users(UserID),
Username	nvarchar(50),
Password	nvarchar(MAX)
)

drop table if exists dbo.UsersAndRoles
/* users and roles */
create table dbo.UsersAndRoles
(
UserID nvarchar(50) foreign key references Users(UserID),
RoleName nvarchar(20) 
)

drop table if exists dbo.RolesAndAccessibleModules
/* Roles and accessible modules */
create table dbo.RolesAndAccessibleModules
(
RoleName nvarchar(20),
ModulesAccessible nvarchar(max)
)

drop table if exists dbo.UserLoginStatus
create table dbo.UserLoginStatus
(
UserId nvarchar(50) foreign key references Users(UserID),
LoginState bit, /* 1 - User Logged In, 0 - User Logged Out */
)
