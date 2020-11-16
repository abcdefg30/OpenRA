#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;

namespace OpenRA.Mods.Common.UpdateRules.Rules
{
	public class RemoveGuard : UpdateRule
	{
		public override string Name { get { return "The 'Guard' trait was removed."; } }
		public override string Description
		{
			get
			{
				return "The 'Guard' trait was removed. Its functionality was moved to the Attack* traits.";
			}
		}

		readonly string[] attackTraits =
		{
			"AttackTesla", "AttackBomber", "AttackFollow", "AttackFrontal", "AttackOmni", "AttackAircraft",
			"AttackGarrisoned", "AttackTurreted", "AttackSwallow", "AttackLeap", "AttackCharges"
		};

		public override IEnumerable<string> UpdateActorNode(ModData modData, MiniYamlNode actorNode)
		{
			var guard = actorNode.LastChildMatching("Guard", false);
			var voice = guard?.LastChildMatching("Voice");
			foreach (var attack in attackTraits)
			{
				var attackNode = actorNode.LastChildMatching(attack, includeRemovals: false);
				if (attackNode == null)
					continue;

				if (guard == null || guard.IsRemoval())
				{
					attackNode.AddNode("CanGuard", "false");
					if (guard == null)
						yield return "No Guard node found on actor '{0}'. Preemptively adding 'CanGuard: false'. " +
							"Please check if manual adjustments are needed. ({1})".F(actorNode.Key, attackNode.Location);
				}
				else if (voice != null)
					attackNode.AddNode("GuardVoice", voice.Value.Value);
			}

			actorNode.RemoveNode(guard);
			yield return "Removed Guard from actor '{0}' (at {1}). Please check if manual adjustments are needed.".F(actorNode.Key, guard.Location);
		}
	}
}
