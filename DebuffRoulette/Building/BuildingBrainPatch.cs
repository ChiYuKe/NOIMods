

using Database;
using HarmonyLib;
using KModTool;
using PeterHan.PLib.UI;
using System;
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




        [HarmonyPatch(typeof(ChoreTypes))]
        [HarmonyPriority(Priority.First)]
        public static class AddNewChorePatch
        {
            public static ChoreType Accepttheinheritance;

            [HarmonyPostfix]
            [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(ResourceSet) })]
            public static void Postfix(ChoreTypes __instance)
            {
                if (__instance != null)
                {
                    MethodInfo addMethod = typeof(ChoreTypes).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (addMethod != null)
                    {
                       
                        object[] parameters = new object[]
                        {
                            "Accepttheinheritance", 
                            new string[0],           
                            "",                     
                            new string[0],           
                            "使用记忆迁移机",
                            "使用记忆迁移机",
                            "这个复制人正在接受记忆传承！！",
                            false,                   
                            -1,                      
                            null                    
                        };
                        Accepttheinheritance = (ChoreType)addMethod.Invoke(__instance, parameters);

                      
                    }
                }
            }
        }










        //public static class Buildpatch
        //{
        //    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        //    public static class OilWellCap_1LoadGeneratedBuildings_Patch
        //    {

        //        public static void Prefix()
        //        {
        //            ModUtil.AddBuildingToPlanScreen("Base", BuildingBrainConfig.ID, "Tiles");
        //            Db.Get().Techs.Get("HighTempForging").unlockedItemIDs.Add(BuildingBrainConfig.ID);
        //            KModStringUtils.Add_New_BuildStrings(BuildingBrainConfig.ID, "迁移测试机", "记忆的传承", "死亡不可怕，被人遗忘才可怕");
        //        }
        //    }
        //}
    }
}
