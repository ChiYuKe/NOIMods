using HarmonyLib;
using KMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
