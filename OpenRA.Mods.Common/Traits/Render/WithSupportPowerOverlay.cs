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
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	[Desc(".")]
	public class WithSupportPowerOverlayInfo : ITraitInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Sequence name to use when active.")]
		[SequenceReference] public readonly string Sequence = "active";

		[Desc("Sequence name to use when charging.")]
		[SequenceReference] public readonly string ChargeSequence = null;

		[Desc("Custom palette name.")]
		[PaletteReference] public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName.")]
		public readonly bool IsPlayerPalette = false;

		public object Create(ActorInitializer init) { return new WithSupportPowerOverlay(init, this); }
	}

	public class WithSupportPowerOverlay : INotifySupportPowerStuff, ITick, INotifyDamageStateChanged, IRender
	{
		readonly Animation overlayActive;
		readonly Animation overlayCharging;
		readonly RenderSprites renderSprites;
		readonly WithSupportPowerOverlayInfo info;

		bool ascending;
		bool active;
		bool charging;
		WPos pos;

		public WithSupportPowerOverlay(ActorInitializer init, WithSupportPowerOverlayInfo info)
		{
			this.info = info;
			renderSprites = init.Self.Trait<RenderSprites>();

			overlayActive = new Animation(init.World, renderSprites.GetImage(init.Self));
			overlayCharging = new Animation(init.World, renderSprites.GetImage(init.Self));
			renderSprites.Add(new AnimationWithOffset(overlayActive, null, () => !active),
				info.Palette, info.IsPlayerPalette);
			renderSprites.Add(new AnimationWithOffset(overlayCharging, null, () => !charging),
				info.Palette, info.IsPlayerPalette);

			// HACK: Don't crash because of the Render method
			overlayActive.PlayRepeating(info.Sequence);
		}

		public void Active(Actor self, bool ascending = false)
		{
			this.ascending = ascending;
			if (ascending)
				pos = self.CenterPosition;
			else
			{
				active = true;
				overlayActive.PlayThen(RenderSprites.NormalizeSequence(overlayActive, self.GetDamageState(), info.Sequence), () => active = false);
			}
		}

		public void Charging(Actor self)
		{
			if (info.ChargeSequence == null)
				return;

			charging = true;
			overlayCharging.PlayThen(RenderSprites.NormalizeSequence(overlayCharging, self.GetDamageState(), info.ChargeSequence), () => charging = false);
		}

		public void Tick(Actor self)
		{
			if (!ascending)
				return;

			overlayActive.Tick();
			pos += new WVec(0, 0, 427);

			if (pos.Z > pos.Y)
				ascending = false;
		}

		public void DamageStateChanged(Actor self, AttackInfo e)
		{
			overlayActive.ReplaceAnim(RenderSprites.NormalizeSequence(overlayActive, e.DamageState, info.Sequence));
			overlayCharging.ReplaceAnim(RenderSprites.NormalizeSequence(overlayActive, e.DamageState, info.Sequence));
		}

		public IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
		{
			return overlayActive.Render(pos, wr.Palette(info.IsPlayerPalette ? info.Palette + self.Owner.InternalName : info.Palette));
		}
	}
}
