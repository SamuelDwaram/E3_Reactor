/* Equipments connected to field devices */
create table dbo.Equipments
(
EquipmentName nvarchar(20),
EquipmentModel nvarchar(20),
EquipmentType nvarchar(20),
FieldDeviceConnectedTo nvarchar(50) foreign key references dbo.FieldDevices(Identifier)
)
go
