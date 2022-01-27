
/* Gets the Recipe Block Execution Info between given Timestamps */
create procedure dbo.GetRecipeBlockExecutionInfo
@FieldDeviceIdentifier nvarchar(50),
@StartTime nvarchar(20),
@EndTime nvarchar(20)
as
begin
	set nocount on;
	select * from dbo.RecipeBlockExecutionInfo where FieldDeviceIdentifier=@FieldDeviceIdentifier and TimeStamp between @StartTime and @EndTime order by TimeStamp asc;
end
go

/* Add Recipe Block Execution Info */
create procedure dbo.AddRecipeBlockExecutionInfo
@FieldDeviceIdentifier nvarchar(50),
@StartTime nvarchar(20),
@EndTime nvarchar(20),
@Duration nvarchar(20),
@ExecutionMessage nvarchar(max)
as
begin
	set nocount on;
	insert into dbo.RecipeBlockExecutionInfo(FieldDeviceIdentifier, StartTime, EndTime, Duration, ExecutionMessage, TimeStamp)
	values(@FieldDeviceIdentifier, @StartTime, @EndTime, @Duration, @ExecutionMessage, GETDATE());
end
go