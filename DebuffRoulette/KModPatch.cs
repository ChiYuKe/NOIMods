using Database;
using HarmonyLib;
using Klei.AI;
using KMod;
using KModTool;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MinionVitalsPanel;
using static STRINGS.CODEX.STORY_TRAITS.MORB_ROVER_MAKER;

namespace DebuffRoulette
{
    public static class Main
    {
        public class Patch : UserMod2
        {
            public override void OnLoad(Harmony harmony)
            {
                base.OnLoad(harmony);
                harmony.PatchAll();
                ModEntry.Initialize();
               
                Debug.Log("Mod已加载并初始化。");
            }
        }
    }

    internal class NewText
    {
        [HarmonyPatch(typeof(BuildingFacades), MethodType.Constructor, new Type[]
        {
            typeof(ResourceSet)
        })]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Postfix(BuildingFacades __instance)
            {
                
                __instance.Add("paofu", "飞鱼床", "描述", PermitRarity.Universal, "LuxuryBed", "KModelegantBed_puft_kanim", DlcManager.AVAILABLE_ALL_VERSIONS, null);
            }
        }
    }

    public class ModifierSetPatch
    {
        [HarmonyPatch(typeof(ModifierSet), "Initialize")]
        public static class ModifierSet_Initialize_Patch
        {
            public static void Postfix(ModifierSet __instance)
            {
                // 在 ModifierSet 初始化后注册自定义效果
                DeBuff.Register(__instance);
            }
        }
    }


    //[HarmonyPatch(typeof(Game), "OnSpawn")]
    //public static class Game_OnSpawn_Patch
    //{
    //    public static void Postfix()
    //    {
    //        // 启动定时器
    //        RandomDebuffTimerManager.StartTimer();
    //        Debug.Log("定时器已启动。");
    //    }
    //}
}
