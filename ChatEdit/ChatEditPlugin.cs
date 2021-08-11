using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

using System.Collections.ObjectModel;
using System.Globalization;
using RoR2.ConVar;
using RoR2.Networking;
using Unity;
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ChatEdit
{
    [BepInPlugin("com.DestroyedClone.ChatEdit", "Chat Edit", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ChatEditPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            // This mod is DESTRUCTIVE AND WILL NOT WORK WITH OTHER MODS
            // Viewer discretion is advised
            IL.RoR2.Chat.AddMessage_string += Chat_AddMessage_string;
        }

        private void Chat_AddMessage_string(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdsfld<IntConVar>("")
                );
            c.Index += 4;
            c.Emit(OpCodes.Ldloc);
            c.EmitDelegate<Func<HealthComponent, bool>>((hc) =>
            {
                if ((bool)hc.body?.hasCloakBuff)
                {
                    return false;
                }
                return true;
            });
        }
    }
}
