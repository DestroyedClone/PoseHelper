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
            CaptainItemController.commonDropList = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier1Item.asset").WaitForCompletion();
            CaptainItemController.uncommonDropList = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier2Item.asset").WaitForCompletion();
            CaptainItemController.legendaryDropList = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/Base/Common/dtTier3Item.asset").WaitForCompletion();

			GenerateDropList();
		}

        private static void GenerateDropList()
        {
            CaptainItemController.stackRollDataList = new CaptainItemController.StackRollData[]
            {
                new StackRollData()
                {
                    dropTable = CaptainItemController.commonDropList,
                    stacks = 1,
                    numRolls = CaptainItemController.commonItemCount
                },
                new StackRollData()
                {
                    dropTable = CaptainItemController.uncommonDropList,
                    stacks = 1,
                    numRolls = CaptainItemController.uncommonItemCount
                },
                new StackRollData()
                {
                    dropTable = CaptainItemController.legendaryDropList,
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
                    List<ItemIndex> itemsGivenList = new List<ItemIndex>();
                    Inventory component = characterBody.inventory;
                    //stackRollDataList was this. not CaptainItemController. before, check here if htere's any issues.
                    try {
					foreach (CaptainItemController.StackRollData stackRollData in stackRollDataList)
                    {
                        if (stackRollData.dropTable)
                        {
                            for (int j = 0; j < stackRollData.numRolls; j++)
                            {
                                PickupDef pickupDef = PickupCatalog.GetPickupDef(stackRollData.dropTable.GenerateDrop(CaptainItemController.rng));
                                component.GiveItem(pickupDef.itemIndex, stackRollData.stacks);
								itemsGivenList.Add(pickupDef.itemIndex);
                            }
                        }
                    }
					} catch {
						//using Inventory.GiveRandomItems
						MainPlugin._logger.LogWarning("Item controller still fucking failed.");
						
						WeightedSelection<List<PickupIndex>> whiteWS = new WeightedSelection<List<PickupIndex>>(1);
						whiteWS.AddChoice(Run.instance.availableTier1DropList, 100f);

						WeightedSelection<List<PickupIndex>> greenWS = new WeightedSelection<List<PickupIndex>>(1);
						greenWS.AddChoice(Run.instance.availableTier2DropList, 100f);

						WeightedSelection<List<PickupIndex>> redWS = new WeightedSelection<List<PickupIndex>>(1);
						redWS.AddChoice(Run.instance.availableTier3DropList, 100f);

						void GiveItem(PickupDef pickupDef)
						{
							var itemIndexToGive = (pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None;
							if (itemIndexToGive != ItemIndex.None)
							{
								characterBody.inventory.GiveItem(itemIndexToGive);
								itemsGivenList.Add(itemIndexToGive);
							}
							
						}
						
						for (int i = 0; i < commonItemCount; i++)
						{
							List<PickupIndex> list = whiteWS.Evaluate(UnityEngine.Random.value);
							PickupDef pickupDef = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]);
							GiveItem(pickupDef);
						}

						for (int i = 0; i < uncommonItemCount; i++)
						{
							List<PickupIndex> list = greenWS.Evaluate(UnityEngine.Random.value);
							PickupDef pickupDef = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]);
							GiveItem(pickupDef);
						}

						for (int i = 0; i < legendaryItemCount; i++)
						{
							List<PickupIndex> list = redWS.Evaluate(UnityEngine.Random.value);
							PickupDef pickupDef = PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]);
							GiveItem(pickupDef);
						}

					}
					itemsGiven = itemsGivenList.ToArray();
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
                        foreach (var itemToGive in itemsGiven)
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

        public ItemIndex[] itemsGiven;

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
