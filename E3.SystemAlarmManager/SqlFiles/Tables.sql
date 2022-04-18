--Clear all old tables
drop table if exists dbo.SystemAlarms
drop table if exists dbo.SystemAlarmParameters
drop table if exists dbo.SystemAlarmPolicies
go

--Alarm Policy
create table dbo.SystemAlarmPolicies 
(
Id int identity primary key,
DeviceId nvarchar(50) foreign key references dbo.FieldDevices(Identifier),
DeviceLabel nvarchar(50),
PolicyType nvarchar(10),
Title nvarchar(max),
Message nvarchar(max),
Status bit,
CreatedTimeStamp datetime,
constraint check_PolicyType check(PolicyType in ('Individual', 'Group'))
)

--Alarm Parameters
create table dbo.SystemAlarmParameters
(
Id int foreign key references dbo.SystemAlarmPolicies(Id),
Name nvarchar(50),
ParametersType nvarchar(20),
RatedValue float,
VariationPercentage float,
VariationType nvarchar(10),
UpperLimit float,
LowerLimit float,
constraint check_ParametersType check (ParametersType in ('Limits', 'RatedValueVariations')),
constraint check_VariationType check (VariationType in ('Above', 'Below', 'Both'))
)

--System Alarms
create table dbo.SystemAlarms
(
Id int identity primary key,
SystemAlarmPolicyId int foreign key references dbo.SystemAlarmPolicies(Id),
SystemAlarmParameterName nvarchar(50),
SystemFailureId int foreign key references dbo.SystemFailures(Id),
DeviceId nvarchar(50) foreign key references dbo.FieldDevices(Identifier),
DeviceLabel nvarchar(50),
FieldPointLabel nvarchar(50),
Title nvarchar(50),
Message nvarchar(max),
State nvarchar(30),
RaisedTimeStamp datetime,
AcknowledgedTimeStamp datetime,
Type nvarchar(20),
TimeStamp datetime,
constraint check_State check(State in ('Raised', 'Acknowledged', 'Resolved')),
constraint check_Type check(Type in ('Process', 'System')),
)
