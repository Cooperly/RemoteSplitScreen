using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using RemoteSplitScreen.Helpers;
using StardewValley;
using StardewValley.Menus;

namespace RemoteSplitScreen.Patches.Menu;

[HarmonyPatch(typeof(OptionsPage), MethodType.Constructor, typeof(int), typeof(int), typeof(int), typeof(int))]
public class EnableButton {
	private static IEnumerable<CodeInstruction> Transpiler(
		IEnumerable<CodeInstruction> instructions,
		ILGenerator generator
	) {
		var matcher = new CodeMatcher(instructions, generator);

		matcher.MatchStartForward(
			Matches.LoadsField(typeof(Game1), nameof(Game1.game1)),
			Matches.GetsProperty(() => default(InstanceGame)!.IsMainInstance),
			Matches.Branches(),
			Matches.LoadsField(typeof(Game1), nameof(Game1.game1)),
			Matches.Calls(typeof(Game1), nameof(Game1.IsLocalCoopJoinable)),
			Matches.Branches(),
			Matches.LoadsConstant(0),
			Matches.StoresLocal(0)
		);

		matcher.ThrowIfInvalid(nameof(EnableButton));

		matcher.RemoveInstructions(8);

		matcher.InsertAndAdvance(
			Instructions.LoadConstant(true),
			Instructions.StoreLocal(0)
		);
		
		return matcher.Instructions();
	}
}

// IL_0407: ldsfld       class StardewValley.Game1 StardewValley.Game1::game1
// IL_040c: callvirt     instance bool StardewValley.InstanceGame::get_IsMainInstance()
// IL_0411: brfalse.s    IL_041f

// IL_0413: ldsfld       class StardewValley.Game1 StardewValley.Game1::game1
// IL_0418: callvirt     instance bool StardewValley.Game1::IsLocalCoopJoinable()
// IL_041d: br.s         IL_0420
// IL_041f: ldc.i4.0
// IL_0420: stloc.0      // flag