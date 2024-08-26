//using HarmonyLib;
//using KModTool;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using HarmonyLib;
using PeterHan.PLib.UI;
using System.Reflection;
using System.Text;
using UnityEngine;
using static STRINGS.CODEX.STORY_TRAITS.MORB_ROVER_MAKER;

namespace DebuffRoulette
{
    internal class ModPatch
    {
        [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
        public static class DetailsScreenPatch
        {
            public static void Postfix()
            {
                PUIUtils.AddSideScreenContent<BuildingBrainSideScreen>();
              

            }
        }

        //        public static class Buildpatch
        //        {
        //            [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        //            public static class OilWellCap_1LoadGeneratedBuildings_Patch
        //            {

        //                public static void Prefix()
        //                {
        //                    ModUtil.AddBuildingToPlanScreen("Base", "KomdBuildingBrain", "Tiles");
        //                    Db.Get().Techs.Get("HighTempForging").unlockedItemIDs.Add("KomdBuildingBrain");
        //                    KModStringUtils.Add_New_BuildStrings("KomdBuildingBrain", "迁移测试机", "记忆的传承", "死亡不可怕，被人遗忘才可怕");
        //                }
        //            }
        //        }
    }
}
