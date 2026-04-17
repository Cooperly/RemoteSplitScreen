using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace RemoteSplitScreen.Patches.Menu;

[HarmonyPatch(typeof(Game1), nameof(Game1.ShowLocalCoopJoinMenu))]
public class EnableInterface {
	[HarmonyPrefix]
	private static bool Prefix(ref bool __result) {
		if (!Game1.game1.IsMainInstance || GameRunner.instance.gameInstances.Count > GameRunner.instance.GetMaxSimultaneousPlayers()) {
			return false;
		}
		
		Game1.playSound("bigSelect");
		Game1.activeClickableMenu = (IClickableMenu) new LocalCoopJoinMenu();
		
		__result = true;
		
		return true;
	}
}
