#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Effects;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Displays an overlay when the building is being repaired by the player.")]
	public class WithRepairOverlayInfo : ITraitInfo, Requires<RenderSpritesInfo>, Requires<IBodyOrientationInfo>
	{
		[Desc("Sequence name to use while repairing.")]
		[SequenceReference] public readonly string Sequence = "active";

		[Desc("Sequence name to use when starting the repair activity.")]
		[SequenceReference] public readonly string StartSequence = null;

		[Desc("Sequence name to use when finishing the repair activity.")]
		[SequenceReference] public readonly string FinishSequence = null;

		[Desc("Position relative to body")]
		public readonly WVec Offset = WVec.Zero;

		[Desc("Custom palette name")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		public readonly bool PauseOnLowPower = false;

		public object Create(ActorInitializer init) { return new WithRepairOverlay(init.Self, this); }
	}

	public class WithRepairOverlay : INotifyDamageStateChanged, INotifyBuildComplete, INotifySold, INotifyRepair
	{
		readonly Animation overlay;
		readonly WithRepairOverlayInfo info;
		bool buildComplete;

		public WithRepairOverlay(Actor self, WithRepairOverlayInfo info)
		{
			this.info = info;

			var rs = self.Trait<RenderSprites>();
			var body = self.Trait<IBodyOrientation>();

			// Always render instantly for units
			buildComplete = !self.HasTrait<Building>();
			overlay = new Animation(self.World, rs.GetImage(self));
			overlay.Play(info.Sequence);

			var anim = new AnimationWithOffset(overlay,
				() => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(self, self.Orientation))),
				() => !buildComplete,
				() => info.PauseOnLowPower && self.IsDisabled(),
				p => WithTurret.ZOffsetFromCenter(self, p, 1));

			rs.Add(anim, info.Palette, info.IsPlayerPalette);
		}

		public void BuildingComplete(Actor self)
		{
			self.World.AddFrameEndTask(w => w.Add(new DelayedAction(120, () =>
				buildComplete = true)));
		}

		public void Sold(Actor self) { }
		public void Selling(Actor self)
		{
			buildComplete = false;
		}

		public void DamageStateChanged(Actor self, AttackInfo e)
		{
			overlay.ReplaceAnim(RenderSprites.NormalizeSequence(overlay, e.DamageState, overlay.CurrentSequence.Name));
		}

		public void StartRepairing(Actor self, Actor host)
		{
			if (info.StartSequence != null)
				overlay.Play(info.StartSequence);
		}

		public void Repairing(Actor self, Actor host)
		{
			overlay.Play(info.Sequence);
		}

		public void FinishRepairing(Actor self, Actor host)
		{
			if (info.FinishSequence != null)
				overlay.Play(info.FinishSequence);
		}
	}
}