using BepInEx;
using System.Security.Permissions;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace UnknownErrorFixes
{
    [BepInPlugin("com.DestroyedClone.UnknownErrorFixes", "UnknownErrorFixes", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.Projectile.ProjectileSteerTowardTarget.FixedUpdate += ProjectileSteerTowardTarget_FixedUpdate;
            On.RoR2.AntiGravityForce.FixedUpdate += AntiGravityForce_FixedUpdate;
        }

        private void AntiGravityForce_FixedUpdate(On.RoR2.AntiGravityForce.orig_FixedUpdate orig, RoR2.AntiGravityForce self)
        {
            if (self.rb)
            {
                orig(self);
            }
        }

        private void ProjectileSteerTowardTarget_FixedUpdate(On.RoR2.Projectile.ProjectileSteerTowardTarget.orig_FixedUpdate orig, RoR2.Projectile.ProjectileSteerTowardTarget self)
        {
            if (self.targetComponent)
            {
                orig(self);
            }
        }
    }
}