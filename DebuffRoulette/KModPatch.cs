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

    [HarmonyPatch(typeof(Deaths))]
    [HarmonyPriority(Priority.First)]
    public static class AddNewDeathPatch
    {
        public static Death customDeath;  

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(ResourceSet) })]
        public static void Postfix(Deaths __instance)
        {
           
            if (__instance != null)
            {
                
                string id = "CustomDeath";
                string name = "老死";
                string description = "{Target} 固有一死，或重于泰山，或轻如鸿毛";
                string animation1 = "dead_on_back";
                string animation2 = "dead_on_back"; 

                customDeath = new Death(id, __instance, name, description, animation1, animation2);
                __instance.Add(customDeath);
                Debug.Log($"新 Death 类型 {name} 已添加");
            }
            else
            {
                Debug.LogWarning("Deaths 实例为空");
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
                Buff.Register(__instance);
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
