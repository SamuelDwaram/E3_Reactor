/* Add new user */
create procedure [dbo].[AddUser]
@UserID nvarchar(50),
@Name nvarchar(MAX),
@Designation nvarchar(50),
@CurrentStatus nvarchar(50),
@CreatedDate nvarchar(50),
@Username nvarchar(50),
@PasswordHash nvarchar(max)
as
begin
	set nocount on;
	insert into [dbo].[Users] values(@UserID, @Name, @Designation, @CurrentStatus, @CreatedDate, GETDATE())
	insert into [dbo].[Credentials] values(@UserID, @Username, @PasswordHash)
end
go

/* check if username already exists */
create procedure [dbo].[CheckIfUsernameAlreadyExists]
@Username nvarchar(max)
as
begin
	set nocount on;
	select * from [dbo].[Credentials] where Username = @Username;
end
go

/* Authenticates User entered Credentials at Login Page */
create procedure [dbo].[AuthenticateCredentials]
@Username nvarchar(50),
@Password nvarchar(50)
as
begin
	set nocount on;
	select * from [dbo].[Users] where UserID in (select UserID from [dbo].[Credentials] where Username = @Username COLLATE SQL_Latin1_General_CP1_CS_AS and Password = @Password)
end
go

/* Gets all the users */
create procedure [dbo].[GetAllUsers]
as
begin
	set nocount on;
	select * from [dbo].[Users]
end
go

/* Get all roles */
create procedure dbo.GetAllRoles
as
begin
	set nocount on;
	select * from dbo.RolesAndAccessibleModules;
end
go

/* Get Accessible Modules by Role */
create procedure dbo.GetAccessibleModulesByRole
@RoleName nvarchar(20)
as
begin
	set nocount on;
	select * from dbo.RolesAndAccessibleModules where RoleName=@RoleName;
end
go

/* Get Assigned roles of user */
create procedure dbo.GetAssignedRolesOfUser
@UserId nvarchar(50)
as
begin
	set nocount on;
	select dbo.RolesAndAccessibleModules.RoleName, dbo.RolesAndAccessibleModules.ModulesAccessible from dbo.UsersAndRoles join dbo.RolesAndAccessibleModules on dbo.UsersAndRoles.RoleName = dbo.RolesAndAccessibleModules.RoleName where UserID=@UserId;
end
go

/* Add role */
create procedure dbo.AddRole
@RoleName nvarchar(20),
@ModulesAccessible nvarchar(max)
as
begin
	set nocount on;
	insert into dbo.RolesAndAccessibleModules values(@RoleName, @ModulesAccessible);
end
go

/* Assign role to user */
create procedure dbo.AssignRoleToUser
@UserID nvarchar(50),
@RoleName nvarchar(50)
as
begin
	set nocount on;
	insert into dbo.UsersAndRoles values(@UserID, @RoleName);
end
go

/* Delete role */
create procedure dbo.DeleteRole
@RoleName nvarchar(20)
as
begin
	set nocount on;
	delete from dbo.RolesAndAccessibleModules where RoleName=@RoleName;
	delete from dbo.UsersAndRoles where RoleName=@RoleName;
end
go

/* Update Role */
create procedure dbo.UpdateRole
@RoleName nvarchar(20),
@ModulesAccessible nvarchar(max)
as
begin
	set nocount on;
	update dbo.RolesAndAccessibleModules set ModulesAccessible=@ModulesAccessible where RoleName=@RoleName;
end
go

drop procedure if exists dbo.UpdateUser
go
/* update user */
create procedure dbo.UpdateUser
@UserID nvarchar(50),
@FieldToBeUpdated nvarchar(20),
@UpdatedValue nvarchar(max)
as
begin
	set nocount on;
	declare @query nvarchar(max);
	
	if (LOWER(@FieldToBeUpdated) = 'password' or LOWER(@FieldToBeUpdated) = 'username')
		begin
			set @query = 'update dbo.Credentials set ' + @FieldToBeUpdated + ' = ''' + @UpdatedValue + ''' where UserID = ' + @UserID + '';
			if(LOWER(@FieldToBeUpdated) = 'password')
				update dbo.Users set ModifiedDate = GETDATE();/* While Updating password - Update Modified Date */
		end
	else if(LOWER(@FieldToBeUpdated) = 'accesslevel')
		update dbo.UsersAndRoles set RoleName=@UpdatedValue where UserID=@UserID;
	else 
		set @query = 'update dbo.Users set ' + @FieldToBeUpdated + ' = ''' + @UpdatedValue + ''' where UserID = ' + @UserID + '';
	exec SP_EXECUTESQL @query;
end
go

drop procedure if exists dbo.UpdateUserLoginStatus
go
create procedure dbo.UpdateUserLoginStatus
@UserId nvarchar(50),
@LoginStateToBeUpdated bit
as
begin
	set nocount on;
	if @LoginStateToBeUpdated = 1
		insert into dbo.UserLoginStatus values(@UserId, 1);
	else
		delete from dbo.UserLoginStatus where UserId=@UserId;
end
go
