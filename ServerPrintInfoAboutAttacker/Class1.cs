using BepInEx;
using RoR2;
using UnityEngine.Networking;

namespace ServerPrintInfoAboutAttacker
{
    [BepInPlugin("com.DestroyedClone.ServerPrintInfoAboutAttacker", "Server Print Info About Attacker", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource _logger;

        public void Awake()
        {
            _logger = Logger;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (NetworkServer.active && damageReport.victimMaster && damageReport.victimMaster.playerCharacterMasterController)
            {
                //string victimBodyName = damageReport.victim?.body.baseNameToken ?? "Unknown Body Type";

                string attackerName = "Unknown Attacker";
                Inventory attackerInventory = null;
                string damageStat = "???";
                string buffList = "\nBuffs: ";
                string debuffList = "\nDebuffs: ";
                if (damageReport.attackerMaster && damageReport.attackerBody)
                {
                    attackerName = damageReport.attackerBody.GetDisplayName();
                    attackerInventory = damageReport.attackerBody.inventory;
                    damageStat = damageReport.attackerBody.damage.ToString();

                    #region buffs

                    BuffIndex buffIndex = (BuffIndex)0;
                    BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
                    while (buffIndex < buffCount)
                    {
                        var buffCount2 = damageReport.attackerBody.GetBuffCount(buffIndex);
                        if (buffCount2 > 0)
                        {
                            BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);

                            var name = Language.GetString(buffDef.name);
                            var text = $"{name}";
                            if (buffDef.canStack)
                            {
                                text += $" ({buffCount2})";
                            }
                            text += ", ";

                            if (buffDef.isDebuff)
                            {
                                debuffList += text;
                            }
                            else
                            {
                                buffList += text;
                            }
                        }
                        buffIndex++;
                    }

                    #endregion buffs
                }

                var message = $"[{Run.instance.NetworkfixedTime}]Info about player death: " +
                    $"\n==Victim: {damageReport.victimBody?.GetUserName() ?? "Unknown Player"}" +
                    $"\nTime Since Last Hit:{damageReport.victimBody.healthComponent?.timeSinceLastHit}" +
                    $"\nDamage Dealt: {damageReport.damageDealt}" +
                    $"\nCombined Health Before Damage: {damageReport.combinedHealthBeforeDamage}" +
                    $"\n{damageReport.victimBody}" +
                    $"\nResulting Health: {damageReport.victimBody?.healthComponent?.health.ToString() ?? "Unknown"}" +
                    $"\n==Attacker: {attackerName}" +
                    $"\nDamageStat: {damageStat}";
                message += buffList + debuffList;

                if (attackerInventory != null)
                {
                    var equipmentDef = EquipmentCatalog.GetEquipmentDef(attackerInventory.currentEquipmentIndex);
                    message += $"\nEquipment: {(equipmentDef ? Language.GetString(equipmentDef.nameToken) : "None")}";
                    message += "\nItems: ";

                    //int num = 0;
                    ItemIndex itemIndex = (ItemIndex)0;
                    ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount;
                    while (itemIndex < itemCount)
                    {
                        var itemDef = ItemCatalog.GetItemDef(itemIndex);
                        if (itemDef && attackerInventory.GetItemCount(itemDef) > 0)
                        {
                            var itemName = Language.GetString(itemDef.nameToken);
                            var itemCount2 = attackerInventory.GetItemCount(itemDef);
                            message += $"{itemName} ({itemCount2}), ";
                        }
                        itemIndex++;
                    }
                }
                _logger.LogMessage(message);
            }
        }
    }
}