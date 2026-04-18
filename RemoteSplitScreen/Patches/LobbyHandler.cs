using System.Reflection.Emit;
using Galaxy.Api;
using HarmonyLib;
using RemoteSplitScreen.Helpers;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy;
using StardewValley.SDKs.Steam;
using Steamworks;

namespace RemoteSplitScreen.Patches;

[HarmonyPatch]
public class LobbyHandler {
	
	public static IClientBuilder? ClientBuilder;
	public static bool IsRemoteHost;
	
	[HarmonyPatch(typeof(CoopMenu), "<enterIPPressed>b__47_0")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler(
		IEnumerable<CodeInstruction> instructions,
		ILGenerator generator
	) {
		var matcher = new CodeMatcher(instructions, generator);

		matcher.MatchStartForward(
			Matches.LoadsArgument(1),
			Matches.Constructs(typeof(LidgrenClient))
		);

		matcher.ThrowIfInvalid(nameof(LobbyHandler));

		matcher.Advance(2);
		matcher.InsertAndAdvance(
			Instructions.LoadArgument(1),
			Instructions.Call(()=>SetLidgren(null!))
		);
		
		
		return matcher.Instructions();
	}

	[HarmonyPatch(typeof(SteamNetHelper), nameof(SteamNetHelper.CreateClientFromHybrid))]
	[HarmonyPostfix]
	private static void Postfix(ref Client? __result) {
		if (__result == null) {
			return;
		}

		switch (__result) {
			case SteamNetClient steamNetClient: {
				SetSteam(
					steamNetClient.SteamLobby == default 
						? null 
						: steamNetClient.SteamLobby,
					steamNetClient.GalaxyLobby
				);
				break;
			}

			case GalaxyNetClient galaxyNetClient: {
				SetGalaxy(galaxyNetClient.lobbyId);
				break;
			}
		}
	}

	[HarmonyPatch(typeof(GalaxyNetHelper), nameof(GalaxyNetHelper.createClient))]
	[HarmonyPrefix]
	private static void Prefix(GalaxyID lobby) {
		SetGalaxy(lobby);
	}
	
	private static void SetLidgren(string address) {
		ClientBuilder = new LidgrenClientBuilder {
			Address = address,
		};
		IsRemoteHost = true;
	}

	private static void SetGalaxy(GalaxyID galaxyID) {
		ClientBuilder = new GalaxyClientBuilder {
			LobbyID = galaxyID,
		};
		IsRemoteHost = true;
	}

	private static void SetSteam(CSteamID? steamID, GalaxyID? galaxyID) {
		ClientBuilder = new SteamClientBuilder {
			LobbyID = steamID,
			GalaxyID = galaxyID
		};
		IsRemoteHost = true;
	}

	public interface IClientBuilder {
		public Client CreateClient();
	}

	private readonly struct LidgrenClientBuilder : IClientBuilder {
		public required string Address { get; init; }

		public Client CreateClient() {
			return new LidgrenClient(Address);
		}
	}

	private readonly struct GalaxyClientBuilder : IClientBuilder {
		public required GalaxyID LobbyID { get; init; }

		public Client CreateClient() {
			return new GalaxyNetClient(LobbyID);
		}
	}

	private readonly struct SteamClientBuilder : IClientBuilder {
		public required CSteamID? LobbyID { get; init; }
		public required GalaxyID? GalaxyID { get; init; }

		public Client CreateClient() {
			if (GalaxyID != null) {
				return new SteamNetClient(GalaxyID);
			}

			if (LobbyID != null) {
				return new SteamNetClient(LobbyID.Value);
			}

			ModEntry.ModMonitor.Log("Both Steam Lobby ID and Galaxy ID are null! Please report to the developer.", LogLevel.Error);
			throw new Exception();
		}
	}
}

// IL_003a: ldarg.1      // address
// IL_003b: newobj       instance void StardewValley.Network.LidgrenClient::.ctor(string)
