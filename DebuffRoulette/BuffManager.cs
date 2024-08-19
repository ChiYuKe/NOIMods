using Database;
using Klei.AI;
using KModTool;
using STRINGS;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebuffRoulette
{
   
    internal class DeBuff
    {
       
        public static void Register(ModifierSet parent)
        {
            Database.Attributes attributes = Db.Get().Attributes;
            Database.Amounts amounts = Db.Get().Amounts;
            new KModEffectConfigurator("shuailao", RandomDebuffTimerManager.shuailaoDebufftime, false)
               .SetEffectName(UI.FormatAsHotkey("衰老"))
               .SetEffectDescription("人老难免有不中用的时候")
               .AddAttributeModifier(attributes.Athletics.Id, -6f, false, false, true)// 运动
               .AddAttributeModifier(attributes.Strength.Id, -5f, false, false, true)//力量
               .AddAttributeModifier(attributes.Digging.Id, -5f, false, false, true)// 挖掘
               .AddAttributeModifier(attributes.Immunity.Id, -2f, false, false, true)// 免疫系统
               .ApplyTo(parent);


            new KModEffectConfigurator("debuff1", 3600f, false)
              .SetEffectName("测试1")
              .SetEffectDescription("这是DeBuff添加测试")

              .ApplyTo(parent);


            new KModEffectConfigurator("debuff2", 3600f, false)
           .SetEffectName("测试2")
           .SetEffectDescription("这是DeBuff添加测试")

           .ApplyTo(parent);


            new KModEffectConfigurator("debuff3", 3600f, false)
           .SetEffectName("测试3")
           .SetEffectDescription("这是DeBuff添加测试")

           .ApplyTo(parent);


            new KModEffectConfigurator("debuff4", 3600f, false)
           .SetEffectName("测试4")
           .SetEffectDescription("这是DeBuff添加测试")

           .ApplyTo(parent);



        }


        // 应用衰老效果
        public static void ApplyDebuff(GameObject gameObject)
        {
            Effects effectsComponent = gameObject.GetComponent<Effects>();
            if (effectsComponent != null && !effectsComponent.HasEffect("shuailao")) // 如果没有衰老效果
            {
                effectsComponent.Add("shuailao", true); // 添加衰老效果
                NotifyDeathApplied(gameObject); // 通知衰老效果已应用
            }
        }


        public static void ApplyRandomDebuff(HashSet<GameObject> cachedMinionGameObjects)
        {
            List<string> debuffTypes = new List<string> { "debuff1", "debuff2", "debuff3", "debuff4" };
            int minionCount = cachedMinionGameObjects.Count;
            int numToSelect = 3;

            if (minionCount >= numToSelect)
            {
                // 打乱小人列表，随机选择前 numToSelect 个小人
                List<GameObject> selectedMinions = cachedMinionGameObjects.OrderBy(x => UnityEngine.Random.value).Take(numToSelect).ToList();

                foreach (GameObject gameObject in selectedMinions)
                {
                    if (gameObject == null) continue;

                    Klei.AI.Effects effectsComponent = gameObject.GetComponent<Klei.AI.Effects>();
                    if (effectsComponent == null) continue;

                    // 随机整一个 Debuff给小人
                    string randomDebuff = debuffTypes[UnityEngine.Random.Range(0, debuffTypes.Count)];

                    if (!effectsComponent.HasEffect(randomDebuff))
                    {
                        effectsComponent.Add(randomDebuff, true);
                        NotifyDebuffApplied1(gameObject); // 通知玩家谁被添加了DeBuff

                    }
                }
            }
            else
            {
                Debug.Log("复制人数不足3人，不添加 Debuff。");
            }
        }


        private static void NotifyDebuffApplied1(GameObject gameObject)
        {
            Notifier notifier = gameObject.AddOrGet<Notifier>();
            Notification notification = new Notification(
                DebuffRoulette.STRINGS.MISC.NOTIFICATIONS.DEBUFFROULETTE.NAME,
                NotificationType.MessageImportant,
                (notificationList, data) => notificationList.ReduceMessages(false),
                "/t• " + gameObject.GetProperName(), true, 0f, null, null, null, true, false, false
            );
            notifier.Add(notification, "");
        }


        // 通知衰老效果应用
        private static void NotifyDeathApplied(GameObject gameObject)
        {
            Notifier notifier = gameObject.AddOrGet<Notifier>();
            Notification notification = new Notification(
                DebuffRoulette.STRINGS.MISC.NOTIFICATIONS.DEATHROULETTE.NAME, // 通知标题
                NotificationType.MessageImportant, // 通知类型
                (notificationList, data) => notificationList.ReduceMessages(false), // 通知处理函数
                "/t• " + gameObject.GetProperName(), // 通知内容
                true, 0f, null, null, null, true, false, false
            );
            notifier.Add(notification, ""); // 添加通知
        }

    }
}