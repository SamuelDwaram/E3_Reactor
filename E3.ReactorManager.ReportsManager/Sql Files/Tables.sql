
/* Pdf Files */
create table [dbo].PdfFiles
(
Identifier nvarchar(50) not null primary key,
Name nvarchar(50),
Content varbinary(max),
ReviewDate date
)
