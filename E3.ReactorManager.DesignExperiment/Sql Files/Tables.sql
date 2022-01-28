
/* Batch Table */
create table [dbo].[BatchTable]
(
Identifier	nvarchar(50) not null primary key,
Name	nvarchar(MAX),
Number	nvarchar(MAX),
Stage nvarchar(20),
ScientistName	nvarchar(MAX),
FieldDeviceIdentifier nvarchar(50),
FieldDeviceLabel nvarchar(50),
HCIdentifier	nvarchar(50),
StirrerIdentifier	nvarchar(50),
DosingPumpUsage	nvarchar(10),
ChemicalDatabaseIdentifier	nvarchar(50),
ImageOfReaction	image,
Comments	nvarchar(MAX),
State	nvarchar(50),
TimeStarted	datetime,
TimeCompleted	datetime,
CleanedBy nvarchar(max),
CleaningSolvent nvarchar(max),
)
