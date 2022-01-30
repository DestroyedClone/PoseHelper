# Boss Drop Reward Delay

This mod adds a short delay between rewards from the teleporter drops. Since modding can increase the amount of drops to high values (such as 16 playercounts x 4 mountain shrines), the amount of drops that are produced becomes too much for the system to handle, either resulting in a long-lasting lagspike or a crash.

The configuration defaults to 0.3s, but the values can be adjusted by the user.

Console command: 
* `bossdrop_delay {seconds}` (Server)
	* Sets the delay to this amount. Useful for changing it midrun for when a larger item drop rate is safe to use.
	* Leave empty to show the current delay amount.


![preview](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/MountainShrineRewardFix/preview.gif)

## Credits
Moffein - IL code, consultation

## Changelog
`1.1.0` - Added a command that allows for mid-run adjustment of the delay

> Written with [StackEdit](https://stackedit.io/).