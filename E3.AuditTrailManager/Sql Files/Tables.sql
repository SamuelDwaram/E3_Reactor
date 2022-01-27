
/* Audit Trail */
create table [dbo].[AuditTrail]
(
Category nvarchar(50),
AuditMessage nvarchar(MAX),
ScientistName nvarchar(MAX),
TimeStamp datetime
)