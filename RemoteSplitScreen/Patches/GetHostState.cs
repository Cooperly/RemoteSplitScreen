using HarmonyLib;
using StardewValley.Network;

namespace RemoteSplitScreen.Patches;

[HarmonyPatch(typeof(GameServer), nameof(GameServer.initialize))]
public class GetHostState {
	[HarmonyPostfix]
	private static void Postfix() {
		LobbyHandler.IsRemoteHost = false;
	}
}
