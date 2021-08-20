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
using UnityEngine.Networking;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete


namespace SpawnAsMonsterOnClick
{
    [BepInPlugin("com.DestroyedClone.SpawnAsMonsterOnClick", "Spawn as monster on lcick", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class Class1 : BaseUnityPlugin
    {
        public static List<CharacterMaster> characterMasters = new List<CharacterMaster>();

        public void Awake()
        {
            On.RoR2.CharacterMaster.Start += CharacterMaster_Start;
            On.RoR2.PlayerCharacterMasterController.Awake += PlayerCharacterMasterController_Awake;
        }

        private void CharacterMaster_Start(On.RoR2.CharacterMaster.orig_Start orig, CharacterMaster self)
        {
            orig(self);
            self.GetBody().gameObject.AddComponent<ClickToSpawnAs>().characterMaster = self;
        }

        private void PlayerCharacterMasterController_Awake(On.RoR2.PlayerCharacterMasterController.orig_Awake orig, PlayerCharacterMasterController self)
        {
            orig(self);
            self.gameObject.AddComponent<ClickToSpawnAsUser>().player = self;
        }
        public class ClickToSpawnAsUser : MonoBehaviour
        {
            public PlayerCharacterMasterController player;

            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (characterMasters.Count > 0)
                    {
                        var chosenBody = characterMasters.FirstOrDefault();
                        player.master.TransformBody(chosenBody.bodyPrefab.name);
                        characterMasters.Clear();
                    }
                }
            }
            public void FixedUpdate()
            {
                if (characterMasters.Count>0)
                {
                    int i = 0;
                    Debug.Log(Time.time);
                    foreach (var characterMaster in characterMasters)
                    {
                        Debug.Log(i + " " + characterMaster.GetBody().GetDisplayName());
                        i++;
                    }
                }
            }
        }

        public class ClickToSpawnAs : MonoBehaviour
        {
            public CharacterMaster characterMaster;

            //When the mouse hovers over the GameObject, it turns to this color (red)
            Color m_MouseOverColor = Color.red;

            //This stores the GameObject’s original color
            Color m_OriginalColor;

            //Get the GameObject’s mesh renderer to access the GameObject’s material and color
            MeshRenderer m_Renderer;

            public void Awake()
            {
            }
            public void Start()
            {
                //Fetch the mesh renderer component from the GameObject
                m_Renderer = GetComponent<MeshRenderer>();
                //Fetch the original color of the GameObject
                m_OriginalColor = m_Renderer.material.color;
            }
            public void OnMouseEnter()
            {
                characterMasters.Add(characterMaster);

                // Change the color of the GameObject to red when the mouse is over GameObject
                m_Renderer.material.color = m_MouseOverColor;
            }
            public void OnMouseExit()
            {
                characterMasters.Remove(characterMaster);


                // Reset the color of the GameObject back to normal
                m_Renderer.material.color = m_OriginalColor;
            }

        }
    }
}
