using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace StageVariantsPlus
{
    public class DebuffZonePlus : MonoBehaviour
    {
        [Tooltip("The buff type to grant")]
        public BuffIndex buffType;
        [Tooltip("The buff duration")]
        public float buffDuration;
        public string buffApplicationSoundString;
        public GameObject buffApplicationEffectPrefab;

        private List<Collider> previousColliderList = new List<Collider>();
        private float resetStopwatch = 3f;
        public float orbResetListFrequency;


        public void FixedUpdate()
        {
            if (this.previousColliderList.Count > 0)
            {
                this.resetStopwatch += Time.fixedDeltaTime;
                if (this.resetStopwatch > 1f / this.orbResetListFrequency)
                {
                    this.resetStopwatch -= 1f / this.orbResetListFrequency;
                    this.previousColliderList.Clear();
                }
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active)
            {
                if (this.previousColliderList.Contains(other))
                {
                    return;
                }
                this.previousColliderList.Add(other);
                CharacterBody component = other.GetComponent<CharacterBody>();
                if (component && component.mainHurtBox)
                {
                    ApplyDebuff(component);
                }
            }
        }

        private void ApplyDebuff(CharacterBody characterBody)
        {
            characterBody.AddTimedBuff(this.buffType, this.buffDuration);
            if (!characterBody.HasBuff(this.buffType))
            {
                Util.PlaySound(this.buffApplicationSoundString, characterBody.gameObject);
                if (this.buffApplicationEffectPrefab)
                {
                    EffectManager.SpawnEffect(this.buffApplicationEffectPrefab, new EffectData
                    {
                        origin = characterBody.mainHurtBox.transform.position,
                        scale = characterBody.radius
                    }, true);
                }
            }
        }
    }
}
