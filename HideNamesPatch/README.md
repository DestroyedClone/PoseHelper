# Hide Names Patch

***Original Mod: ["Hide Names"](https://thunderstore.io/package/Moffein/Hide_Names/) by [Moffein](https://thunderstore.io/package/Moffein/)***

Make everyone's name the same, or replaced by their survivor name. Config lets you change what peoples' names are replaced by. Mostly untested. 

* **Default Name:**
	* (empty) - Defaults to their Survivor name. (Commando, Huntress, Captain)
	* "Skin" - Shows their skin name, formatted by the associated command.
	* (any other text) - replaces with this instead
* **Body Fallback Name:** Displays this name when it can't replace the default name, usually on player join/disconnect messages.
* **Skin Fallback Name**: Ditto, but with skins.
* **Skin Name Formatting**: When "Default Name" is set to "Skin", the user can customize how it's displayed.
	* 0: Body Name, 1: Skin Name
	* "{1} {0}": Default Commando, Hornet Commando, Arctic Huntress, Admiral Captain
* **Default Skin Name Override:** If you would prefer the default skin to be a different name.
* **Show Host:** Appends a string after the name of the hoster. 
	* {0} = Original Name Override. Defaults to "{0} (string)" if the {0} is missing.
	* Leave empty to disable.
	* Example: "{0} (Hoster)" = "Player (Hoster)" | "(Hoster)" = "Player (Hoster)"
	* "(Host) {0}" = "(Host) Player"

Below is an example of a modified config.

**Example 1**
![enter image description here](https://media.discordapp.net/attachments/849798075001864214/892684191446216704/unknown.png)
 * Default Name: Skin
 * Fallback Body Name: Player
 * Fallback Skin Name: Skin 0
 * Skin Name Formatting : "<style=cIsDamage>{1}</style> {0}"
	 * If you want it to be formatted properly when you chat, use my mod "[Rich Text Chat](https://thunderstore.io/package/DestroyedClone/RichTextChat/)" which removes the \<noparse> tag from chat messages.
 * Default Skin Name Override: Non-Mastered

**Example 2: With [Rich Text Chat](https://thunderstore.io/package/DestroyedClone/RichTextChat/)**

![preview](https://media.discordapp.net/attachments/891018412464177162/895177363657220096/unknown.png)
* Default Name: Skin
* Skin Name Formatting: "<style=cIsDamage>{0}</style>, <size=80%>the <style=cIsUtility>{1}</style></size>"

**Example 3: Host**

![](https://cdn.discordapp.com/attachments/887600433755987978/896519722219237376/unknown.png)



There will be quirks between the client and server, but it should work.

## Installation
Place the .dll in /Risk of Rain 2/BepInEx/plugins/

## Changelog

`1.1.1`
 - Added experimental option to show who's the host.
 - Fixed nullref in lobby

`1.1.0`
- Added option to display their skin name, with formatting
- Frequency of name update reduced from every second to every 3 seconds.
- Steam name in lobby creation is now replaced with either your overridden name or your fallback body name.

`1.0.0`
- Release