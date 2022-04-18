drop table if exists dbo.SystemFailures
drop table if exists dbo.SystemFailurePolicies
go

create table dbo.SystemFailurePolicies(
Id int identity primary key,
DeviceId nvarchar(50) foreign key references dbo.FieldDevices(Identifier),
DeviceLabel nvarchar(50),
FailedResourceLabel nvarchar(50),
TargetValue nvarchar(10),
Title nvarchar(50),
Message nvarchar(max),
TroubleShootMessage nvarchar(max),
FailureResourceType nvarchar(20) check (FailureResourceType in ('Device', 'Controller')),
Status bit,
CreatedTimeStamp datetime)

create table dbo.SystemFailures(
Id int identity primary key,
SystemFailurePolicyId int foreign key references dbo.SystemFailurePolicies(Id),
DeviceId nvarchar(50) foreign key references dbo.FieldDevices(Identifier),
DeviceLabel nvarchar(50),
Title nvarchar(50),
TroubleShootMessage nvarchar(max),
FailedResourceLabel nvarchar(50),
FailureState nvarchar(20) check (FailureState in ('Raised', 'Acknowledged', 'Resolved')),
FailureType nvarchar(20) check(FailureType in ('System', 'Hardware', 'Communication')),
TimeStamp datetime,
RaisedTimeStamp datetime,
AcknowledgedTimeStamp datetime)

