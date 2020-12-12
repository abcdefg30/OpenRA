
WorldLoaded = function()
	Charge()

	Trigger.AfterDelay(DateTime.Seconds(0), function()
	--	Spy.Move(CPos.New(34, 40))
		GrenA.Attack(GrenB, true, true)
		GrenB.Attack(GrenA, true, true)
	end)
end

lastA = 0
lastB = 0
count = -1
Charge = function()
	count = count + 1

	if count ~= 0 then
		TeslaA.RevokeCondition(lastA)
		TeslaB.RevokeCondition(lastB)
	end

	lastA = TeslaA.GrantCondition("con" .. (count % 4 + 1))
	lastB = TeslaB.GrantCondition("con" .. (count % 4 + 1))
	Trigger.AfterDelay(35, Charge)
end

