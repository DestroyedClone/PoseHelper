using BepInEx;
using EntityStates.Missions.BrotherEncounter;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BrotherPhaseSkip
{
    [BepInPlugin("com.DestroyedClone.MithrixPhaseSkip", "Mithrix Phase Skip", "1.0.1")]
    public class Class1 : BaseUnityPlugin
    {
        public void Start()
        {
            bool bind(int phaseNum, bool startingValue)
            {
                return Config.Bind("", $"Skip Phase {phaseNum}", startingValue).Value;
            }

            if (bind(1, false))
            {
                On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += (orig, self) =>
                {
                    orig(self);
                    self.PreEncounterBegin();
                    self.outer.SetNextState(new Phase2());
                };
            }
            if (bind(2, true))
            {
                On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter += (orig, self) =>
                {
                    orig(self);
                    if (Config.Bind("","Add Phase 2 Pillars", true, "If true, then the pillars will raise.").Value)
                    {
                        foreach (var pillar in self.pillarsToActive)
                        {
                            pillar.SetActive(true);
                        }
                    }
                    self.outer.SetNextState(new Phase3());
                };
            }
            if (bind(3, false))
            {
                On.EntityStates.Missions.BrotherEncounter.Phase3.OnEnter += (orig, self) =>
                {
                    orig(self);
                    self.outer.SetNextState(new Phase4());
                };
            }
            if (bind(4, false))
            {
                On.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter += (orig, self) =>
                {
                    orig(self);
                    self.outer.SetNextState(new BossDeath());
                };
            }
        }
    }
}