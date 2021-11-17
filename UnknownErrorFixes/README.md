# Unknown Error Fixes

If you've never gotten these errors then there's no point in downloading this mod.

A bunch of errors I have no idea how it could be fixed. Feel free to send me more shit to add nullchecks to.

## ProjectileSteerTowardsTarget
> [Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
Stack trace:
RoR2.Projectile.ProjectileSteerTowardTarget.FixedUpdate () (at <da7c19fa62814b28bdb8f3a9223868e1>:IL_0000)

Probably an error with Annihilator/Echo Elites. Error comes from `if (targetComponent.target)` on the first line, except it doesn't check if `targetComponent` exists, which is doesn't, then floods console with the nullref. Which is weird, since the component is supposed to exist on the projectile? Attempting fix by adding a nullcheck for `targetComponent`.