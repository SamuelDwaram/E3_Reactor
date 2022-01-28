
/* Return all the Equipments connected to a Field Device */
create procedure dbo.GetAllConnectedEquipments
@FieldDeviceIdentifier nvarchar(50)
as
begin
	set nocount on;
	select * from dbo.Equipments where FieldDeviceConnectedTo=@FieldDeviceIdentifier;
end
go

/* Add an Equipment to a Field Device */
create procedure dbo.AddEquipment
@EquipmentName nvarchar(20),
@EquipmentModel nvarchar(20),
@EquipmentType nvarchar(20),
@FieldDeviceConnectedTo nvarchar(50)
as
begin
	set nocount on;
	insert into dbo.Equipments(EquipmentName, EquipmentModel, EquipmentType, FieldDeviceConnectedTo) values(@EquipmentName, @EquipmentModel, @EquipmentType, @FieldDeviceConnectedTo);
end
go

/* Get all Equipments */
create procedure dbo.GetAllEquipments
as
begin
	set nocount on;
	select * from dbo.Equipments;
end
go