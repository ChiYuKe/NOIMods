using HarmonyLib;
using Klei.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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


    [HarmonyPatch(typeof(MinionConfig))]
    public static class AddMinionTraitsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("AddMinionTraits")]
        public static void Postfix(string name, Modifiers modifiers)
        {
            // 确保 Db 和 Amounts.Age 不为空
            var db = Db.Get();
            if (db != null && db.Amounts != null && db.Amounts.Age != null)
            {

                // 获取 Trait 对象
                var trait = db.traits.Get(MinionConfig.MINION_BASE_TRAIT_ID);
                if (trait != null)
                {
                    // 检查是否已存在此 AttributeModifier
                    bool alreadyExists = trait.SelfModifiers.Any(modifier => modifier.AttributeId == db.Amounts.Age.maxAttribute.Id);

                    if (!alreadyExists)
                    {
                      
                        // 添加 AttributeModifier 到 Trait
                       trait.Add(new AttributeModifier(db.Amounts.Age.maxAttribute.Id, RandomDebuffTimerManager.KModminionAge, name, false, false, true));
                       trait.Add(new AttributeModifier(db.Amounts.Age.deltaAttribute.Id, 1 / 600f, name, false, false, true));
                        Debug.Log($"已添加 AttributeModifier");
                    }
                    else
                    {
                        Debug.LogWarning($"已存在 AttributeModifier：{db.Amounts.Age.maxAttribute.Id}，不会重复添加");
                    }
                }
                else
                {
                    Debug.LogWarning("Trait 对象为空");
                }
            }
            else
            {
                Debug.LogWarning("Db 或 Amounts.Age 为空");
            }
        }
    }

    [HarmonyPatch(typeof(Database.Amounts))]
    [HarmonyPriority(Priority.First)]
    public static class AddAmountPatch
    {
        public static Amount newAmount;  // 静态变量保存创建的 Amount

        [HarmonyPostfix]
        [HarmonyPatch("Load")]
        public static void Postfix(Database.Amounts __instance)
        {
            if (__instance != null)
            {
                string id = "NewTest";
                float min = 0f;
                float max = 100f;
                bool show_max = true;
                Units units = Units.Flat;
                float delta_threshold = 0.1675f;
                bool show_in_ui = true;
                string string_root = "测试";
                string uiSprite = "ui_icon_age";

                newAmount = __instance.CreateAmount(id, min, max, show_max, units, delta_threshold, show_in_ui, string_root, uiSprite);
                Debug.Log($"新 Amount 对象 {id} 已添加");
            }
            else
            {
                Debug.LogWarning("Amounts 对象为空");
            }
        }
    }

















}
