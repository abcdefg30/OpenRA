#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	[Desc(".")]
	public class WithSupportPowerOverlayInfo : ITraitInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Sequence name to use when active.")]
		[SequenceReference] public readonly string ActiveSequence = "active";

		[Desc("Sequence name to use when charging.")]
		[SequenceReference] public readonly string ChargeSequence = null;

		[Desc("Custom palette name.")]
		[PaletteReference] public readonly string Palette = "effect";

		[Desc("Custom palette is a player palette BaseName.")]
		public readonly bool IsPlayerPalette = false;

		public object Create(ActorInitializer init) { return new WithSupportPowerOverlay(init, this); }
	}

	public class WithSupportPowerOverlay : INotifySupportPowerStuff, INotifyDamageStateChanged
	{
		readonly Animation overlayActive;
		readonly Animation overlayCharging;
		readonly RenderSprites renderSprites;
		readonly WithSupportPowerOverlayInfo info;

		bool active;
		bool charging;

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
		}

		public void Active(Actor self)
		{
			if (info.ActiveSequence == null)
				return;

			active = true;
			overlayActive.PlayThen(RenderSprites.NormalizeSequence(overlayActive, self.GetDamageState(), info.ActiveSequence), () => active = false);
		}

		public void Charging(Actor self)
		{
			if (info.ChargeSequence == null)
				return;

			charging = true;
			overlayCharging.PlayThen(RenderSprites.NormalizeSequence(overlayCharging, self.GetDamageState(), info.ChargeSequence), () => charging = false);
		}

		public void DamageStateChanged(Actor self, AttackInfo e)
		{
			overlayActive.ReplaceAnim(RenderSprites.NormalizeSequence(overlayActive, e.DamageState, info.ActiveSequence));
			overlayCharging.ReplaceAnim(RenderSprites.NormalizeSequence(overlayActive, e.DamageState, info.ActiveSequence));
		}
	}
}
