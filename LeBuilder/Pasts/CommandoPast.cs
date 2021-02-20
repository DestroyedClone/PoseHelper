using BepInEx;
using UnityEngine;
using RoR2;
using R2API.Utils;
using System;
using R2API;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace LeBuilder.Pasts
{
    public class CommandoPast : PastDef
    {
        public override DirectorAPI.Stage Stage => DirectorAPI.Stage.TitanicPlains;
        public override string StageName => "Traumatic Experience";
        public override string StageDesc => "witty tagline";
        public override string Soundtrack => "";
        protected override DirectorAPI.DirectorCardHolder[] AllowedSpawns()
        {
            DirectorCard bisonDC = new DirectorCard
            {
                spawnCard = Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscBison"),
                selectionWeight = 1,
                allowAmbushSpawn = true,
                preventOverhead = false,
                minimumStageCompletions = 0,
                requiredUnlockable = "",
                forbiddenUnlockable = "",
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard
            };
            DirectorAPI.DirectorCardHolder bisonCard = new DirectorAPI.DirectorCardHolder
            {
                Card = bisonDC,
                MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
                InteractableCategory = DirectorAPI.InteractableCategory.None
            };
            return new DirectorAPI.DirectorCardHolder[1] { bisonCard };
        }
        public override void BuildScene()
        {
            CreateFaker();
        }

        private void CreateFaker()
        {

        }
    }
}
