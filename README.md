# NeuroScope

NeuroScope is an Outer Wilds mod that provides real-time game context and actions to the AI Neuro-sama, making her more aware of what is happening in the game. The mod sends information about player status, ship status, dialogue, achievements, notifications, and more to Neuro using the [Neuro Game SDK](https://github.com/VedalAI/neuro-game-sdk).

## Features

- Allows Neuro to read the ship and player status
- Shares nomai writing, dialogue and dialogue options and allows Neuro to respond
- Notifies Neuro of achievements, notifications, ship log facts and signalscope scans
- Track player location

## Installation
1. Install the [Outer Wilds Mod Manager](https://outerwildsmods.com/mod-manager/)
2. Download the lastest release of the mod
3. In the Mod Manager select "Install from" -> "Zip File" and select the zip file of the mod
4. Start the game using the Mod Manager and set the websocket URL
5. Restart the game


## Configuration

The context and actions sent to Neuro can be configured in the ingame settings menu. The websocket URL can also be set here or via the environment variable `NEURO_SDK_WS_URL`.
