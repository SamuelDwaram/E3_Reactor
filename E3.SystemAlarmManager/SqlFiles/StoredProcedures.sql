drop procedure if exists dbo.GetAllSystemAlarmPolicies
go
--get all SystemAlarmPolices
create procedure dbo.GetAllSystemAlarmPolicies
as
begin
	set nocount on;
	select * from dbo.SystemAlarmPolicies inner join dbo.SystemAlarmParameters on dbo.SystemAlarmPolicies.Id=dbo.SystemAlarmParameters.Id;
end
go

drop procedure if exists dbo.InsertSystemAlarmPolicy
go
--insert SystemAlarmPolicy
create procedure dbo.InsertSystemAlarmPolicy
@DeviceId nvarchar(50),
@DeviceLabel nvarchar(50),
@PolicyType nvarchar(10),
@Title nvarchar(max),
@Message nvarchar(max),
@Status bit,
@ParameterName nvarchar(50),
@ParametersType nvarchar(50),
@RatedValue float,
@VariationPercentage float,
@VariationType nvarchar(10),
@UpperLimit float,
@LowerLimit float
as
begin
	set nocount on;
	insert into dbo.SystemAlarmPolicies values(@DeviceId, @DeviceLabel, @PolicyType, @Title, @Message, @Status, GETDATE());
	declare @alarmPolicyId int = (select SCOPE_IDENTITY() SystemAlarmPolicies);
	insert into dbo.SystemAlarmParameters 
		values(@alarmPolicyId, @ParameterName, @ParametersType, @RatedValue, @VariationPercentage, @VariationType, @UpperLimit, @LowerLimit);
end
go

drop procedure if exists dbo.UpdateSystemAlarmPolicyAndAlarmParameters
go
--update SystemAlarmPolicy and SystemAlarmParameters
create procedure dbo.UpdateSystemAlarmPolicyAndAlarmParameters
@Id int,
@PolicyType nvarchar(10),
@Title nvarchar(max),
@Message nvarchar(max),
@Status bit,
@ParametersType nvarchar(50),
@RatedValue float,
@VariationPercentage float,
@VariationType nvarchar(10),
@UpperLimit float,
@LowerLimit float
as
begin
	set nocount on;
	update dbo.SystemAlarmPolicies set PolicyType = @PolicyType, Title = @Title, Message = @Message, Status = @Status where Id = @Id;
	update dbo.SystemAlarmParameters set ParametersType = @ParametersType, RatedValue = @RatedValue, VariationPercentage = @VariationPercentage, 
		VariationType = @VariationType, UpperLimit = @UpperLimit, LowerLimit = @LowerLimit where Id = @Id
end
go

drop procedure if exists dbo.DeleteSystemAlarmPolicy
go
--delete SystemAlarmPolicy
create procedure dbo.DeleteSystemAlarmPolicy
@Id int
as
begin
	set nocount on;
	delete from dbo.SystemAlarmParameters where Id = @Id;
	delete from dbo.SystemAlarmPolicies where Id = @Id;
end
go

drop procedure if exists dbo.InsertSystemAlarm
go
--Insert System Alarm
create procedure dbo.InsertSystemAlarm
@SystemAlarmPolicyId int = 0,
@SystemAlarmParameterName nvarchar(50) = '',
@SystemFailureId int = 0,
@DeviceId nvarchar(50),
@DeviceLabel nvarchar(50),
@FieldPointLabel nvarchar(50),
@Title nvarchar(50),
@Message nvarchar(max),
@State nvarchar(20),
@Type nvarchar(20),
@TimeStamp datetime
as
begin
	set nocount on;
	insert into dbo.SystemAlarms(SystemAlarmPolicyId, SystemAlarmParameterName, SystemFailureId, DeviceId, DeviceLabel, FieldPointLabel, Title, Message, State, Type,TimeStamp, RaisedTimeStamp)
	values(iif(@SystemAlarmPolicyId = 0, null, @SystemAlarmPolicyId), iif(@SystemAlarmParameterName = '', null, @SystemAlarmParameterName),
	iif(@SystemFailureId = 0, null, @SystemFailureId), @DeviceId, @DeviceLabel, @FieldPointLabel, @Title, @Message, @State, @Type, @TimeStamp, @TimeStamp);
end
go

drop procedure if exists dbo.UpdateSystemAlarm
go
--Update SystemAlarm
create procedure dbo.UpdateSystemAlarm
@Id int,
@State nvarchar(20),
@TimeStamp datetime,
@RaisedTimeStamp datetime,
@AcknowledgedTimeStamp datetime
as
begin
	set nocount on;
	update dbo.SystemAlarms set State = @State, TimeStamp = @TimeStamp, RaisedTimeStamp = @RaisedTimeStamp, AcknowledgedTimeStamp = @AcknowledgedTimeStamp
	where Id = @Id;
end
go

drop procedure if exists dbo.ModifyAlarmState
go
--Modify Alarm State
create procedure dbo.ModifyAlarmState
@Id int,
@AlarmState nvarchar(20)
as
begin
	set nocount on;
	declare @isResolvedBefore bit;
	set @isResolvedBefore = (select iif((select count(*) from dbo.SystemAlarms where State='Resolved' and Id=@Id) > 0, 1, 0));
	if @isResolvedBefore > 0
		update dbo.SystemAlarms set AcknowledgedTimeStamp=GETDATE() where Id=@Id;
	else
		begin
			if @AlarmState = 'Acknowledged'
				update dbo.SystemAlarms set State=@AlarmState, AcknowledgedTimeStamp=GETDATE(), TimeStamp=GETDATE() where Id=@Id;
			else
				update dbo.SystemAlarms set State=@AlarmState, TimeStamp=GETDATE() where Id=@Id;
		end
end
go

drop procedure if exists dbo.GetAllSystemAlarms
go
--Get All System Alarms
create procedure dbo.GetAllSystemAlarms
as
begin
	set nocount on;
	declare @timeLimit datetime = (select DATEADD(DD, -1, GETDATE()));
	select * from dbo.SystemAlarms where CONVERT(varchar, RaisedTimeStamp, 21) >= CONVERT(varchar, @timeLimit, 21);
end
go

drop procedure if exists dbo.GetAlarmsBetweenTimeStamps
go
create procedure dbo.GetAlarmsBetweenTimeStamps
@DeviceId nvarchar(50),
@StartTime datetime, 
@EndTime datetime
as
begin
	set nocount on;
	select * from dbo.SystemAlarms where DeviceId = @DeviceId and (RaisedTimeStamp between @StartTime and @EndTime);
end
go

drop procedure if exists dbo.GetMaxDevicePoliciesType
go
create procedure dbo.GetMaxDevicePoliciesType
@DeviceId nvarchar(30)
as
begin
	set nocount on;
	declare @groupPolicyCount int, @individualPolicyCount int;
	set @groupPolicyCount = (select count(*) from dbo.SystemAlarmPolicies where PolicyType='Group' and DeviceId=@DeviceId);
	set @individualPolicyCount = (select count(*) from dbo.SystemAlarmPolicies where PolicyType='Individual' and DeviceId=@DeviceId);
	if @groupPolicyCount > @individualPolicyCount
		select 'Group';
	else
		select 'Individual';
end
go