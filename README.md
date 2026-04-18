# RemoteSplitScreen

A client-side mod that allows you to utilize split-screen in online multiplayer, even when you're not the host.

## Install
1. Install [SMAPI](https://smapi.io/).
2. Download RemoteSplitScreen and extract its contents into the Mods folder created within Stardew Valley's root directory.
3. Launch the game using SMAPI.

Or, alternatively, use [Vortex](https://www.nexusmods.com/vortex) to install this mod.

## Usage
Doesn't differ from the base game:
1. Be connected to a remote host (LAN/Steam/GOG) that has cabin slots free or occupied by your account.
2. Go to Options and scroll down until you see Start Local Co-Op; click on it.
3. Press the options/menu key on your additional controller(s).
4. Create or select a farmhand.

This mod does not prevent using split-screen in a non-online save or when hosting.

## Notes
- The host does not need this mod and will see any additional split-screen players as regular online farmhands with the same account identifier (or IP address for LAN) as the primary player.
  - Some hosts do not like this behaviour or want a single account to take up multiple cabin slots. Please refrain from using this mod if the host does not allow it.
- Every player in split-screen receives packets (and as such updates to the world and positions) separately due to how split-screen internally works. This includes packets from other split-screen players which have to go to the host and back.
  - This makes delays between split-screen player actions noticeable when looking between screens or when actions play audio depending on your latency to the host.
  - This also means that network usage will be multiplied by the number of split-screen players, but this is not too relevant unless you are on a metered connection.
  - Improvements could possibly be made to sync local players first between all instances, but that gets very complicated and I don't want to mess with that, for now.
