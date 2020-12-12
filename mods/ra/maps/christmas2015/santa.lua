
WorldLoaded = function()
	neutral = Player.GetPlayer("Neutral")

	Camera.Position = Drop.CenterPosition
	Actor.Create("camera", true, { Owner = Player.GetPlayer("Multi0"), Location = Drop.Location })

	--[[local offsets =
	{
		{ -2700, -100 },
		{ -1800, 100 },
		{ -1800, -200 },
		{ -1100, 100 },
		{ -1100, -200 }
	}
	local i = 0
	Utils.Do({ "flyingdogA", "flyingdog", "flyingdog" , "flyingdog", "flyingdog" }, function(t)
		i = i + 1
		local dog = Actor.Create(t, true, { Owner = neutral, Location = Spawn.Location, Facing = 64, CenterPosition = Spawn.CenterPosition + WVec.New(offsets[i][1], offsets[i][2], Actor.CruiseAltitude("santa")) })
		dog.Move(Leave.Location)
	end)]]

	--local i = 5
	--local dog = Actor.Create("flyingdog", true, { Owner = neutral, Location = Spawn.Location, Facing = 64, CenterPosition = Spawn.CenterPosition + WVec.New(offsets[i][1], offsets[i][2], Actor.CruiseAltitude("santa")) })
	--dog.Move(Leave.Location)

	local santa = Actor.Create("santa", true, { Owner = neutral, Location = Spawn.Location, Facing = Angle.West, CenterPosition = Spawn.CenterPosition + WVec.New(0, 0, Actor.CruiseAltitude("santa")) })

	local lz = Drop.Location
    Utils.Do({ "crate", "crate", "crate", "crate", "crate" }, function(type)
        local a = Actor.Create(type, false, { Owner = neutral })
        santa.LoadPassenger(a)
    end)

    santa.Paradrop(lz)
end
