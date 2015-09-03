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

namespace OpenRA.Mods.Common.Traits.Render
{
	[Desc("Used for tesla coil and obelisk.")]
	public class RenderSupportPowerInfo : RenderBuildingInfo
	{
		[Desc("Sequence to use for support power activation animation.")]
		[SequenceReference] public readonly string ActiveSequence = "active";

		[Desc("Sequence to use for support power charge animation.")]
		[SequenceReference] public readonly string ChargeSequence = null;

		public override object Create(ActorInitializer init) { return new RenderSupportPower(init, this); }
	}

	public class RenderSupportPower : RenderBuilding, INotifySupportPowerStuff
	{
		readonly RenderSupportPowerInfo info;

		public RenderSupportPower(ActorInitializer init, RenderSupportPowerInfo info)
			: base(init, info)
		{
			this.info = info;
		}

		public void Active(Actor self, bool ascending = false)
		{
			PlayCustomAnim(self, info.ActiveSequence);
		}

		public void Charging(Actor self)
		{
			if (info.ChargeSequence != null)
				PlayCustomAnim(self, info.ChargeSequence);
		}
	}
}
