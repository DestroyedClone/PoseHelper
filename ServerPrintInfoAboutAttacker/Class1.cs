using BepInEx;
using BepInEx.Logging;
using RoR2;
using System.Text;
using UnityEngine.Networking;

namespace ServerPrintInfoAboutAttacker
{
    [BepInPlugin("com.DestroyedClone.ServerPrintInfoAboutAttacker", "Server Print Info About Attacker", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        internal static ManualLogSource _logger;

        public void Awake()
        {
            _logger = base.Logger;
            GlobalEventManager.onCharacterDeathGlobal += this.GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            static void ReturnBuffList(CharacterBody body, ref string buffList, ref string debuffList)
            {
                BuffIndex buffIndex = (BuffIndex)0;
                BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount;
                while (buffIndex < buffCount)
                {
                    int buffCount2 = body.GetBuffCount(buffIndex);
                    bool flag3 = buffCount2 > 0;
                    if (flag3)
                    {
                        BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
                        string @string = Language.GetString(buffDef.name);
                        string text5 = @string ?? "";
                        bool canStack = buffDef.canStack;
                        if (canStack)
                        {
                            text5 += string.Format(" ({0})", buffCount2);
                        }
                        text5 += ", ";
                        bool isDebuff = buffDef.isDebuff;
                        if (isDebuff)
                        {
                            debuffList += text5;
                        }
                        else
                        {
                            buffList += text5;
                        }
                    }
                    buffIndex++;
                }
            }
            bool flag = NetworkServer.active && damageReport.victimMaster && damageReport.victimMaster.playerCharacterMasterController && damageReport.victimBody;
            if (!flag)
            {
                return;
            }
            StringBuilder output = new StringBuilder();
            output.AppendLine($"[{Run.instance.NetworkfixedTime}] Info about player death: ");
            output.AppendLine($"Stage: {Language.GetString(SceneCatalog.GetSceneDefForCurrentScene().nameToken)}|" +
                $" Run Time: {Run.instance.fixedTime}|");
            CharacterBody victimBody = damageReport.victimBody;
            HealthComponent healthComponent = damageReport.victimBody.healthComponent;

            output.AppendLine($"==Victim: {(victimBody ? victimBody.GetUserName() : "Unknown Player")}");
            string victimBuffs = "Buffs: ";
            string victimDebuffs = "Debuffs: ";
            ReturnBuffList(victimBody, ref victimBuffs, ref victimDebuffs);
            output.AppendLine(victimBuffs);
            output.AppendLine(victimDebuffs);
            output.AppendLine($"Armor: {victimBody.armor}");
            output.AppendLine($"Damage Dealt: {damageReport.damageDealt}");
            output.AppendLine($"Damage Type(s): {damageReport.damageInfo.damageType}");
            var damageInfoType = damageReport.damageInfo.damageType;
            DamageType damageType = (DamageType)0;
            DamageType damageTypeCount = (DamageType)DamageType.GetNames(typeof(DamageType)).Length;
            while (damageType < damageTypeCount)
            {
                if (damageInfoType.HasFlag(damageType))
                {
                    output.Append($"{damageType}, ");
                }
                damageType++;
            }

            output.AppendLine($"Combined Health Before Damage: {damageReport.combinedHealthBeforeDamage}");
            output.AppendLine($"{damageReport.victimBody}");
            output.AppendLine($"Resulting Health: {healthComponent.health}");
            Inventory inventory = null;
            string attackerDamage = "???";
            string text3 = "Buffs: ";
            string text4 = "Debuffs: ";
            string attackerName = "???";
            bool flag2 = damageReport.attackerMaster && damageReport.attackerBody;
            if (flag2)
            {
                attackerName = damageReport.attackerBody.GetDisplayName();
                inventory = damageReport.attackerBody.inventory;
                attackerDamage = damageReport.attackerBody.damage.ToString();
                ReturnBuffList(damageReport.attackerBody, ref text3, ref text4);
            }
            else
            {
            }
            output.AppendLine($"==Attacker: {attackerName}");
            output.AppendLine($"DamageStat: {attackerDamage}");
            output.AppendLine(text3);
            output.AppendLine(text4);
            bool flag4 = inventory != null;
            if (flag4)
            {
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(inventory.currentEquipmentIndex);
                output.AppendLine($"Equipment: {(equipmentDef ? Language.GetString(equipmentDef.nameToken) : "None")}");
                output.AppendLine("Items: ");
                ItemIndex itemIndex = (ItemIndex)0;
                ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount;
                while (itemIndex < itemCount)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                    bool flag5 = itemDef && inventory.GetItemCount(itemDef) > 0;
                    if (flag5)
                    {
                        string string2 = Language.GetString(itemDef.nameToken);
                        if (string.IsNullOrEmpty(string2)) string2 = itemDef.name;
                        int itemCount2 = inventory.GetItemCount(itemDef);
                        output.Append(string.Format("{0} ({1}), ", string2, itemCount2));
                    }
                    itemIndex++;
                }
            }
            Main._logger.LogMessage(output);
        }
    }
}