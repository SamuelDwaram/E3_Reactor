
/* Gets all batches*/
create procedure [dbo].[GetAllBatches]
as
begin
	set nocount on;
	select * from [dbo].BatchTable where State = 'Completed'
end
go

/* Gets all Running Batches*/
create procedure [dbo].[GetAllRunningBatches]
as
begin
	set nocount on;
	select * from [dbo].[BatchTable] where State = 'Running'
end
go

/* Add new batch*/
create procedure [dbo].[AddBatch]
@Identifier nvarchar(max),
@Name nvarchar(50),
@Number nvarchar(50),
@Stage nvarchar(10),
@ScientistName nvarchar(50),
@FieldDeviceIdentifier nvarchar(50),
@FieldDeviceLabel nvarchar(50),
@HCIdentifier nvarchar(50),
@StirrerIdentifier nvarchar(50),
@DosingPumpUsage nvarchar(10),
@ChemicalDatabaseIdentifier nvarchar(50),
@ImageOfReaction varbinary,
@Comments nvarchar(max),
@State nvarchar(50),
@TimeStarted datetime
as
begin
	set nocount on;
	insert into [dbo].BatchTable(Identifier, Name, Number, Stage, ScientistName, FieldDeviceIdentifier, FieldDeviceLabel, HCIdentifier, StirrerIdentifier, DosingPumpUsage, ChemicalDatabaseIdentifier, ImageOfReaction, Comments, State, TimeStarted)
	values(@Identifier, @Name, @Number, @Stage, @ScientistName, @FieldDeviceIdentifier, @FieldDeviceLabel, @HCIdentifier, @StirrerIdentifier, @DosingPumpUsage, @ChemicalDatabaseIdentifier, @ImageOfReaction, @Comments, @State, @TimeStarted);
end
go

/* End batch and update cleaning status */
create procedure [dbo].[EndBatchAndUpdateCleaningStatus]
@BatchIdentifier nvarchar(max),
@CleanedBy nvarchar(max),
@CleaningSolvent nvarchar(max)
as
begin
	set nocount on;
	update dbo.BatchTable set CleanedBy=@CleanedBy, CleaningSolvent=@CleaningSolvent, State = 'Completed', TimeCompleted = CURRENT_TIMESTAMP where Identifier = @BatchIdentifier;
end
go

/* Gets the batch details with the given Name*/
create procedure [dbo].[GetBatchDetails]
@Name as nvarchar(50)
as
begin
	set nocount on;
	select * from [dbo].BatchTable where Name = @Name
end
go

/* Gets the batch info with the given identifier */
create procedure [dbo].[GetBatchInfo]
@Identifier as nvarchar(50)
as
begin
	set nocount on;
	select * from [dbo].BatchTable where Identifier = @Identifier;
end
go

/* Gets the batch running in FieldDevice */
create procedure [dbo].[GetBatchRunningInDevice]
@FieldDeviceIdentifier as nvarchar(50)
as
begin
	set nocount on;
	select * from [dbo].BatchTable where State='Running' and FieldDeviceIdentifier = @FieldDeviceIdentifier;
end
go