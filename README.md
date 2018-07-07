# SpawnMobX [![Github Releases](https://img.shields.io/github/downloads/deadsurgeon42/SpawnMobX/total.svg)](https://github.com/deadsurgeon42/SpawnMobX)
🧟 *Spawn mobs with special properties!*

Perfect for usage with [Command-Timelines](https://github.com/Enerdy/Command-Timelines).

## Installation
Drop [the plugin](https://github.com/deadsurgeon42/SpawnMobX/releases) to your `ServerPlugins/` folder. That's all!

Give the `spawnmobx.use` permission to the groups you wish to give access to the command.

## The `/smx` command

In its basic form, its syntax is equal to `/sm`: `/smx <mob name/id> <amount>`

If `amount` is omitted, it's assumed to be `1`.

The special ability of `/smx` can be utilized by adding modifiers after the name or spawn count,
separated by spaces and delimited by the `=` sign:

`/smx Paladin 5 health=30000 ai0=6 ai1=4.4 ai2=4.1 ai3=1`

which spawns five paladins with the total health of 30000 (6 times the default) and customized AI parameters.

### Modifiers

⚠️ *Most modifiers added don't seem to work as expected, whether due to client-server
communication limitations or the incompetence of SpawnMobX's author.
They are implemented for possible compatibility in the future releases of Terraria.*

**`name`:** A nickname to give to the NPC. *May not work client side.*

--------------------

**`health`:** Maximum HP of the NPC. Limited to a certain multiple of the default HP.

**`regen`:** HP regen rate of the NPC. *May not work client side, or at all.*

**`target`:** ID of the player targeted by the NPC. *May not work client side, or at all.*

--------------------

**`x`:** The X tile coordinate (as reported by /pos) to spawn the NPC on.

**`y`:** The Y tile coordinate (as reported by /pos) to spawn the NPC on.

--------------------

**`wx`:** The X world coordinate (tile coordinate * 16) to spawn the NPC on.

**`wy`:** The Y world coordinate (tile coordinate * 16) to spawn the NPC on.

--------------------

**`ai0` to `ai3`:** A floating point number altering various behaviors of the NPC's AI. *May not work client side, or with "simpler" NPCs.*
