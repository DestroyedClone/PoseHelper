# Boss Contribution Tracker

| [![github issues/request link](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/github_link.webp)](https://github.com/DestroyedClone/PoseHelper/issues) | [![discord invite](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/discord_link.webp)](https://discord.gg/DpHu3qXMHK) |
|--|--|

Inspired by Versus Saxton Hale, this mod prints the top 3 damage dealers to chat.

![enter image description here](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/BossDamageContribution/bdc_preview.webp)

## Configuration

 - **Owner Gets Minion Damage (true)**
	 - If true, then the damage dealt by minions will be attributed to the owner of those minions.
 - **Minion Shows Owner Name (false)**
	 - If true, then the result shown will show the owner of the minion after their name.
	 - Ex: Engineer Turret (TheEngi)
	 - This setting is incompatible if "Owner Gets Minion Damage" is true, since the minion won't be included in the list.
	 - ![enter image description here](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/BossDamageContribution/bdc_preview_minionname.webp)
 - **Top Damage Places (3)**
	 - The number of places available. There will be a last place which is accumulative of the rest.
		 - Ex: With 2 places, plrA dealt 1500, plrB dealt 500, plrC dealt 250, and plrD dealt 125"
		 - The result would look something like:
		 - 1: plrA (1500)
		 - 2: plrB (500)
		 - The Rest: (375)");

## To-Do
- Store the last used name of the CharacterMaster, so it doesn't default to ??? if the player's body is missing
- UI Element instead of chat
- Localize tokens
- Track self-damage (such as REX umbras and fall damage)
- Track damage done by master-less bodies (pots)
- Adjust damage tracking from health loss to actual damage taken
- Multiplayer Testing

> Written with [StackEdit](https://stackedit.io/).