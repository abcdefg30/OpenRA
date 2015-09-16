#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Traits;

namespace OpenRA.Mods.Common.Warheads
{
	public class CrushWarhead : DamageWarhead
	{
		/*public override void DoImpact(WPos pos, Actor firedBy, IEnumerable<int> damageModifiers)
		{
			var world = firedBy.World;
			var range = Spread[0];
			var hitActors = world.FindActorsInCircle(pos, range);
			if (Spread.Length > 1 && Spread[1].Length > 0)
				hitActors = hitActors.Except(world.FindActorsInCircle(pos, Spread[1]));

			foreach (var victim in hitActors)
				DoImpact(victim, firedBy, damageModifiers);
		}*/

		public override void DoImpact(Actor victim, Actor firedBy, IEnumerable<int> damageModifiers)
		{
			var healthInfo = victim.Info.Traits.GetOrDefault<HealthInfo>();
			if (healthInfo == null)
				return;

			victim.InflictDamage(firedBy, healthInfo.HP, this);
		}
	}
}
