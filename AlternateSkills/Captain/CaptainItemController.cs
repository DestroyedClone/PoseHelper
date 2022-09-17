using System;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Linq;

namespace AlternateSkills.Captain
{
	public class CaptainItemController : MonoBehaviour
	{
        public static RoR2.BasicPickupDropTable commonDropList;
        public static RoR2.BasicPickupDropTable uncommonDropList;
        public static RoR2.BasicPickupDropTable legendaryDropList;

		static CaptainItemController()
		{
			Run.onRunStartGlobal += CaptainItemController.OnRunStart;
            commonDropList = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier1Item.asset").WaitForCompletion();
            uncommonDropList = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier2Item.asset").WaitForCompletion();
            legendaryDropList = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier3Item.asset").WaitForCompletion();


		}

        private static void GenerateDropList()
        {
            CaptainItemController.stackRollDataList = new CaptainItemController.StackRollData[]
            {
                new StackRollData()
                {
                    dropTable = commonDropList,
                    stacks = 1,
                    numRolls = CaptainItemController.commonItemCount
                },
                new StackRollData()
                {
                    dropTable = uncommonDropList,
                    stacks = 1,
                    numRolls = CaptainItemController.uncommonItemCount
                },
                new StackRollData()
                {
                    dropTable = legendaryDropList,
                    stacks = 1,
                    numRolls = CaptainItemController.legendaryItemCount
                },
            };
        }

		private static void OnRunStart(Run run)
		{
			CaptainItemController.rng.ResetSeed(run.seed);
		}

		private void Awake()
		{
			this.characterBody = base.GetComponent<CharacterBody>();
		}

		private void Start()
		{
			if (NetworkServer.active)
			{
				this.TryGrantItem();
			}
		}

		private void OnEnable()
		{
			if (NetworkServer.active)
			{
				MasterSummon.onServerMasterSummonGlobal += this.OnServerMasterSummonGlobal;
			}
		}

		private void OnDisable()
		{
			if (NetworkServer.active)
			{
				MasterSummon.onServerMasterSummonGlobal -= this.OnServerMasterSummonGlobal;
			}
		}

		private void TryGrantItem()
		{
			if (this.characterBody.master)
            {
				hasGivenItem = false;
				if (this.characterBody.master.playerStatsComponent)
				{
					hasGivenItem = (this.characterBody.master.playerStatsComponent.currentStats.GetStatValueDouble(PerBodyStatDef.totalTimeAlive, BodyCatalog.GetBodyName(this.characterBody.bodyIndex)) > 0.0);
				}
				if (!hasGivenItem)
				{
                    List<ItemDef> itemDefsGivenList = new List<ItemDef>();
                    Inventory component = base.GetComponent<Inventory>();
                    //stackRollDataList was this. not CaptainItemController. before, check here if htere's any issues.
                    foreach (CaptainItemController.StackRollData stackRollData in CaptainItemController.stackRollDataList)
                    {
                        if (stackRollData.dropTable)
                        {
                            for (int j = 0; j < stackRollData.numRolls; j++)
                            {
                                PickupDef pickupDef = PickupCatalog.GetPickupDef(stackRollData.dropTable.GenerateDrop(CaptainItemController.rng));
                                component.GiveItem(pickupDef.itemIndex, stackRollData.stacks);
                            }
                        }
                    }
                    itemsDefsGiven = itemsDefsGiven.ToArray();
				}
			}
		}

		private void OnServerMasterSummonGlobal(MasterSummon.MasterSummonReport summonReport)
		{
			if (this.characterBody.master && this.characterBody.master == summonReport.leaderMasterInstance)
			{
				CharacterMaster summonMasterInstance = summonReport.summonMasterInstance;
				if (summonMasterInstance)
				{
					CharacterBody body = summonMasterInstance.GetBody();
					if (body && (body.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
					{
                        foreach (var itemToGive in itemsDefsGiven)
                        {
						    summonMasterInstance.inventory.GiveItem(itemToGive);
                        }
					}
				}
			}
		}
        public bool hasGivenItem = false;

        public static int commonItemCount = 3;
        public static int uncommonItemCount = 1;
        public static int legendaryItemCount = 0;

        public ItemDef[] itemsDefsGiven;

		private CharacterBody characterBody;

		public static CaptainItemController.StackRollData[] stackRollDataList;

		private static readonly Xoroshiro128Plus rng = new Xoroshiro128Plus(0UL);

		[Serializable]
		public struct StackRollData
		{
			public PickupDropTable dropTable;

			public int stacks;

			public int numRolls;
		}

        public class CaptainItemControllerItemGiven : MonoBehaviour
        {

        }
	}
}
