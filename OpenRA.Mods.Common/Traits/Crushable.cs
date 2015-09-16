#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("This actor is crushable.")]
	class CrushableInfo : ITraitInfo
	{
		// TODO: Remove, sound is at weapon now
		[Desc("Sound to play when being crushed.")]
		public readonly string CrushSound = null;

		[Desc("Which crush classes does this actor belong to.")]
		public readonly HashSet<string> CrushClasses = new HashSet<string> { "infantry" };

		[Desc("Probability of mobile actors noticing and evading a crush attempt.")]
		public readonly int WarnProbability = 75;

		[Desc("Will friendly units just crush me instead of pathing around.")]
		public readonly bool CrushedByFriendlies = false;

		[WeaponReference, FieldLoader.Require, Desc("Weapon to use for crushing.")]
		public readonly string Weapon = "Crush";

		public object Create(ActorInitializer init) { return new Crushable(init.Self, this); }
	}

	class Crushable : ICrushable
	{
		readonly Actor self;
		readonly CrushableInfo info;
		readonly HealthInfo healthInfo;

		public Crushable(Actor self, CrushableInfo info)
		{
			this.self = self;
			this.info = info;
			healthInfo = self.Info.Traits.GetOrDefault<HealthInfo>();
		}

		public void WarnCrush(Actor crusher)
		{
			var mobile = self.TraitOrDefault<Mobile>();
			if (mobile != null && self.World.SharedRandom.Next(100) <= info.WarnProbability)
				mobile.Nudge(self, crusher, true);
		}

		public void OnCrush(Actor crusher)
		{
			if (healthInfo == null)
				return;

			Sound.Play(info.CrushSound, crusher.CenterPosition);
			var wda = self.TraitsImplementing<WithDeathAnimation>()
				.FirstOrDefault(s => s.Info.CrushedSequence != null);
			if (wda != null)
			{
				var palette = wda.Info.CrushedSequencePalette;
				if (wda.Info.CrushedPaletteIsPlayerPalette)
					palette += self.Owner.InternalName;

				wda.SpawnDeathAnimation(self, wda.Info.CrushedSequence, palette);
			}
			
			var weapon = crusher.World.Map.Rules.Weapons[info.Weapon.ToLowerInvariant()];
			if (weapon.Report != null && weapon.Report.Any())
				Sound.Play(weapon.Report.Random(crusher.World.SharedRandom), self.CenterPosition);

			// Use .FromPos since this actor is killed. Cannot use Target.FromActor
			//weapon.Impact(Target.FromPos(self.CenterPosition), crusher, Enumerable.Empty<int>());

			weapon.Impact(Target.FromActor(self), crusher, Enumerable.Empty<int>());
		}

		public bool CrushableBy(HashSet<string> crushClasses, Player crushOwner)
		{
			// Only make actor crushable if it is on the ground.
			if (!self.IsAtGroundLevel())
				return false;

			if (!info.CrushedByFriendlies && crushOwner.IsAlliedWith(self.Owner))
				return false;

			return info.CrushClasses.Overlaps(crushClasses);
		}
	}
}
