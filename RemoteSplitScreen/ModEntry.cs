using HarmonyLib;
using StardewModdingAPI;

namespace RemoteSplitScreen;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod {
	private Harmony? harmony;
	public override void Entry(IModHelper helper) {
		// while (!Debugger.IsAttached) {
		// 	Console.WriteLine("Waiting for debugger...");
		// 	Thread.Sleep(100);
		// }
		//
		// Debugger.Break();
		
		harmony = new Harmony("club.freewifi.void.RemoteSplitScreen");
		
		harmony.PatchAll();
	}

	protected override void Dispose(bool disposing) {
		harmony?.UnpatchAll(harmony.Id);
	}
}
