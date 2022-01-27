/* Recipe Block Execution Info */
create table dbo.RecipeBlockExecutionInfo
(
FieldDeviceIdentifier nvarchar(50),
BatchIdentifier nvarchar(50),
StartTime nvarchar(20),
EndTime nvarchar(20),
Duration nvarchar(20),
ExecutionMessage nvarchar(max),
TimeStamp datetime
)
go
