using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace RemoteSplitScreen;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod {
	private Harmony? harmony;
	public override void Entry(IModHelper helper) {
		harmony = new Harmony("club.freewifi.void.RemoteSplitScreen");
		
		harmony.PatchAll();
	}

	protected override void Dispose(bool disposing) {
		harmony?.UnpatchAll(harmony.Id);
	}
}
