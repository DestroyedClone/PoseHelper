# Original Jellyfish

Emulates the jellyfish from Risk of Rain 1. Jellyfish have a relatively low amount of damage: (5, +1/level) which is supposed to scale with their normal nuking suicide. Beetles scale better:  (12, +2.4/level). So I scaled up their damage, which lets them deal between 2-6 damage very early (Commando has 110 health).

Also included is an experimental localization for configs for Spanish, Japanese, and Russian. As long as the game is launched in the selected language with no config file (deleted or otherwise), then it will replace the names and descriptions with ones for the defined language. It doesn't support hotswapping, though.

🔧 = Configurable

**Body**

 - 🔧 Hitstun and stun immunity
 - 🔧 Now passes through walls
 - 🔧 Knockback immunity
 - 🔧 Lorebook entry overwritten with RoR1's lore entry
	 - EN, ES, JP, RU translations included
 - 🔧 Base Damage: 5 → 10
	 - 🔧 Per Level: +1 → +1.5
 - Deathstate slightly modified to reduce errors in console.

**Attack**

 - Rename: Nova -> Discharge
 - Base Duration: 3s → 0.25s
 - Recharge rate: 5s → 1s
 - 🔧Damage: 1000% → 100%
 - Proc Coefficient: 200% → 100%
 - Radius: Decreased by 20%
 - No longer deals knockback
 - No longer suicides on use.

### ToDo

* Make Jellyfish move straight towards enemy
* Change icon to RoR1
* Adjust visuals

## Changelog

* 1.1.0 - Fixes + Config
	* 🛠 Fixed TILER2 error by adding a default language to the configuration setup method.
	* 🔧Config options added for: Hitstun immunity, Stun immunity, knockback immunity, and collision toggle.
	* Added localization for new configs
	* ⚔️Adjusted Level Damage default to be 20% of Base Damage, scaling like other monsters.
* 1.0.0 - Release

> Written with [StackEdit](https://stackedit.io/).
> Translations made with [DeepL](https://www.deepl.com/en/translator).