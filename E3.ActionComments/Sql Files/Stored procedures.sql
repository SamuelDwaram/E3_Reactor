
/* Logs Action Comments */
create procedure [dbo].[LogActionComments]
@FieldDeviceIdentifier nvarchar(50),
@User nvarchar(50),
@Comments nvarchar(max)
as
begin
	set nocount on;
	insert into dbo.ActionCommentsTable values(@FieldDeviceIdentifier, @Comments, @User, CURRENT_TIMESTAMP);
end
go

/* Reads the Action Comments for the field device*/
create procedure [dbo].[ReadActionComments]
@FieldDeviceIdentifier nvarchar(50),
@startTime nvarchar(25),
@endTime nvarchar(25)
as
begin
	set nocount on;
	select * from [dbo].ActionCommentsTable where FieldDeviceIdentifier = @FieldDeviceIdentifier and TimeStamp between @startTime and @endTime order by TimeStamp;
end
go

/* Reads the top 10 Action Comments for the field device according to time stamp */
create procedure [dbo].[GetLatestActionComments]
@FieldDeviceIdentifier nvarchar(50)
as
begin
	set nocount on;
	select top 10 * from [dbo].ActionCommentsTable where FieldDeviceIdentifier = @FieldDeviceIdentifier order by TimeStamp desc;
end
go
