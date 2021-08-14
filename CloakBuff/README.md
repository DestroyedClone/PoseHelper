
# CloakBuff
This mod aims to enhance the effects of cloaking in Risk of Rain 2 to become more useful than just "monsters can't see you".  If you felt that cloaked enemies hidden by Celestines or holding Old War Stealthkits were barely a threat, then this mod is for you.

Nearly all methods of homing attacks will become unable to target cloaked enemies. Of course, this applies to you as well, particularly for those Bandit players with phobias of Scavengers with Disposable Missile Launchers.

Make sure to check the config.

## Visual
* **Healthbar:** Hidden on cloaked targets
* **Mislead Pinging:** When pinging a cloaked target and there's another enemy on the other side, it will ping that other enemy instead.
* **Damage Numbers:** Hidden on cloaked targets
* **Umbra Effect:** The Umbra's distinct swirling particle effects vanish when cloaked
* **Stunned** and **Shocked** overhead effects no longer show up on cloaked targets
* **BossIndicator:** Hidden on cloaked targets

## Targeting
* **Items:** Ukulele, Unstable Tesla Coil, N'kuhana's Opinion, Little Disciple, Ceremonial Dagger, Mired Urn
* **Equipment:** Royal Capacitator
* **Survivors:** Engineer's Harpoons, Pressure Mines, and Spider Mines<sup>[1]</sup>; Huntress' Auto-Targeting and Glaive bouncing; Mercenary's Eviscerate.

*<sup>[1] So that it's still useful, if you land it on a cloaked target then it will explode. (config)</sup>*

## Extra
* **Shock Disrupts Cloak:** Targets that are shocked are unable to be cloaked. Celestine elites will briefly give them the cloak buff, which will get immediately removed.
* **Shock Pauses Celestine:** Shocked targets can't be given cloak at all from Celestine elites.
* **Enable Stuns/Shocks:** It's disabled by default as stuns are a very easy way to shut down enemies, but if you want it enabled I recommend `UmbraOnly`. Can be configured to both Survivors and Umbras, Umbras only, or neither.

## ToDo:
* Revision: Prevent pinging cloaked enemies entirely
* Revision: replace messy hook with IL
* MP testing
* Feature: If a pinged target cloaks, then the ping will vanish.

## Known Bugs:

* Some don't work w/ the mod's targeting filter: (Preon Accumulator, Razorwire, Acrid's Epidemic).
	* Disabled by default.
* Mired Urn still targets a cloaked enemy if they are the only target within range.
	* Enabled by default as I felt it was not a complete failure.
* Engineer's Pressure mines no longer prematurely explode if there is both a cloaked and non-cloaked target within range. 
	* Enabled by default, though if this is a dealbreaker you can disable it.
* Merc's Eviscerate fails to target *any* valid target so long as a cloaked target is within range. Otherwise it's fine. 
	* Disabled by default.

## Credits
Bubbet - IL Help [(Thunderstore)](https://thunderstore.io/package/Bubbet/) [(Github)](https://github.com/Bubbet)

# Changelog
* 1.1.0 - New Features
* 1.0.0 - Release

> Written with [StackEdit](https://stackedit.io/).