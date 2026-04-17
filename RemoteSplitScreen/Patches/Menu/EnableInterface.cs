using HarmonyLib;
using StardewValley;
using StardewValley.Menus;

namespace RemoteSplitScreen.Patches.Menu;

[HarmonyPatch(typeof(Game1), nameof(Game1.ShowLocalCoopJoinMenu))]
public class EnableInterface {
	[HarmonyPrefix]
	private static bool Prefix(Game1 __instance, ref bool __result) {
		Game1.playSound("bigSelect");
		Game1.activeClickableMenu = (IClickableMenu) new LocalCoopJoinMenu();
		
		__result = true;
		
		return true;
	}
}
