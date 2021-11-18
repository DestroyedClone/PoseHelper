using BepInEx;
using System.Security.Permissions;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using R2API.Utils;
using R2API.Networking;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace ShareYourFood
{
    [BepInPlugin("com.DestroyedClone.ShareYourFood", "Share Your Food", "1.0.0")]
	[BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
	[R2APISubmoduleDependency(nameof(PrefabAPI))]

	public class Main : BaseUnityPlugin
    {
        public static GameObject fruitPickup;

        public void Start()
        {
			CreatePrefab();

			On.RoR2.EquipmentSlot.FireFruit += EquipmentSlot_FireFruit;
        }

        private static void CreatePrefab()
        {
			fruitPickup = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/NetworkedObjects/HealPack"), "FruitPack", true);
			fruitPickup.transform.rotation = Quaternion.identity;
			fruitPickup.transform.localRotation = Quaternion.identity;

			Object.Destroy(fruitPickup.GetComponent<DestroyOnTimer>());
            Destroy(fruitPickup.GetComponent<BeginRapidlyActivatingAndDeactivating>());
            Destroy(fruitPickup.GetComponent<VelocityRandomOnStart>());
			Destroy(fruitPickup.transform.Find("GravitationController").gameObject);

			var healthPickup = fruitPickup.transform.Find("PickupTrigger").GetComponent<HealthPickup>();
			var foodPickup = fruitPickup.transform.Find("PickupTrigger").gameObject.AddComponent<FoodPickup>();
			foodPickup.baseObject = healthPickup.baseObject;
			foodPickup.teamFilter = healthPickup.teamFilter;
			Destroy(healthPickup);

			fruitPickup.GetComponent<Rigidbody>().freezeRotation = true;

			fruitPickup.AddComponent<ConstantForce>();

			var fruitModel = Instantiate(Resources.Load<GameObject>("prefabs/pickupmodels/PickupFruit"), fruitPickup.transform.Find("HealthOrbEffect"));

			var jar = Resources.Load<GameObject>("prefabs/pickupmodels/PickupWilloWisp");
			
			var jarLid = Object.Instantiate(jar.transform.Find("mdlGlassJar/GlassJarLid"));
			jarLid.parent = fruitModel.transform;
			jarLid.localScale = new Vector3(1f, 1f, 0.3f);
			jarLid.localPosition = new Vector3(0f, -0.9f, 0f);

			foodPickup.modelObject = fruitModel;
		}

        private bool EquipmentSlot_FireFruit(On.RoR2.EquipmentSlot.orig_FireFruit orig, EquipmentSlot self)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
			{
				GameObject pickup = UnityEngine.Object.Instantiate<GameObject>(fruitPickup, self.characterBody.transform.position, UnityEngine.Random.rotation);
				pickup.GetComponent<TeamFilter>().teamIndex = self.teamComponent.teamIndex;
				FoodPickup foodPickup = pickup.GetComponentInChildren<FoodPickup>();
				foodPickup.owner = self.characterBody;
				pickup.transform.localScale = new Vector3(1f, 1f, 1f);
				NetworkServer.Spawn(pickup);
				return true;
            }

            return orig(self);

        }

		public class FoodPickup : MonoBehaviour
		{
			// Token: 0x060010F4 RID: 4340 RVA: 0x000475AC File Offset: 0x000457AC

			private void Start()
            {
				gameObject.transform.rotation = Quaternion.identity;
            }

			private void Update()
			{
				this.localTime += Time.deltaTime;
				if (modelObject)
				{
					Transform transform = this.modelObject.transform;
					Vector3 localEulerAngles = transform.localEulerAngles;
					localEulerAngles.y = this.spinSpeed * this.localTime;
					transform.localEulerAngles = localEulerAngles;
				}
            }

			private void OnTriggerStay(Collider other)
			{
				if (NetworkServer.active && this.alive && TeamComponent.GetObjectTeam(other.gameObject) == this.teamFilter.teamIndex)
				{
					CharacterBody component = other.GetComponent<CharacterBody>();
					if (component != owner)
					{
						var equipmentSlot = component.equipmentSlot;
						if (equipmentSlot)
                        {
							equipmentSlot.FireFruit();
							UnityEngine.Object.Destroy(this.baseObject);
						}
					}
				}
			}

			// Token: 0x04000F18 RID: 3864
			[Tooltip("The base object to destroy when this pickup is consumed.")]
			public GameObject baseObject;

			// Token: 0x04000F19 RID: 3865
			[Tooltip("The team filter object which determines who can pick up this pack.")]
			public TeamFilter teamFilter;

			// Token: 0x04000F1A RID: 3866
			public GameObject pickupEffect;

			// Token: 0x04000F1D RID: 3869
			private bool alive = true;

			public CharacterBody owner;

			public GameObject modelObject;

			public float localTime = 0;

			public float spinSpeed = 55f;

			//public bool ownerCanPickup = false;
		}
	}
}
