using BepInEx;
using RoR2;
using UnityEngine;

namespace PressurePlateHolders
{
    [BepInPlugin("com.DestroyedClone.PressurePlateHolders", "Pressure Plate Holders", "1.0.0")]
    public class Class1 : BaseUnityPlugin
    {
        public static GameObject VendingMachinePrefab = null;
        public static Vector3 platePos;

        public void Start()
        {
            VendingMachinePrefab = LegacyResourcesAPI.Load<GameObject>("RoR2/DLC1/VendingMachine/VendingMachine");
            //VendingMachinePrefab = LegacyResourcesAPI.Load<GameObject>("RoR2/DLC1/VendingMachine/VendingMachine");
            //var capCollider = VendingMachinePrefab.AddComponent<CapsuleCollider>();
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            On.RoR2.PressurePlateController.Start += PressurePlateController_Start;
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            VendingMachinePrefab.transform.GetChild(0).gameObject.GetComponent<BoxCollider>().isTrigger = false;
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.isPlayerControlled)
            {
                TeleportHelper.TeleportBody(self, platePos);
            }
        }

        private void PressurePlateController_Start(On.RoR2.PressurePlateController.orig_Start orig, PressurePlateController self)
        {
            orig(self);
            platePos = self.transform.position;
        }
    }
}