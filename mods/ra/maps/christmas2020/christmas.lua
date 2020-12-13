
WorldLoaded = function()
	Neutral = Player.GetPlayer("Neutral")

	Camera.Position = Center.CenterPosition

	local santa = Actor.Create("santa", true, { Owner = Neutral, Location = Spawn.Location, Facing = Angle.West, CenterPosition = Spawn.CenterPosition + WVec.New(0, 0, Actor.CruiseAltitude("santa")) })

	local lz = Drop.Location
    Utils.Do({ "crate", "crate", "crate", "crate", "crate" }, function(type)
        local a = Actor.Create(type, false, { Owner = Neutral })
        santa.LoadPassenger(a)
    end)

    santa.Paradrop(lz)

	Trigger.AfterDelay(DateTime.Seconds(14), AfterDrop)

	lightSourcesA[1] = Actor.Create("INREDLMP", false, { Owner = Neutral, Location = TeslaA.Location })
	lightSourcesA[2] = Actor.Create("INBLULMP", false, { Owner = Neutral, Location = TeslaA.Location })
	lightSourcesA[3] = Actor.Create("INYELWLAMP", false, { Owner = Neutral, Location = TeslaA.Location })
	lightSourcesA[4] = Actor.Create("INGRNLMP", false, { Owner = Neutral, Location = TeslaA.Location })
	lightSourcesB[1] = Actor.Create("INREDLMP", false, { Owner = Neutral, Location = TeslaB.Location })
	lightSourcesB[2] = Actor.Create("INBLULMP", false, { Owner = Neutral, Location = TeslaB.Location })
	lightSourcesB[3] = Actor.Create("INYELWLAMP", false, { Owner = Neutral, Location = TeslaB.Location })
	lightSourcesB[4] = Actor.Create("INGRNLMP", false, { Owner = Neutral, Location = TeslaB.Location })

	Charge()

	Trigger.AfterDelay(DateTime.Seconds(0), function()
		GrenA.Attack(GrenB, true, true)
		GrenB.Attack(GrenA, true, true)
	end)
end

lightSourcesA = {}
lightSourcesB = {}
lastA = 0
lastB = 0
count = -1
Charge = function()
	if count ~= -1 then
		TeslaA.RevokeCondition(lastA)
		TeslaB.RevokeCondition(lastB)

		lightSourcesA[count + 1].IsInWorld = false
		lightSourcesB[count + 1].IsInWorld = false
	end

	count = (count + 1) % 4

	lastA = TeslaA.GrantCondition("con" .. (count + 1))
	lastB = TeslaB.GrantCondition("con" .. (count + 1))

	lightSourcesA[count + 1].IsInWorld = true
	lightSourcesB[count + 1].IsInWorld = true

	Trigger.AfterDelay(35, Charge)
end

AfterDrop = function()
	Trigger.AfterDelay(DateTime.Seconds(2), function()
		Actor.Create("smallyellowlight", true, { Owner = Neutral, Location = Building1.Location })
	end)
	Trigger.AfterDelay(DateTime.Seconds(4), function()
		Actor.Create("smallyellowlight", true, { Owner = Neutral, Location = Building3.Location + CVec.New(1, 1) })
	end)
	Trigger.AfterDelay(DateTime.Seconds(7), function()
		Actor.Create("smallyellowlight", true, { Owner = Neutral, Location = Building2.Location + CVec.New(1, 1) })
	end)
	Trigger.AfterDelay(DateTime.Seconds(8), function()
		Actor.Create("smallyellowlight", true, { Owner = Neutral, Location = Building4.Location })
	end)
end
