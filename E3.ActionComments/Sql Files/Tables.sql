/* Action Comments Table */
create table [dbo].[ActionCommentsTable]
(
FieldDeviceIdentifier nvarchar(50),
Comments nvarchar(max),
[User] nvarchar(50),
TimeStamp datetime
)