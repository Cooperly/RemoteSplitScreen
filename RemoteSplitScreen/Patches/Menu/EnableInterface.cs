using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace RemoteSplitScreen.Patches.Menu;

[HarmonyPatch(typeof(Game1), nameof(Game1.ShowLocalCoopJoinMenu))]
public class EnableInterface {
	[HarmonyPrefix]
	private static bool Prefix(ref bool __result) {
		int free_farmhands = 0;
		
		Utility.ForEachLocation((Func<GameLocation, bool>) (location =>
		{
			if (location is Cabin cabin2 && (!cabin2.HasOwner || !cabin2.IsOwnerActivated))
				++free_farmhands;
			return true;
		}));
		
		// The two if statements below will cause the same if statements to run again in the original method if it hits.
		// This is negligible hopefully better than copying everything directly in my opinion.
		// - void
		
		if (!Game1.game1.IsMainInstance || GameRunner.instance.gameInstances.Count > GameRunner.instance.GetMaxSimultaneousPlayers()) {
			return false;
		}

		if (free_farmhands == 0) {
			Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:CoopMenu_NoSlots"));
			return false;
		}
		
		Game1.playSound("bigSelect");
		Game1.activeClickableMenu = (IClickableMenu) new LocalCoopJoinMenu();
		
		__result = true;
		
		return true;
	}
}
