using HarmonyLib;
using KMod;
using KModTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    [HarmonyPatch(typeof(EntityConfigManager), "LoadGeneratedEntities")]
    public class New_Plant_Database_Description
    {
        public static void Prefix()
        {
            KModStringUtils.Add_New_Deaths_Strings("KModCombat", STRINGS.DUPLICANTS.DEATHS.NAME, STRINGS.DUPLICANTS.DEATHS.DESCRIPTION);


        }
    }



    //[HarmonyPatch(typeof(MinionPersonalityPanel), "RefreshBioPanel")]
    //public static class RefreshBioPanelPatch
    //{

    //    // 使用 Postfix 在原方法执行后插入代码
    //    public static void Postfix(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
    //    {
    //        MinionIdentity minionIdentity = targetEntity.GetComponent<MinionIdentity>();
    //        float currentCycle = (float)GameClock.Instance.GetCycle();
    //        float minionAge = (currentCycle - minionIdentity.arrivalTime) * 600f;
    //        float num1  = 17 * 600f - minionAge;
    //        // 插入新的标签
    //        targetPanel.SetLabel("zhuangtai：", "衰老死亡时间：" + num1, "");
    //    }
    //}




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
