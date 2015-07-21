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
	[Desc("Replaces the building animation when it repairs a unit.")]
	public class WithRepairAnimationInfo : ITraitInfo, Requires<RenderBuildingInfo>
	{
		[Desc("Sequence name to use while repairing.")]
		[SequenceReference] public readonly string Sequence = "active";

		[Desc("Sequence name to use when starting the repair activity.")]
		[SequenceReference] public readonly string StartSequence = null;

		[Desc("Sequence name to use when finishing the repair activity.")]
		[SequenceReference] public readonly string FinishSequence = null;

		public readonly bool PauseOnLowPower = false;

		public object Create(ActorInitializer init) { return new WithRepairAnimation(init.Self, this); }
	}

	public class WithRepairAnimation : INotifyBeingRepaired
	{
		readonly WithRepairAnimationInfo info;

		public WithRepairAnimation(Actor self, WithRepairAnimationInfo info)
		{
			this.info = info;
		}

		public void StartRepairing(Actor self, Actor host)
		{
			if (info.StartSequence == null)
				return;

			var building = host.TraitOrDefault<RenderBuilding>();
			if (building != null && !(info.PauseOnLowPower && self.IsDisabled()))
				building.PlayCustomAnim(host, info.StartSequence);
		}

		public void Repairing(Actor self, Actor host)
		{
			var building = host.TraitOrDefault<RenderBuilding>();
			if (building != null && !(info.PauseOnLowPower && self.IsDisabled()))
				building.PlayCustomAnim(host, info.Sequence);
		}

		public void FinishRepairing(Actor self, Actor host)
		{
			if (info.FinishSequence == null)
				return;

			var building = host.TraitOrDefault<RenderBuilding>();
			if (building != null && !(info.PauseOnLowPower && self.IsDisabled()))
				building.PlayCustomAnim(host, info.FinishSequence);
		}
	}
}