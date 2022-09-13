using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ReturnToVoidFields
{
    [BepInPlugin("com.DestroyedClone.ReturnToVoidFields", "ReturnToVoidFields", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        /* 1. Find out how the portal in the bazaar is disabled
         * 2. Find the check for it, variable?
         * 3. Override the check
         * 4. Enable the timer
         * 5. Enable normal monster spawns.
         */

        public void Start()
        {

        }
    }
}
