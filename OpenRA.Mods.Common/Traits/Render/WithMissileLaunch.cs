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
	[Desc("Renders a missile effect.")]
	public class WithMissileLaunchInfo : ITraitInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Sequence name to use when ascending.")]
		[SequenceReference] public readonly string AscendSequence = "missileUp";

		[Desc("Sequence name to use when descending.")]
		[SequenceReference] public readonly string DescendSequence = null;

		[Desc("Custom palette name.")]
		[PaletteReference] public readonly string Palette = "effect";

		[Desc("Custom palette is a player palette BaseName.")]
		public readonly bool IsPlayerPalette = false;

		// TODO: Maybe only get a 'z' for a WVec with x=0 and y=0 here
		public readonly WVec AscendVelocity = new WVec(0, 0, 427);
		public readonly WVec DescendVelocity = new WVec(0, 0, 427);

		[Desc("Delay (in ticks) after which the missile is launched.")]
		public readonly int LaunchDelay = 0;

		public object Create(ActorInitializer init) { return new WithMissileLaunch(init, this); }
	}

	public class WithMissileLaunch : INotifyMissileLaunch, ITick, INotifyDamageStateChanged, IRender
	{
		readonly Animation missile;
		readonly RenderSprites renderSprites;
		readonly WithMissileLaunchInfo info;

		bool active;
		int ticks;
		int turn;
		WPos pos;
		WPos targetPos;

		public WithMissileLaunch(ActorInitializer init, WithMissileLaunchInfo info)
		{
			this.info = info;
			renderSprites = init.Self.Trait<RenderSprites>();

			missile = new Animation(init.World, renderSprites.GetImage(init.Self));
			missile.PlayRepeating(info.AscendSequence);
		}

		public void MissileLaunch(Actor self, int flightDelay = 0, WPos targetPos = new WPos(), bool skipAscent = false)
		{
			Game.RunAfterDelay(info.LaunchDelay, () =>
			{
				missile.PlayRepeating(info.AscendSequence);
				active = true;
				turn = flightDelay / 2;
				ticks = skipAscent ? turn : 0;
				pos = self.CenterPosition;

				this.targetPos = targetPos;
			});
		}

		public void Tick(Actor self)
		{
			if (!active)
				return;

			missile.Tick();

			if (turn > 0 && info.DescendSequence != null)
			{
				if (ticks == turn)
				{
					// Missiles could handle x and y themselves
					var x = (info.AscendVelocity.X > 0 || info.DescendVelocity.X > 0) ? 0 : targetPos.X - pos.X;
					var y = (info.AscendVelocity.Y > 0 || info.DescendVelocity.Y > 0) ? 0 : targetPos.Y - pos.Y;
					pos += new WVec(x, y, info.DescendVelocity.Z * (turn - 1) - pos.Z);
					missile.PlayRepeating(info.DescendSequence);
				}

				// TODO: What did lerpquadratic?
				if (ticks <= turn)
					pos += info.AscendVelocity;
				else
					pos -= info.DescendVelocity;

				if (ticks == turn * 2)
					active = false;

				ticks++;
			}
			else
			{
				pos += info.AscendVelocity;

				if (pos.Z > pos.Y)
					active = false;
			}
		}

		public void DamageStateChanged(Actor self, AttackInfo e)
		{
			missile.ReplaceAnim(RenderSprites.NormalizeSequence(missile, e.DamageState, ticks <= turn ? info.AscendSequence : info.DescendSequence));
		}

		public IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
		{
			if (!active)
				yield break;

			foreach (var m in missile.Render(pos, wr.Palette(info.IsPlayerPalette ? info.Palette + self.Owner.InternalName : info.Palette)))
				yield return m;
		}
	}
}
