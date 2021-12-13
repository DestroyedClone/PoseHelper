using EntityStates.Mage;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;

namespace PersonalizedPodPrefabs
{
    public class Artificer : PodBase
    {
        public static Dictionary<string, BuffDef> primaryToken_to_buffDef = new Dictionary<string, BuffDef>()
        {
            { "MAGE_PRIMARY_FIRE_NAME" , RoR2Content.Buffs.AffixRed },
            { "MAGE_PRIMARY_LIGHTNING_NAME", RoR2Content.Buffs.AffixBlue }
        };

        public override string BodyName => RoR2Content.Survivors.Mage.bodyPrefab.name;

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            /*podPrefab.GetComponentInChildren<MeshRenderer>().material
                = SurvivorCatalog.FindSurvivorDefFromBody(BodyCatalog.FindBodyPrefab(BodyName))
                .displayPrefab.transform.Find("MageMesh").GetComponent<SkinnedMeshRenderer>().material;*/
            podPrefab.AddComponent<ArtificerPodComponent>();
            return podPrefab;
        }

        private class ArtificerPodComponent : PodComponent
        {
            private readonly float buffDuration = 8f;

            protected override void Start()
            {
                addLandingAction = false;
                addExitAction = true;
                base.Start();
            }

            protected override void VehicleSeat_onPassengerExit(GameObject passenger)
            {
                if (isServer)
                {
                    var characterBody = passenger.GetComponent<CharacterBody>();
                    if (characterBody && characterBody.skillLocator && characterBody.skillLocator.primary)
                    {
                        var token = characterBody.skillLocator.primary.skillDef.skillNameToken;
                        if (primaryToken_to_buffDef.TryGetValue(token, out BuffDef buffDef))
                        {
                            characterBody.AddTimedBuff(buffDef, buffDuration);
                        }
                    }
                }
            }

            private BlastAttack CreateBlastAttack(GameObject podObject, GameObject passengerObject)
            {
                var characterBody = passengerObject.GetComponent<CharacterBody>();
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.radius = FlyUpState.blastAttackRadius;
                blastAttack.procCoefficient = FlyUpState.blastAttackProcCoefficient;
                blastAttack.position = podObject.transform.position;
                blastAttack.attacker = passengerObject;
                blastAttack.crit = characterBody ? Util.CheckRoll(characterBody.crit, characterBody.master) : false;
                // 12 is artificer's base damage
                blastAttack.baseDamage = (characterBody ? characterBody.damage : 12f) * FlyUpState.blastAttackDamageCoefficient;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.baseForce = FlyUpState.blastAttackForce;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                blastAttack.attackerFiltering = AttackerFiltering.NeverHit;
                return blastAttack;
            }

            private void ShootFire(GameObject podObject, GameObject passengerObject)
            {
                var blastAttack = CreateBlastAttack(podObject, passengerObject);
                blastAttack.damageType = DamageType.IgniteOnHit;
                blastAttack.Fire();
            }

            private void ShootShock(GameObject podObject, GameObject passengerObject)
            {
                var blastAttack = CreateBlastAttack(podObject, passengerObject);
                blastAttack.damageType = DamageType.Stun1s;
                blastAttack.Fire();

                //effect
                //CreateBlinkEffect is private :(
                EffectData effectData = new EffectData();
                effectData.rotation = Util.QuaternionSafeLookRotation(Vector3.up);
                effectData.origin = blastAttack.position;
                EffectManager.SpawnEffect(FlyUpState.blinkPrefab, effectData, true);
            }
        }
    }
}