using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using RoR2.Projectile;
using static PersonalizedPodPrefabs.PersonalizePodPlugin;
using UnityEngine.Networking;
using EntityStates.Mage;

namespace PersonalizedPodPrefabs
{
    public class Artificer : PodBase
    {
        public override string BodyName => "MageBody";

        public override GameObject CreatePod()
        {
            GameObject podPrefab = PrefabAPI.InstantiateClone(genericPodPrefab, PodPrefabName);
            podPrefab.AddComponent<ArtificerPodComponent>();
            return podPrefab;
        }

        private class ArtificerPodComponent : PodComponent
        {
            private string landingType;

            protected override void Start()
            {
                base.Start();
                landingType = EvaluatePrimarySkillDef();
                PersonalizePodPlugin.onPodLandedServer += PersonalizePodPlugin_onPodLandedServer;
            }

            private void PersonalizePodPlugin_onPodLandedServer(VehicleSeat eventVehicleSeat, GameObject passengerBodyObject)
            {
                if (eventVehicleSeat == vehicleSeat)
                    CreateLandingEffect(eventVehicleSeat.gameObject, passengerBodyObject);
            }

            protected override void OnDestroy()
            {
                base.OnDestroy();
                PersonalizePodPlugin.onPodLandedServer -= PersonalizePodPlugin_onPodLandedServer;
            }

            private string EvaluatePrimarySkillDef()
            {
                if (vehicleSeat.currentPassengerBody && vehicleSeat.currentPassengerBody.skillLocator)
                {
                    if (vehicleSeat.currentPassengerBody.skillLocator.primary)
                    {
                        var skillDef = vehicleSeat.currentPassengerBody.skillLocator.primary.skillDef;
                        if (skillDef == vehicleSeat.currentPassengerBody.skillLocator.primary.baseSkill)
                        {
                            return "Fire";
                        } else
                        {
                            return "Shock";
                        }
                    }
                }
                return "Fire";
            }

            private void CreateLandingEffect(GameObject podObject, GameObject passengerObject)
            {
                if (NetworkServer.active)
                {
                    switch (landingType)
                    {
                        case "Fire":
                            ShootFire(podObject, passengerObject);
                            break;
                        case "Shock":
                        default:
                            ShootShock(podObject, passengerObject);
                            break;
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
