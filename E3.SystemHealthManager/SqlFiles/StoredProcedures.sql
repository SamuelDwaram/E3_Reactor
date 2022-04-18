drop procedure if exists dbo.GetAllSystemFailurePolicies
drop procedure if exists dbo.InsertSystemFailurePolicy
drop procedure if exists dbo.UpdateSystemFailurePolicy
drop procedure if exists dbo.DeleteSystemFailurePolicy
drop procedure if exists dbo.InsertSystemFailure
drop procedure if exists dbo.UpdateSystemFailure
drop procedure if exists dbo.ModifyFailureState
drop procedure if exists dbo.GetAllSystemFailures
go

--get all SystemFailurePolices
create procedure dbo.GetAllSystemFailurePolicies
as
begin
	set nocount on;
	select * from dbo.SystemFailurePolicies;
end
go

--insert SystemFailurePolicy
create procedure dbo.InsertSystemFailurePolicy
@DeviceId nvarchar(50),
@DeviceLabel nvarchar(50),
@FailedResourceLabel nvarchar(50),
@TargetValue nvarchar(10),
@Title nvarchar(max),
@Message nvarchar(max),
@TroubleShootMessage nvarchar(max),
@FailureResourceType nvarchar(20),
@Status bit,
@CreatedTimeStamp datetime
as
begin
	set nocount on;
	insert into dbo.SystemFailurePolicies values(@DeviceId, @DeviceLabel, @FailedResourceLabel, @TargetValue, @Title, @Message, @TroubleShootMessage, @FailureResourceType, @Status, @CreatedTimeStamp);
end
go

--update SystemFailurePolicy
create procedure dbo.UpdateSystemFailurePolicy
@Id int,
@TargetValue nvarchar(10),
@Title nvarchar(max),
@Message nvarchar(max),
@TroubleShootMessage nvarchar(max),
@FailureResourceType nvarchar(20),
@Status bit
as
begin
	set nocount on;
	update dbo.SystemFailurePolicies set TargetValue = @TargetValue, Title = @Title, Message = @Message, TroubleShootMessage = @TroubleShootMessage,
	FailureResourceType = @FailureResourceType, Status = @Status where Id = @Id;
end
go

--delete SystemFailurePolicy
create procedure dbo.DeleteSystemFailurePolicy
@Id int
as
begin
	set nocount on;
	delete from dbo.SystemFailurePolicies where Id = @Id;
end
go

--Insert System Failure
create procedure dbo.InsertSystemFailure
@SystemFailurePolicyId int,
@DeviceId nvarchar(50),
@DeviceLabel nvarchar(50),
@Title nvarchar(50),
@TroubleShootMessage nvarchar(max),
@FailedResourceLabel nvarchar(50),
@FailureState nvarchar(20),
@FailureType nvarchar(20),
@TimeStamp datetime
as
begin
	set nocount on;
	insert into dbo.SystemFailures(SystemFailurePolicyId, DeviceId, DeviceLabel, Title, TroubleShootMessage, FailedResourceLabel, FailureState, FailureType, TimeStamp, RaisedTimeStamp)
	values(@SystemFailurePolicyId, @DeviceId, @DeviceLabel, @Title, @TroubleShootMessage, @FailedResourceLabel, @FailureState, @FailureType, @TimeStamp, @TimeStamp);
end
go

--Modify Failure State
create procedure dbo.ModifyFailureState
@Id int,
@FailureState nvarchar(20)
as
begin
	set nocount on;
	if @FailureState = 'Acknowledged'
		update dbo.SystemFailures set FailureState = @FailureState, AcknowledgedTimeStamp = GETDATE(), TimeStamp = GETDATE() where Id = @Id;
	else 
		update dbo.SystemFailures set FailureState = @FailureState, TimeStamp = GETDATE() where Id = @Id;
end
go

--Get All System Failures
create procedure dbo.GetAllSystemFailures
as
begin
	set nocount on;
	declare @timeLimit datetime = (select DATEADD(DD, -1, GETDATE()));
	select * from dbo.SystemFailures inner join dbo.SystemFailurePolicies on dbo.SystemFailures.SystemFailurePolicyId=dbo.SystemFailurePolicies.Id
	where CONVERT(varchar, TimeStamp, 21) >= CONVERT(varchar, @timeLimit, 21);
end
go