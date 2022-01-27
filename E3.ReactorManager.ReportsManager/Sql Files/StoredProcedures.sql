drop procedure if exists dbo.UploadPdf;
go
/* Upload the pdf to database */
create procedure [dbo].UploadPdf
@Identifier nvarchar(50),
@Name nvarchar(50),
@Content varbinary(max)
as
begin
	set nocount on;
	insert into dbo.PdfFiles values(@Identifier, @Name, @Content, CAST(GETDATE() as date));
end
go

drop procedure if exists dbo.GetPdfDetails;
go
/* Get the Pdf Details from the database */
create procedure [dbo].GetPdfDetails
@Identifier nvarchar(50)
as
begin
	set nocount on;
	select * from dbo.PdfFiles where Identifier = @Identifier;
end
go

drop procedure if exists dbo.GetAvailablePdfList;
go
/* Gets the Available Pdf List */
create procedure [dbo].GetAvailablePdfList
as
begin
	set nocount on;
	select Identifier, Name, ReviewDate from [dbo].PdfFiles;
end
go

drop function if exists dbo.IsDataRecordingForDevice;
go
create function dbo.IsDataRecordingForDevice(@deviceId nvarchar(50))
returns bit
as
begin
	if object_id(@deviceId, 'u') is not null 
		return 1;
	return 0;
end
go

drop procedure if exists dbo.GetDataRecordedDevices;
go
/*Get the Available Devices for which Data is being recorded*/
create procedure dbo.GetDataRecordedDevices
as
begin
	set nocount on;
	create table #AvailableDevices(DeviceId nvarchar(50), DeviceLabel nvarchar(50));
	insert into #AvailableDevices select Identifier, Label from dbo.FieldDevices where dbo.IsDataRecordingForDevice(Identifier) = 1;
	select * from #AvailableDevices
end
go
