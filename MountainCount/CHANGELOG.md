* `0.2.01` - Dependency Fix
	* Migrated MountainShrine reference to Assets
	* Added a 'ModifyShrineUseToken' abstract method to ShrineReferenceBase in order to isolate `Chat_SendBroadcastChat_ChatMessageBase` method from trying to access fields from unloaded mod dlls
	* Config default changes
* `0.2.0` - Mod Compat
	* Added support for Risk of Options.
	* Added support for ExtraChallengeShrines
	* Shrine information is now referenced via an inheritance of a ShrineReferenceBase class
	* Language support added
	* Individual shrine info can be requested by specifying the shrine (xwind? for ExtraChallengeMode shrines, xmtn? for Mountain shrine, and x? for all available shrines)
	* Replaced cfgIncrement with cfgExpandedInfo
	* Added estimated item count command

* `0.1.0` - Release