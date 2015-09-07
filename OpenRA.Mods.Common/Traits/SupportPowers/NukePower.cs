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
using System.Linq;
using OpenRA.Effects;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;


namespace OpenRA.Mods.Common.Traits
{
	class NukePowerInfo : SupportPowerInfo, Requires<IBodyOrientationInfo>
	{
		[WeaponReference]
		public readonly string MissileWeapon = "";

		// TODO: Remove
		public readonly WVec SpawnOffset = WVec.Zero;

		[Desc("Travel time - split equally between ascent and descent")]
		public readonly int FlightDelay = 400;

		// TODO: Remove
		[Desc("Visual ascent velocity in WDist / tick")]
		public readonly WDist FlightVelocity = new WDist(512);

		// TODO: Should be removed
		[Desc("Descend immediately on the target, with half the FlightDelay")]
		public readonly bool SkipAscent = false;

		[Desc("Amount of time before detonation to remove the beacon")]
		public readonly int BeaconRemoveAdvance = 25;

		[ActorReference]
		[Desc("Actor to spawn before detonation")]
		public readonly string CameraActor = null;

		[Desc("Amount of time before detonation to spawn the camera")]
		public readonly int CameraSpawnAdvance = 25;

		[Desc("Amount of time after detonation to remove the camera")]
		public readonly int CameraRemoveDelay = 25;

		public readonly string FlashType = null;

		public override object Create(ActorInitializer init) { return new NukePower(init.Self, this); }
	}

	class NukePower : SupportPower, ITick
	{
		readonly NukePowerInfo info;
		readonly IBodyOrientation body;

		bool active;
		int ticks;
		Player firedBy;
		WPos targetPosition;

		public NukePower(Actor self, NukePowerInfo info)
			: base(self, info)
		{
			body = self.Trait<IBodyOrientation>();
			this.info = info;
		}

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);

			firedBy = self.Owner;
			ticks = 0;
			active = true;

			if (self.Owner.IsAlliedWith(self.World.RenderPlayer))
				Sound.Play(Info.LaunchSound);
			else
				Sound.Play(Info.IncomingSound);

			foreach (var sa in self.TraitsImplementing<INotifySupportPowerStuff>())
				sa.Active(self);

			targetPosition = self.World.Map.CenterOfCell(order.TargetLocation);
			foreach (var ml in self.TraitsImplementing<INotifyMissileLaunch>())
				ml.MissileLaunch(self, info.FlightDelay, targetPosition, info.SkipAscent);

			// Fire the nuke
			// TODO: info.SpawnOffset is not handled in MissileLaunch
			var launchPos = self.CenterPosition + body.LocalToWorld(info.SpawnOffset);
			var weaponRules = firedBy.World.Map.Rules.Weapons[info.MissileWeapon.ToLowerInvariant()];
			if (weaponRules.Report != null && weaponRules.Report.Any())
				Sound.Play(weaponRules.Report.Random(firedBy.World.SharedRandom), launchPos);

			if (info.SkipAscent)
				ticks = info.FlightDelay / 2;

			if (info.CameraActor != null)
			{
				var camera = self.World.CreateActor(false, info.CameraActor, new TypeDictionary
				{
					new LocationInit(order.TargetLocation),
					new OwnerInit(firedBy),
				});

				camera.QueueActivity(new Wait(info.CameraSpawnAdvance + info.CameraRemoveDelay));
				camera.QueueActivity(new RemoveSelf());

				Action addCamera = () => self.World.AddFrameEndTask(w => w.Add(camera));
				self.World.AddFrameEndTask(w => w.Add(new DelayedAction(info.FlightDelay - info.CameraSpawnAdvance, addCamera)));
			}

			if (Info.DisplayBeacon)
			{
				var beacon = new Beacon(
					order.Player,
					targetPosition,
					Info.BeaconPalettePrefix,
					Info.BeaconPoster,
					Info.BeaconPosterPalette,
					() => FractionComplete);

				Action removeBeacon = () => self.World.AddFrameEndTask(w =>
				{
					w.Remove(beacon);
					beacon = null;
				});

				self.World.AddFrameEndTask(w =>
				{
					w.Add(beacon);
					w.Add(new DelayedAction(info.FlightDelay - info.BeaconRemoveAdvance, removeBeacon));
				});
			}
		}

		public void Tick(Actor self)
		{
			if (!active)
				return;

			ticks++;

			if (ticks == info.FlightDelay)
			{
				active = false;
				Explode(self.World);
			}
		}

		void Explode(World world)
		{
			var weapon = world.Map.Rules.Weapons[info.MissileWeapon.ToLowerInvariant()];
			weapon.Impact(Target.FromPos(targetPosition), firedBy.PlayerActor, Enumerable.Empty<int>());
			world.WorldActor.Trait<ScreenShaker>().AddEffect(20, targetPosition, 5);

			foreach (var flash in world.WorldActor.TraitsImplementing<FlashPaletteEffect>())
				if (flash.Info.Type == info.FlashType)
					flash.Enable(-1);
		}

		public float FractionComplete { get { return ticks * 1f / info.FlightDelay; } }
	}
}
