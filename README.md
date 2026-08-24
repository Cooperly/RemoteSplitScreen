# RemoteSplitScreen

A client-side mod that allows you to utilize split-screen in online multiplayer, even when you're not the host.

## Install
1. Install [SMAPI](https://smapi.io/).
2. Download RemoteSplitScreen and extract its contents into the Mods folder created within Stardew Valley's root directory.
3. Launch the game using SMAPI.

Or, alternatively, use [Vortex](https://www.nexusmods.com/vortex) to install this mod.

## Notes
- The host does not need this mod and will see any additional split-screen players as regular online farmhands with the same account identifier (or IP address for LAN) as the primary player.
  - Some hosts find this behaviour odd or malicious. Please refrain from using this mod if the host does not allow it.
- Every player in split-screen receives packets (and as such updates to the world and positions) separately due to how split-screen internally works. This includes packets from other split-screen players which have to go to the host and back.
  - This makes delays and sounds between split-screen player actions noticeable depending on your latency to the host.
  - This also means that network usage will be multiplied by the number of split-screen players, but this is not too relevant.
