using BepInEx;
using R2API.Utils;
using R2API;
using UnityEngine.Networking;
using RoR2;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using Path = System.IO.Path;
using R2API.Networking;
using UnityEngine.Playables;
using System;
using static UnityEngine.ScriptableObject;

namespace LeBuilder
{
    public class PastInfo
    {
        //https://github.com/harbingerofme/DebugToolkit/blob/bd0be518320ee83bfac59a0801cbbbc4801f9618/Code/DT-Commands/CurrentRun.cs#L70-L84
        public void NoEnemies(bool noEnemies)
        {
            typeof(CombatDirector).GetFieldValue<RoR2.ConVar.BoolConVar>("cvDirectorCombatDisable").SetBool(noEnemies);
            if (noEnemies)
            {
                SceneDirector.onPrePopulateSceneServer += OnPrePopulateSetMonsterCreditZero;
            }
            else
            {
                SceneDirector.onPrePopulateSceneServer -= OnPrePopulateSetMonsterCreditZero;
            }
        }

        //https://github.com/harbingerofme/DebugToolkit/blob/ef081b609837de8971363e0bd791b28795cf62c4/Code/Hooks.cs#L270-L274
        internal static void OnPrePopulateSetMonsterCreditZero(SceneDirector director)
        {
            //Note that this is not a hook, but an event subscription.
            director.monsterCredit = 0;
        }
    }


}
