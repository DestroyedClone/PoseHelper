using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace BanditItemlet
{
    [BepInPlugin("com.DestroyedClone.PartialPayment", "Partial Payment", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class PartialPayment : BaseUnityPlugin
    {
        public void Start()
        {
            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += PurchaseInteraction_CanBeAffordedByInteractor;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            var master = activator.GetComponent<CharacterBody>()?.master;
            if (master)
            {
                if (self.costType == CostTypeIndex.Money)
                {
                    var originalCost = self.Networkcost;
                    var money = master.money;
                    var result = originalCost - money;
                    if (result > 0) //purchaser has less than the required amount $25 - $5 = $20
                    {
                        DamageNumberManager.instance.SpawnDamageNumber(money, self.transform.position, false, TeamIndex.None, DamageColorIndex.Item);
                        self.Networkcost = (int)result;
                        master.money = 0;
                    }
                }
            }

            orig(self, activator);
        }

        private bool PurchaseInteraction_CanBeAffordedByInteractor(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
        {
            var original = orig(self, activator);
            if (self.costType == CostTypeIndex.Money && activator?.GetComponent<CharacterBody>()?.master?.money > 0)
            {
                var highlight = self.gameObject.GetComponent<Highlight>();
                if (!original)
                {
                    highlight.highlightColor = Highlight.HighlightColor.teleporter;
                    return true;
                }
                else
                {
                    highlight.highlightColor = Highlight.HighlightColor.interactive;
                }
            }
            return original;
        }
    }
}