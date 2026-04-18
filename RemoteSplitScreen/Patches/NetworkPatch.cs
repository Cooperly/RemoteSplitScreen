using System.Reflection.Emit;
using HarmonyLib;
using RemoteSplitScreen.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;

namespace RemoteSplitScreen.Patches;

[HarmonyPatch(typeof(Game1), nameof(Game1.UpdateTitleScreen))]
public class NetworkPatch {
	private static IEnumerable<CodeInstruction> Transpiler(
		IEnumerable<CodeInstruction> instructions,
		ILGenerator generator
	) {
		var matcher = new CodeMatcher(instructions, generator);

		matcher.MatchStartForward(
			Matches.LoadsConstant("localhost"),
			Matches.Constructs(typeof(LidgrenClient))
		);

		matcher.ThrowIfInvalid(nameof(NetworkPatch));

		matcher.RemoveInstructions(2);
		matcher.InsertAndAdvance(
			Instructions.Call(() => ReplaceConnectionTarget())
		);

		return matcher.Instructions();
	}

	private static Client ReplaceConnectionTarget() {
		if (!LobbyHandler.IsRemoteHost) {
			return new LidgrenClient("localhost");
		}

		if (LobbyHandler.ClientBuilder == null) {
			ModEntry.ModMonitor.Log($"Cannot determine platform (LAN/Steam/GOG/Hybrid) as ClientBuilder is null, but the current client is somehow not the host? Please report to the developer.", LogLevel.Error);
			throw new Exception();
		}
		
		return LobbyHandler.ClientBuilder.CreateClient();
	}
}

// IL_00cd: ldsfld       class StardewValley.Multiplayer StardewValley.Game1::multiplayer
// IL_00d2: ldstr        "localhost"
// IL_00d7: newobj       instance void StardewValley.Network.LidgrenClient::.ctor(string)
// IL_00dc: callvirt     instance class StardewValley.Network.Client StardewValley.Multiplayer::InitClient(class StardewValley.Network.Client)
// IL_00e1: newobj       instance void StardewValley.Menus.FarmhandMenu::.ctor(class StardewValley.Network.Client)
// IL_00e6: call         void StardewValley.Game1::set_activeClickableMenu(class StardewValley.Menus.IClickableMenu)
