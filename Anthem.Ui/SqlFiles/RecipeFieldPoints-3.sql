delete from dbo.CommandPoints where CommandPoints.Label in (select Label from dbo.FieldPoints where TypeOfAddress='RecipeTag')
delete from dbo.FieldPoints where TypeOfAddress='RecipeTag'

insert into dbo.FieldPoints values('RecipeStatus', 'RecipeStatus', 'RecipeTag', 'RecipeTags.bRecipeStatus', 'bool', 'false', 'true', 'sensorDataSet_1', 'Reactor_1')
insert into dbo.FieldPoints values('RecipeEnded', 'RecipeEnded', 'RecipeTag', 'RecipeTags.bRecipeEnded', 'bool', 'false', 'true', 'sensorDataSet_1', 'Reactor_1')
insert into dbo.FieldPoints values('PauseRecipe', 'PauseRecipe', 'RecipeTag', 'RecipeTags.PauseRecipe', 'bool', 'false', 'true', 'sensorDataSet_1', 'Reactor_1')
insert into dbo.FieldPoints values('ClearRecipe', 'ClearRecipeStatus', 'RecipeTag', 'RecipeTags.bClearRecipe', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
insert into dbo.FieldPoints values('AbortRecipeStatus', 'AbortRecipeStatus', 'RecipeTag', 'RecipeTags.bAbortRecipe', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
--insert into dbo.FieldPoints values('NumberOfRecipeSteps', 'NumberOfRecipeSteps','RecipeTag', 'RecipeTags.NumberOfRecipeSteps', 'int', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')

declare @counter int
set @counter = 1
while (@counter <= 0)
begin
declare @c nvarchar(10)
set @c = convert(nvarchar, @counter)
	if @c = 0
	begin
	insert into dbo.FieldPoints values('HeatCoolModeSelection_0', 'Recipe_Start_HeatCoolModeSelection_0', 'RecipeTag', 'R1.RecipeStep[0].HeatCoolModeSelection', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolDeltaTemp_0', 'Recipe_Start_HeatCoolDeltaTemp_0', 'RecipeTag', 'R1.RecipeStep[0].HeatCoolDeltaTemp', 'int', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')

	insert into dbo.FieldPoints values('Started_0', 'Recipe_Start_Started_0', 'RecipeTag', 'R1.RecipeStep[0].Started', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('Ended_0', 'Recipe_Start_Ended_0', 'RecipeTag', 'R1.RecipeStep[0].Ended', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('StartedTime_0', 'Recipe_Start_StartedTime_0', 'RecipeTag', 'R1.RecipeStep[0].StartedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('EndedTime_0', 'Recipe_Start_EndedTime_0', 'RecipeTag', 'R1.RecipeStep[0].EndedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	end
	else
	begin
	-- HC block
	insert into dbo.FieldPoints values('HeatCoolEnabled_' + @c, 'Recipe_HeatCool_Enabled_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolEnabled', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolStarted_' + @c, 'Recipe_HeatCool_Started_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolStarted', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolEnded_' + @c, 'Recipe_HeatCool_Ended_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolEnded', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolStartedTime_' + @c, 'Recipe_HeatCool_StartedTime_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolStartedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolEndedTime_' + @c, 'Recipe_HeatCool_EndedTime_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolEndedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolSetPoint_' + @c, 'Recipe_HeatCool_SetPoint_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolSetPointExpression', 'int', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolDuration_' + @c, 'Recipe_HeatCool_Duration_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolDuration', 'int', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('HeatCoolOperatingMode_' + @c, 'Recipe_HeatCool_OperatingMode_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].HeatCoolOperatingMode', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	
	--Stirrer block
	insert into dbo.FieldPoints values('StirrerEnabled_' + @c, 'Recipe_Stirrer_Enabled_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].StirrerEnabled', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('StirrerStarted_' + @c, 'Recipe_Stirrer_Started_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].StirrerStarted', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('StirrerEnded_' + @c, 'Recipe_Stirrer_Ended_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].StirrerEnded', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('StirrerStartedTime_' + @c, 'Recipe_Stirrer_StartedTime_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].StirrerStartedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('StirrerEndedTime_' + @c, 'Recipe_Stirrer_EndedTime_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].StirrerEndedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('StirrerSetPoint_' + @c, 'Recipe_Stirrer_SetPoint_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].StirrerSetPointExpression', 'int', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')

	--Wait block
	insert into dbo.FieldPoints values('WaitEnabled_' + @c, 'Recipe_Wait_Enabled_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].WaitEnabled', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('WaitStarted_' + @c, 'Recipe_Wait_Started_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].WaitStarted', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('WaitEnded_' + @c, 'Recipe_Wait_Ended_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].WaitEnded', 'bool', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('WaitStartedTime_' + @c, 'Recipe_Wait_StartedTime_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].WaitStartedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('WaitEndedTime_' + @c, 'Recipe_Wait_EndedTime_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].WaitEndedTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('WaitDuration_' + @c, 'Recipe_Wait_Duration_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].WaitDuration', 'int', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	insert into dbo.FieldPoints values('WaitRemainingTime_' + @c, 'Recipe_Wait_RemainingTime_' + @c, 'RecipeTag', 'R1.RecipeStep['+@c+'].WaitRemainingTime', 'string', 'false', 'false', 'sensorDataSet_1', 'Reactor_1')
	end
set @counter = @counter + 1
end

insert into dbo.CommandPoints select Label, SensorDataSetIdentifier, FieldDeviceIdentifier from dbo.FieldPoints where TypeOfAddress='RecipeTag'
