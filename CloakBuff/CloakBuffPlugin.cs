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

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace CloakBuff
{
    [BepInPlugin("com.DestroyedClone.CloakBuff", "CloakBuff", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class CloakBuffPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> HideDoppelgangerEffect { get; set; }
        public static ConfigEntry<bool> EnableHealthbar { get; set; }
        public static ConfigEntry<bool> EnablePinging { get; set; }
        public static ConfigEntry<bool> EnableDamageNumbers { get; set; }
        public static ConfigEntry<int> MissileIncludesFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> MissileIncludesHarpoons { get; set; }

        public static ConfigEntry<bool> MissileIncludesDMLATG { get; set; }
        public static ConfigEntry<int> LightningOrbIncludesFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> LightningOrbIncludesBFG { get; set; }

        public static ConfigEntry<bool> LightningOrbIncludesGlaive { get; set; }
        public static ConfigEntry<bool> LightningOrbIncludesUkulele { get; set; }
        public static ConfigEntry<bool> LightningOrbIncludesRazorwire { get; set; }
        public static ConfigEntry<bool> LightningOrbIncludesCrocoDisease { get; set; }
        public static ConfigEntry<bool> LightningOrbIncludesTesla { get; set; }
        public static ConfigEntry<int> DevilOrbIncludesFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> DevilOrbIncludesSprintWisp { get; set; }

        public static ConfigEntry<bool> DevilOrbIncludesNovaOnHeal { get; set; }
        public static ConfigEntry<int> ProjectileDirectionalTargetFinderFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist
        public static ConfigEntry<bool> ProjectileDirectionalTargetFinderDagger { get; set; }

        public static ConfigEntry<int> ProjectileSphereTargetFinderFilterType { get; set; }

        // 0 = No hook, 1 = All, 2 = Whitelist

        public static ConfigEntry<bool> MiredUrn { get; set; }
        public static ConfigEntry<bool> RoyalCap { get; set; }
        public static ConfigEntry<bool> HuntressCantAim { get; set; }
        public static ConfigEntry<bool> MercCantFind { get; set; }
        public static ConfigEntry<bool> ShockKillsCloak { get; set; }
        public static ConfigEntry<bool> EngiChargeMine { get; set; }
        public static ConfigEntry<bool> EngiSpiderMine { get; set; }
        public static ConfigEntry<bool> EngiSpiderMineCanExplodeOnImpaled { get; set; }

        public GameObject DoppelgangerEffect = Resources.Load<GameObject>("prefabs/temporaryvisualeffects/DoppelgangerEffect");
        public static float evisMaxRange = EntityStates.Merc.Evis.maxRadius;

        public void Awake()
        {
            SetupConfig();
            if (HideDoppelgangerEffect.Value)
                ModifyDoppelGangerEffect();
            if (EnableHealthbar.Value)
                On.RoR2.UI.CombatHealthBarViewer.VictimIsValid += CombatHealthBarViewer_VictimIsValid;
            if (EnablePinging.Value)
                On.RoR2.Util.HandleCharacterPhysicsCastResults += Util_HandleCharacterPhysicsCastResults;
            if (EnableDamageNumbers.Value)
                IL.RoR2.HealthComponent.HandleDamageDealt += HealthComponent_HandleDamageDealt1;
            //IL.RoR2.Util.HandleCharacterPhysicsCastResults += Util_HandleCharacterPhysicsCastResults1;

            // Character Specific
            if (HuntressCantAim.Value)
                On.RoR2.HuntressTracker.SearchForTarget += HuntressTracker_SearchForTarget;
            if (MercCantFind.Value)
            {
                On.EntityStates.Merc.Evis.SearchForTarget += Evis_SearchForTarget;
            }
            // Squid
            //On.EntityStates.Squid.SquidWeapon.FireSpine.
            if (ProjectileSphereTargetFinderFilterType.Value != 0)
            {
                if (EngiChargeMine.Value || EngiSpiderMine.Value)
                {
                    On.RoR2.Projectile.ProjectileSphereTargetFinder.PassesFilters += ProjectileSphereTargetFinder_PassesFilters;
                }
            }

            // Shock thing
            if (ShockKillsCloak.Value)
                On.RoR2.SetStateOnHurt.SetShock += SetStateOnHurt_SetShock;

            // Items
            // DML + ATG
            if (MissileIncludesFilterType.Value != 0)
            {
                On.RoR2.Projectile.MissileController.FindTarget += MissileController_FindTarget;
                if (MissileIncludesHarpoons.Value)
                    On.EntityStates.Engi.EngiMissilePainter.Paint.GetCurrentTargetInfo += Paint_GetCurrentTargetInfo;
            }
            // BFG / Huntress' Glaive / Ukulele / Razorwire / CrocoDisease / Tesla
            if (LightningOrbIncludesFilterType.Value != 0)
                On.RoR2.Orbs.LightningOrb.PickNextTarget += LightningOrb_PickNextTarget;
            // Little Disciple / N'kuhana's Opinion
            if (DevilOrbIncludesFilterType.Value != 0)
                On.RoR2.Orbs.DevilOrb.PickNextTarget += DevilOrb_PickNextTarget;
            if (MiredUrn.Value)
                On.RoR2.SiphonNearbyController.SearchForTargets += SiphonNearbyController_SearchForTargets;
            // Ceremonial Dagger
            if (ProjectileDirectionalTargetFinderFilterType.Value != 0)
                On.RoR2.Projectile.ProjectileDirectionalTargetFinder.SearchForTarget += ProjectileDirectionalTargetFinder_SearchForTarget;
            if (RoyalCap.Value)
                On.RoR2.EquipmentSlot.ConfigureTargetFinderForEnemies += EquipmentSlot_ConfigureTargetFinderForEnemies;
        }

        private void HealthComponent_HandleDamageDealt1(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdloc(2),
                x => x.MatchCallvirt<TeamComponent>("get_teamIndex"),
                x => x.MatchLdloc(0),
                x => x.MatchLdfld<DamageDealtMessage>("damageColorIndex"),
                x => x.MatchCallvirt<DamageNumberManager>("SpawnDamageNumber")
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

        public void SetupConfig()
        {
            HideDoppelgangerEffect = Config.Bind("Visual", "Umbra", true, "Hides the Umbra's swirling particle effects");
            EnableHealthbar = Config.Bind("Visual", "Healthbar", true, "Become unable to see the enemy's healthbar");
            EnablePinging = Config.Bind("Visual", "Pinging", true, "Attempts to mislead pinging by pinging the enemy behind the cloaked target.");
            EnableDamageNumbers = Config.Bind("Visual", "Damage Numbers", true, "Hides damage numbers from appearing on cloaked targets.");
            
            MissileIncludesDMLATG = Config.Bind("Items", "Disposable Missile Launcher and AtG Missile Mk. 1", true, "Missiles no longer consider cloaked targets valid.");
            LightningOrbIncludesBFG = Config.Bind("Items", "Preon Accumulator", false, "Currently Broken");
            LightningOrbIncludesUkulele = Config.Bind("Items", "Ukulele", true, "Ukulele electricity no longer arcs to cloaked targets.");
            LightningOrbIncludesRazorwire = Config.Bind("Items", "Razorwire", false, "Currently Broken");
            LightningOrbIncludesTesla = Config.Bind("Items", "Unstable Tesla Coil", true, "Tesla electricity no longer arcs to cloaked targets.");
            DevilOrbIncludesNovaOnHeal = Config.Bind("Items", "Nkuhanas Opinion", true, "No longer seeks out cloaked targets.");
            DevilOrbIncludesSprintWisp = Config.Bind("Items", "Little Disciple", true, "No longer seeks out cloaked targets.");
            ProjectileDirectionalTargetFinderDagger = Config.Bind("Items", "Ceremonial Dagger", true, "No longer seeks out cloaked targets.");
            MiredUrn = Config.Bind("Items", "Mired Urn", true, "Finnicky. Prioritizes noncloaked enemies, but will target a cloaked enemy if they are the only target.");
            RoyalCap = Config.Bind("Items", "Royal Capacitator", true, "Prevents the aiming reticle from targeting cloaked targets.");

            LightningOrbIncludesCrocoDisease = Config.Bind("Survivors", "Acrid Epidemic", false, "Currently Broken. Affects Acrid's special Epidemic's spreading");
            MissileIncludesHarpoons = Config.Bind("Survivors", "Engineer Harpoons+Targeting", true, "Affects the Engineer's Utility Thermal Harpoons. Also prevents the user from painting cloaked enemies as targets.");
            EngiChargeMine = Config.Bind("Survivors", "Engineer Pressure Mines", true, "Finnicky. Affects the Engineer's Secondary Pressure Mines. Prevents exploding when cloaked enemies are in proximity.");
            EngiSpiderMine = Config.Bind("Survivors", "Engineer Spider Mines", true, "Affects the Engineer's Secondary Spider Mines. Prevents exploding when cloaked enemies are in proximity.");
            EngiSpiderMineCanExplodeOnImpaled = Config.Bind("Survivors", "Engineer Spider Mines Single Target", true, "Affects the Engineer's Secondary Spider Mines, requires the previous option to be enabled. If true, then it will explode when armed if it is stuck on a cloaked target.");
            HuntressCantAim = Config.Bind("Survivors", "Huntress Aiming", true, "This adjustment will make Huntress unable to target cloaked enemies with her primary and secondary abilities");
            LightningOrbIncludesGlaive = Config.Bind("Survivors", "Huntress Glaive", true, "Affects the Huntress' Secondary Laser Glaive.");
            MercCantFind = Config.Bind("Survivors", "Mercernary Eviscerate", true, "Finnicky. Fails if an invalid enemy is within the same range of a valid enemy. The adjustment will prevent Mercernary's Eviscerate from targeting cloaked enemies");

            ShockKillsCloak = Config.Bind("Nerfs", "Shocking disrupts cloak", true, "Setting this value to true will allow shocking attacks (Captain's M2 and Shocking Beacon) to clear cloak on hit. Note that Survivors are immune to this damagetype.");

            MissileIncludesFilterType = Config.Bind("zFiltering", "MissileController", 2, "Its safe to ignore the options in this category." +
                "\n 0 = Disabled," +
                "\n 1 = All missiles are affected" +
                "\n 2 = Only the following options");
            LightningOrbIncludesFilterType = Config.Bind("zFiltering", "Lightning Orbs", 2, "0 = Disabled," +
                "\n 1 = All Lightning Orbs are affected" +
                "\n 2 = Only the following options");
            DevilOrbIncludesFilterType = Config.Bind("zFiltering", "Devil Orbs", 2, "0 = Disabled," +
                "\n 1 = All Devil Orbs are affected" +
                "\n 2 = Only the following options");
            ProjectileDirectionalTargetFinderFilterType = Config.Bind("zFiltering", "ProjectileDirectionalTargetFinder", 2, "0 = Disabled," +
                "\n 1 = All ProjectileDirectionalTargetFinderFilterType are affected" +
                "\n 2 = Only the following options");
            ProjectileSphereTargetFinderFilterType = Config.Bind("zFiltering", "ProjectileSphereTargetFinder", 2, "#NG!M!N#S" +
                "\n 0 = Disabled," +
                "\n 1 = All ProjectileSphereTargetFinderFilterType are affected" +
                "\n 2 = Only the following options");
        }

        private void ModifyDoppelGangerEffect()
        {
            if (!DoppelgangerEffect) return;

            var comp = DoppelgangerEffect.GetComponent<HideShadowIfCloaked>();
            if (!comp)
            {
                comp = DoppelgangerEffect.AddComponent<HideShadowIfCloaked>();
            }
            comp.particles = DoppelgangerEffect.transform.Find("Particles").gameObject;
            comp.visEfx = DoppelgangerEffect.GetComponent<TemporaryVisualEffect>();
        }

        // SurvivorSpecific
        private void Paint_GetCurrentTargetInfo(On.EntityStates.Engi.EngiMissilePainter.Paint.orig_GetCurrentTargetInfo orig, EntityStates.Engi.EngiMissilePainter.Paint self, out HurtBox currentTargetHurtBox, out HealthComponent currentTargetHealthComponent)
        {
            orig(self, out currentTargetHurtBox, out currentTargetHealthComponent);
            foreach (HurtBox hurtBox in self.search.GetResults())
            {
                if ((bool)hurtBox.healthComponent?.alive && hurtBox.healthComponent.body && !hurtBox.healthComponent.body.hasCloakBuff)
                {
                    currentTargetHurtBox = hurtBox;
                    currentTargetHealthComponent = hurtBox.healthComponent;
                    return;
                }
            }
            currentTargetHurtBox = null;
            currentTargetHealthComponent = null;
        }

        private void HuntressTracker_SearchForTarget(On.RoR2.HuntressTracker.orig_SearchForTarget orig, HuntressTracker self, Ray aimRay)
        {
            orig(self, aimRay);
            self.trackingTarget = FilterMethod(self.search.GetResults());
        }

        private HurtBox Evis_SearchForTarget(On.EntityStates.Merc.Evis.orig_SearchForTarget orig, EntityStates.Merc.Evis self)
        {
            var original = orig(self);
            if (!original.healthComponent.body.hasCloakBuff)
            {
                return original;
            }
            BullseyeSearch bullseyeSearch = new BullseyeSearch
            {
                searchOrigin = self.transform.position,
                searchDirection = UnityEngine.Random.onUnitSphere,
                maxDistanceFilter = evisMaxRange,
                teamMaskFilter = TeamMask.GetUnprotectedTeams(self.GetTeam()),
                sortMode = BullseyeSearch.SortMode.Distance
            };
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(self.gameObject);
            return FilterMethod(bullseyeSearch.GetResults());
        }

        // Targeting
        private void ProjectileDirectionalTargetFinder_SearchForTarget(On.RoR2.Projectile.ProjectileDirectionalTargetFinder.orig_SearchForTarget orig, RoR2.Projectile.ProjectileDirectionalTargetFinder self)
        {
            orig(self);
            var type = self.gameObject.name;
            if (ProjectileDirectionalTargetFinderFilterType.Value == 2)
            {
                var daggerCheck = (type == "DaggerProjectile(Clone)" && ProjectileDirectionalTargetFinderDagger.Value);
                if (!daggerCheck)
                {
                    IEnumerable<HurtBox> source = self.bullseyeSearch.GetResults().Where(new Func<HurtBox, bool>(self.PassesFilters));
                    self.SetTarget(FilterMethod(source));
                }
            }
        }

        private void SiphonNearbyController_SearchForTargets(On.RoR2.SiphonNearbyController.orig_SearchForTargets orig, SiphonNearbyController self, List<HurtBox> dest)
        {
            orig(self, dest);
            self.sphereSearch.ClearCandidates();
            self.sphereSearch.RefreshCandidates();
            self.sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(self.networkedBodyAttachment.attachedBody.teamComponent.teamIndex));
            var destCopy = new List<HurtBox>(dest);
            foreach (var hurtBox in destCopy)
            {
                //Debug.Log("Mired Urn: Checking " + hurtBox.healthComponent.body.GetDisplayName());
                if ((bool)hurtBox.healthComponent?.body?.hasCloakBuff)
                {
                    dest.Remove(hurtBox);
                    //Debug.Log("Removed");
                }
                else
                {
                    //Debug.Log("Kept");
                }
            }
            self.sphereSearch.OrderCandidatesByDistance();
            self.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            self.sphereSearch.GetHurtBoxes(dest);
            self.sphereSearch.ClearCandidates();
        }

        private HurtBox DevilOrb_PickNextTarget(On.RoR2.Orbs.DevilOrb.orig_PickNextTarget orig, RoR2.Orbs.DevilOrb self, Vector3 position, float range)
        {
            var type = self.effectType;
            //Debug.Log("Devil Orb: "+type.ToString());
            if (DevilOrbIncludesFilterType.Value == 2)
            {
                var novaOnHealCheck = (type == RoR2.Orbs.DevilOrb.EffectType.Skull && DevilOrbIncludesNovaOnHeal.Value);
                var sprintWispCheck = (type == RoR2.Orbs.DevilOrb.EffectType.Wisp && DevilOrbIncludesSprintWisp.Value);
                if (!(novaOnHealCheck || sprintWispCheck))
                {
                    return orig(self, position, range);
                }
            }

            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = position;
            bullseyeSearch.searchDirection = Vector3.zero;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.teamMaskFilter.RemoveTeam(self.teamIndex);
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.maxDistanceFilter = range;
            bullseyeSearch.RefreshCandidates();
            List<HurtBox> list = bullseyeSearch.GetResults().ToList<HurtBox>();
            if (list.Count <= 0)
            {
                return null;
            }

            HurtBox hurtBox = list[UnityEngine.Random.Range(0, list.Count)];

            while ((bool)hurtBox.healthComponent?.body.hasCloakBuff && list.Count > 0)
            {
                list.Remove(hurtBox);
                hurtBox = list[UnityEngine.Random.Range(0, list.Count)];
            }
            return hurtBox;
        }

        private HurtBox LightningOrb_PickNextTarget(On.RoR2.Orbs.LightningOrb.orig_PickNextTarget orig, RoR2.Orbs.LightningOrb self, Vector3 position)
        {
            var type = self.lightningType;
            var original = orig(self, position);
            //Debug.Log("Lightning Orb: "+type.ToString());

            if (LightningOrbIncludesFilterType.Value == 2)
            {
                var bfgCheck = (type == RoR2.Orbs.LightningOrb.LightningType.BFG && LightningOrbIncludesBFG.Value);
                var glaiveCheck = (type == RoR2.Orbs.LightningOrb.LightningType.HuntressGlaive && LightningOrbIncludesGlaive.Value);
                var ukuleleCheck = (type == RoR2.Orbs.LightningOrb.LightningType.Ukulele && LightningOrbIncludesUkulele.Value);
                var razorwireCheck = (type == RoR2.Orbs.LightningOrb.LightningType.RazorWire && LightningOrbIncludesRazorwire.Value);
                var crocoDiseaseCheck = (type == RoR2.Orbs.LightningOrb.LightningType.CrocoDisease && LightningOrbIncludesCrocoDisease.Value);
                var teslaCheck = (type == RoR2.Orbs.LightningOrb.LightningType.Tesla && LightningOrbIncludesTesla.Value);
                if (!(bfgCheck || glaiveCheck || ukuleleCheck || razorwireCheck || crocoDiseaseCheck || teslaCheck))
                {
                    return original;
                }
            }
            if (self.search == null)
                self.search = new BullseyeSearch();
            self.search.searchOrigin = position;
            self.search.searchDirection = Vector3.zero;
            self.search.teamMaskFilter = TeamMask.allButNeutral;
            self.search.teamMaskFilter.RemoveTeam(self.teamIndex);
            self.search.filterByLoS = false;
            self.search.sortMode = BullseyeSearch.SortMode.Distance;
            self.search.maxDistanceFilter = self.range;
            self.search.RefreshCandidates();
            HurtBox hurtBox = (from v in self.search.GetResults()
                               where (!self.bouncedObjects.Contains(v.healthComponent) && !v.healthComponent.body.hasCloakBuff)
                               select v).FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                self.bouncedObjects.Add(hurtBox.healthComponent);
            }
            return hurtBox;
        }

        private Transform MissileController_FindTarget(On.RoR2.Projectile.MissileController.orig_FindTarget orig, RoR2.Projectile.MissileController self)
        {
            var objName = self.gameObject.name;
            if (MissileIncludesFilterType.Value == 2)
            {
                var harpoonCheck = (objName == "EngiHarpoon(Clone)" && MissileIncludesHarpoons.Value);
                var dmrAtgCheck = (objName == "MissileProjectile(Clone)" && MissileIncludesDMLATG.Value);
                if (!(harpoonCheck || dmrAtgCheck))
                {
                    return orig(self);
                }
            }
            self.search.searchOrigin = self.transform.position;
            self.search.searchDirection = self.transform.forward;
            self.search.teamMaskFilter.RemoveTeam(self.teamFilter.teamIndex);
            self.search.RefreshCandidates();
            HurtBox hurtBox = FilterMethod(self.search.GetResults());

            if (hurtBox == null)
            {
                return null;
            }
            return hurtBox.transform;
        }

        private bool Util_HandleCharacterPhysicsCastResults(On.RoR2.Util.orig_HandleCharacterPhysicsCastResults orig, GameObject bodyObject, Ray ray, RaycastHit[] hits, out RaycastHit hitInfo)
        {
            int num = -1;
            float num2 = float.PositiveInfinity;
            for (int i = 0; i < hits.Length; i++)
            {
                float distance = hits[i].distance;
                if (distance < num2)
                {
                    HurtBox component = hits[i].collider.GetComponent<HurtBox>();
                    if (component)
                    {
                        HealthComponent healthComponent = component.healthComponent;
                        if (healthComponent)
                        {
                            if (healthComponent.gameObject == bodyObject)
                                goto IL_82;
                            else if (healthComponent.body.hasCloakBuff) // This is where you would put IL if you were smart (not me)
                            {
                                continue;
                            }
                        }
                    }
                    if (distance == 0f)
                    {
                        hitInfo = hits[i];
                        hitInfo.point = ray.origin;
                        return true;
                    }
                    num = i;
                    num2 = distance;
                }
            IL_82:;
            }
            if (num == -1)
            {
                hitInfo = default;
                return false;
            }
            hitInfo = hits[num];
            return true;
        }

        private bool CombatHealthBarViewer_VictimIsValid(On.RoR2.UI.CombatHealthBarViewer.orig_VictimIsValid orig, RoR2.UI.CombatHealthBarViewer self, HealthComponent victim)
        {
            return orig(self, victim) && !victim.body.hasCloakBuff;
        }

        private void SetStateOnHurt_SetShock(On.RoR2.SetStateOnHurt.orig_SetShock orig, SetStateOnHurt self, float duration)
        {
            orig(self, duration);
            var body = self.gameObject.GetComponent<CharacterBody>();
            if (!body)
            {
                return;
            }
            if (body.HasBuff(RoR2Content.Buffs.Cloak))
            {
                body.ClearTimedBuffs(RoR2Content.Buffs.Cloak);
            }
        }

        private void EquipmentSlot_ConfigureTargetFinderForEnemies(On.RoR2.EquipmentSlot.orig_ConfigureTargetFinderForEnemies orig, EquipmentSlot self)
        {
            orig(self);
            foreach (var target in self.targetFinder.GetResults())
            {
                if ((bool)target.healthComponent?.body?.hasCloakBuff)
                    self.targetFinder.FilterOutGameObject(target.gameObject);
            }
        }

        private void Util_HandleCharacterPhysicsCastResults1(ILContext il) //harb
        {
            ILCursor c = new ILCursor(il);
            int healthComponentLocal = -1;
            c.GotoNext(
              x => x.MatchLdfld("HealthComponent", "healthComponent"),

              x => x.MatchLdloc(out healthComponentLocal)
            );
            ILLabel continLabel = null;
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                //x => x.MatchCall("op_Equality"),
                x => x.MatchBrtrue(out continLabel)
            );
            c.Emit(OpCodes.Ldloc, healthComponentLocal);
            c.EmitDelegate<Func<HealthComponent, bool>>(
                (HealthComponent hc) =>
                { return hc.body.hasCloakBuff; }
                );
            c.Emit(OpCodes.Brtrue, continLabel);

            c.Emit(OpCodes.Add);
        }

        private bool ProjectileSphereTargetFinder_PassesFilters(On.RoR2.Projectile.ProjectileSphereTargetFinder.orig_PassesFilters orig, RoR2.Projectile.ProjectileSphereTargetFinder self, HurtBox result)
        {
            var original = orig(self, result);
            var objName = self.gameObject.name;
            var spiderCheck = (objName == "SpiderMine(Clone)" && EngiSpiderMine.Value);
            CharacterBody body = result.healthComponent.body;
            if (ProjectileSphereTargetFinderFilterType.Value == 2)
            {
                var mineCheck = (objName == "EngiMine(Clone)" && EngiChargeMine.Value);
                if (!(mineCheck || spiderCheck))
                {
                    Debug.Log("Neither type of mine");
                    return original;
                }
                if (spiderCheck)
                {
                    if (EngiSpiderMineCanExplodeOnImpaled.Value)
                    {
                        var stickOnImpact = self.gameObject.GetComponent<RoR2.Projectile.ProjectileStickOnImpact>();
                        if (stickOnImpact?.victim == body.gameObject)
                        {
                            Debug.Log("Spidermine is attached, so we're exploding it");
                            return original;
                        }
                    }
                }
            }
            return original && !body.hasCloakBuff;
        }

        private HurtBox FilterMethod(IEnumerable<HurtBox> listOfTargets)
        {
            HurtBox hurtBox = listOfTargets.FirstOrDefault<HurtBox>();
            if (hurtBox == null)
            {
                //Debug.Log("Evis chose target: None");
            }
            else
            {
                //Debug.Log("Evis chose target: " + hurtBox.healthComponent.body.GetDisplayName());
            }
            //Debug.Log("Attempting Iteration with list of length: "+listOfTargets.Count());
            int index = 0;
            while (hurtBox != null)
            {
                if ((bool)hurtBox.healthComponent?.body?.hasCloakBuff)
                {
                    //Debug.Log("Target was cloaked, moving on to");
                    index++;
                    hurtBox = listOfTargets.ElementAtOrDefault(index);
                    //Debug.Log("NEW Target: " + hurtBox.healthComponent.body.GetDisplayName());
                    continue;
                }
                //Debug.Log("Chosen target works!");
                break;
            }
            return hurtBox;
        }

        private class HideShadowIfCloaked : MonoBehaviour
        {
            public CharacterBody body;
            public GameObject particles;
            public TemporaryVisualEffect visEfx;

            public void Start()
            {
                body = visEfx.healthComponent.body;
            }

            public void FixedUpdate()
            {
                if (body)
                {
                    particles.SetActive(!body.hasCloakBuff);
                }
            }
        }

        private enum MissileTypes
        {
            None,
            All,
            Harpoons,
            DMLATG
        }

        private enum LightningOrbTypes
        {
            None,
            All,
            BFG,
            Glaive,
            Ukulele,
            Razorwire,
            CrocoDisease,
            Tesla
        }

        private enum DevilOrbTypes
        {
            None,
            All,
            SprintWisp,
            NovaOnHeal
        }
    }
}