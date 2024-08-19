using HarmonyLib;
using System;
using Klei.AI;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Database;
using System.Reflection;

namespace DebuffRoulette
{

    [HarmonyPatch(typeof(MinionConfig))]
    public static class AddMinionAmountsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("AddMinionAmounts")] // 替换为实际的方法名
        public static void Postfix(Modifiers modifiers)
        {
            // 确保 Db 和 Amounts.Age 不为空
            if (Db.Get() != null && Db.Get().Amounts != null && Db.Get().Amounts.Age != null)
            {
                if (modifiers.initialAmounts != null)
                {
                    modifiers.initialAmounts.Add(Db.Get().Amounts.Age.Id);
                  
                }
            }
        }
    }

    [HarmonyPatch(typeof(MinionModifiers), "OnDeath")]
    public static class MinionModifiers_OnDeath_Patch
    {
        static bool Prefix(MinionModifiers __instance, object data)
        {
            // 使用 __instance 获取死亡对象
            GameObject deathObject = __instance.gameObject;
           
            if (deathObject == null)
            {
                Debug.LogError("OnDeath: data 不是一个 GameObject 对象。");
                return true; // 返回 true 继续执行原方法
            }

            Debug.Log($"OnDeath: {deathObject.name} 收到死亡事件。");

            // 判断死亡对象是否具有特定标签（如 "NoMourning"）
            if (deathObject.HasTag("KModNoMourning"))
            {
                // 如果有这个标签，终止后续的 Mourning 效果添加
                Debug.Log($"{deathObject.name} 拥有 'NoMourning' 标签，跳过 Mourning 效果。");
                return false; // 返回 false 跳过原方法
            }

            // 否则允许原方法执行
            return true;
        }
    }





    [HarmonyPatch(typeof(MinionConfig))]
    public static class AddMonionTraitsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("AddMinionTraits")]
        public static void Postfix(string name, Modifiers modifiers)
        {
            // 确保 Db 和 Amounts.Age 不为空
            var db = Db.Get();
            if (db?.Amounts?.Age == null)
            {
                Debug.LogWarning("Db 或 Amounts.Age 为空，无法添加 AttributeModifier。");
                return;
            }

            // 获取 Trait 对象
            var trait = db.traits.Get(MinionConfig.MINION_BASE_TRAIT_ID);
            if (trait == null)
            {
                Debug.LogWarning("Trait 对象为空，无法添加 AttributeModifier。");
                return;
            }

            // 检查是否已存在此 AttributeModifier
            if (trait.SelfModifiers.Any(modifier => modifier.AttributeId == db.Amounts.Age.maxAttribute.Id))
            {
                Debug.LogWarning($"Trait 中已存在 AttributeModifier：{db.Amounts.Age.maxAttribute.Id}，不会重复添加。");
                return;
            }

            // 添加 AttributeModifier 到 Trait
            trait.Add(new AttributeModifier(db.Amounts.Age.maxAttribute.Id, RandomDebuffTimerManager.KModminionAge, name, false, false, true));
            trait.Add(new AttributeModifier(db.Amounts.Age.deltaAttribute.Id, 1 / 600f, name, false, false, true));
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

    [HarmonyPatch(typeof(Database.Amounts), "Load")]
    [HarmonyPriority(Priority.First)]
    public static class AddAmountPatch
    {
        public static Amount MiniAgeAmount; 

        [HarmonyPostfix]
        public static void Postfix(Database.Amounts __instance)
        {
            if (__instance != null)
            {
                string id = "MiniAge";
                float min = 0f;
                float max = 100f;
                bool showMax = true;
                Units units = Units.Flat;
                float deltaThreshold = 0.1675f;
                bool showInUI = true;
                string stringRoot = "STRINGS.CREATURES";
                string uiSprite = "ui_icon_age";

                MiniAgeAmount = __instance.CreateAmount(id, min, max, showMax, units, deltaThreshold, showInUI, stringRoot, uiSprite);
                Debug.Log($"新 Amount 对象 {id} 已添加");
            }
            else
            {
                Debug.LogWarning("Amounts 对象为空");
            }
        }
    }


    [HarmonyPatch(typeof(MinionConfig))]
    public static class AddMinionTagPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreatePrefab")] 
        public static void Postfix(GameObject __result)
        {
            if (__result != null)
            {
                Tag modTag = new Tag("ShowModifiedAge");
                // 添加标签，为了确保年龄描述正常显示
                __result.AddOrGet<KPrefabID>().AddTag(modTag, false);
            }
        }
    }


    [HarmonyPatch(typeof(MinionVitalsPanel))]
    [HarmonyPatch("Refresh")]
    public static class MinionVitalsPanelRefreshPatch
    {
        public static void Postfix(MinionVitalsPanel __instance, GameObject selectedEntity)
        {
            var amountsLines = Traverse.Create(__instance).Field("amountsLines").GetValue<List<MinionVitalsPanel.AmountLine>>();
            if (amountsLines == null || selectedEntity == null) return;

            Klei.AI.Amounts amounts = selectedEntity.GetAmounts();
            if (amounts == null) return;

            KPrefabID prefabID = selectedEntity.GetComponent<KPrefabID>();
            bool hasShowModifiedAgeTag = prefabID != null && prefabID.HasTag("ShowModifiedAge");

            //不是复制人就跳过文本替换
            if (!hasShowModifiedAgeTag) return;
            foreach (var amountLine in amountsLines)
            {
                if (amountLine.amount.Id == "Age")
                {
                    AmountInstance ageInstance = amounts.Get(amountLine.amount);
                    if (ageInstance != null)
                    {
                        
                        string customAgeText = amountLine.amount.GetDescription(ageInstance).Replace("年龄", "复制人年龄");
                        string customAgeTooltip = amountLine.toolTipFunc(ageInstance).Replace(
                            "这只小动物在<style=\"KKeyword\">年龄</style>到达物种寿命上限时就会死去",
                            "复制人我啊........ \n\n 到点就彻底死了捏");

                        amountLine.locText.SetText(customAgeText);
                        amountLine.toolTip.toolTip = customAgeTooltip;
                    }
                }
            }
        }
    }








    
}
