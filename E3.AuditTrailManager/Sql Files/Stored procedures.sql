
/* Log the Audit Trail */
create procedure [dbo].[LogAuditTrail]
@Category nvarchar(50),
@AuditMessage nvarchar(max),
@ScientistName nvarchar(50)
as
begin
	set nocount on;
	insert into dbo.AuditTrail values(@Category, @AuditMessage, @ScientistName, CURRENT_TIMESTAMP);
end
go

/* Get the Audit Trail from the DB */
create procedure [dbo].[GetAuditTrail]
as
begin
	set nocount on;
	select * from [dbo].AuditTrail order by TimeStamp desc;
end
go

/* Get the Audit Trail between timestamp from the DB */
create procedure [dbo].[GetAuditTrailBetweenTimeStamps]
@StartTime datetime,
@EndTime datetime
as
begin
	set nocount on;
	select * from [dbo].AuditTrail where TimeStamp between @StartTime and @EndTime order by TimeStamp asc;
end
go
