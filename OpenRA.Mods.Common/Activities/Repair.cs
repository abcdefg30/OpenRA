#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Activities
{
	public class Repair : Activity
	{
		readonly RepairsUnitsInfo repairsUnits;
		readonly Actor host;
		int remainingTicks;
		Health health;
		bool initialized = false;

		public Repair(Actor host)
		{
			this.host = host;
			repairsUnits = host.Info.Traits.Get<RepairsUnitsInfo>();
		}

		public override Activity Tick(Actor self)
		{
			if (IsCanceled || host == null || !host.IsInWorld)
			{
				foreach (var depot in host.TraitsImplementing<INotifyRepair>())
					depot.FinishRepairing(self, host);

				return NextActivity;
			}

			health = self.TraitOrDefault<Health>();
			if (health == null)
				return NextActivity;

			if (health.DamageState == DamageState.Undamaged)
			{
				Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", repairsUnits.FinishRepairingNotification, self.Owner.Faction.InternalName);

				foreach (var depot in host.TraitsImplementing<INotifyRepair>())
					depot.FinishRepairing(self, host);

				return NextActivity;
			}

			if (remainingTicks == 0)
			{
				if (!initialized)
				{
					initialized = true;
					Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", repairsUnits.StartRepairingNotification, self.Owner.Faction.InternalName);

					foreach (var depot in host.TraitsImplementing<INotifyRepair>())
						depot.StartRepairing(self, host);

					return this;
				}

				var unitCost = self.Info.Traits.Get<ValuedInfo>().Cost;
				var hpToRepair = repairsUnits.HpPerStep;
				var cost = Math.Max(1, (hpToRepair * unitCost * repairsUnits.ValuePercentage) / (health.MaxHP * 100));

				if (!self.Owner.PlayerActor.Trait<PlayerResources>().TakeCash(cost))
				{
					remainingTicks = 1;
					return this;
				}

				self.InflictDamage(self, -hpToRepair, null);

				foreach (var depot in host.TraitsImplementing<INotifyRepair>())
					depot.Repairing(self, host);

				remainingTicks = repairsUnits.Interval;
			}
			else
				--remainingTicks;

			return this;
		}
	}
}
