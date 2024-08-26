using Database;
using Epic.OnlineServices.Logging;
using Klei.AI;
using KModTool;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebuffRoulette
{
    public static class RandomDebuffTimerManager
    {
        // 定时器配置
        private static readonly float TimerInterval = 600f; // 每600秒触发一次
        private static float nextExecutionTime = 0f; // 下次执行时间
        private static float lastUpdateTime = 0f; // 上次更新的时间

        // 复制人对象相关
        private static HashSet<GameObject> cachedMinionGameObjects = new HashSet<GameObject>(); // 缓存的复制人对象集合
        private static HashSet<GameObject> deadMinions = new HashSet<GameObject>(); // 已死亡的复制人集合
        private static int cachedMinionCount = 0; // 缓存的复制人数量

        // 复制人年龄相关
        public static float MinionAgeThreshold = 1f; // 复制人年龄阈值（单位：分钟）
        private static float AgeThreshold = MinionAgeThreshold * 600f; // 年龄阈值（秒）
        private static float Age80PercentThreshold = AgeThreshold * 0.7f; // 年龄80%阈值
        public static float DebuffTimeThreshold = AgeThreshold - Age80PercentThreshold; // 衰老效果阈值

        // 初始化定时器
        public static void InitializeTimer()
        {
            nextExecutionTime = Time.time + TimerInterval;
            lastUpdateTime = Time.time;
            Debug.Log("Debuff定时器已启动，时间: " + System.DateTime.Now);
        }

        // 每帧调用更新逻辑
        public static void Update()
        {
            // 获取并更新缓存的复制人对象
            List<GameObject> currentMinions = KModMinionUtils.GetAllMinionGameObjects();

            int count = currentMinions.Count;

            // 如果缓存的复制人数量发生变化，则更新缓存
            if (count != cachedMinionCount)
            {
                UpdateMinionCache(currentMinions);
            }
           

            // 每秒执行一次更新逻辑
            if (Time.time - lastUpdateTime >= 1f)
            {
                ProcessMinionObjects();
                lastUpdateTime = Time.time;
            }

            // 定时执行任务
            if (Time.time >= nextExecutionTime)
            {
                ExecutePeriodicTask();
                nextExecutionTime = Time.time + TimerInterval;
            }
        }

        // 更新复制人对象缓存
        private static void UpdateMinionCache(List<GameObject> currentMinions)
        {
            // 创建新集合，并移除死亡或无效对象
            var validMinions = currentMinions
                .Where(obj => obj != null && !deadMinions.Contains(obj))
                .ToHashSet();

            // 更新缓存
            cachedMinionGameObjects.RemoveWhere(obj => !validMinions.Contains(obj));
            cachedMinionGameObjects.UnionWith(validMinions);
            cachedMinionCount = cachedMinionGameObjects.Count;
            Debug.Log($"复制人对象数量已更新: {cachedMinionCount}");
        }

        // 处理缓存中的复制人对象
        private static void ProcessMinionObjects()
        {
            foreach (GameObject minion in cachedMinionGameObjects.ToList())
            {
                if (minion == null || deadMinions.Contains(minion)) continue;

                var ageInstance = Db.Get().Amounts.Age.Lookup(minion);
                if (ageInstance == null) continue;

                float currentAgeInSeconds = ageInstance.value * 600;
                //死亡
                if (currentAgeInSeconds >= AgeThreshold)
                {
                    HandleDeath(minion);
                }
                // 衰老
                if (currentAgeInSeconds >= Age80PercentThreshold)
                {
                    DeBuff.ApplyDebuff(minion);
                }
            }
        }

        // 处理复制人对象的死亡
        private static void HandleDeath(GameObject minion)
        {
            var deathMonitor = minion.GetSMI<DeathMonitor.Instance>();
            if (deathMonitor != null)
            {
                minion.AddOrGet<KPrefabID>().AddTag(new Tag("KModNoMourning"));
                var customDeathConfig = AddNewDeathPatch.customDeath;
                deathMonitor.Kill(customDeathConfig);

                Debug.Log($"{minion.name} 已执行老死逻辑");

                // 更新缓存和状态
                cachedMinionGameObjects.Remove(minion);
                deadMinions.Add(minion);

                // 延迟执行操作，以确保在处理完死亡逻辑后更新复制人缓存和移除无效的游戏对象
                KModDelayedActionExecutor.Instance.ExecuteAfterDelay(0.1f, () =>
                {                
                    UpdateMinionCache(KModMinionUtils.GetAllMinionGameObjects());
                    RemoveInvalidGameObjects();
                });
                // 延迟 2 秒后执行以下操作，以确保在处理完死亡逻辑后生成新的对象
                KModDelayedActionExecutor.Instance.ExecuteAfterDelay(2f, () =>
                {
                    if (minion != null)
                    {
                        GenerateNewObject(minion, minion.transform.position);
                    }
                });
            }
            else
            {
                Debug.LogWarning("DeathMonitor.Instance 为空，无法执行死亡操作");
            }
        }

        // 生成新的复制人对象
        private static void GenerateNewObject(GameObject oldMinion, Vector3 position)
        {
            GameObject prefab = Assets.GetPrefab(new Tag("KmodMiniBrainCore"));
            if (prefab == null)
            {
                Debug.LogError("未找到 KmodMiniBrainCore 预制件.");
                return;
            }

            GameObject newMinion = GameUtil.KInstantiate(prefab, position, Grid.SceneLayer.Ore, null, 0);
            if (newMinion == null)
            {
                Debug.LogError("无法实例化新的复制人对象.");
                return;
            }

            newMinion.SetActive(true);

            KModDelayedActionExecutor.Instance.ExecuteAfterDelay(0.1f, () =>
            {
                TransferAttributesAndSkills(oldMinion, newMinion);
                SetNewMinionName(oldMinion, newMinion);
            });
        }



        // 转移旧对象的特质、技能和属性到新对象
        private static void TransferAttributesAndSkills(GameObject oldMinion, GameObject newMinion)
        {
            TransferTraits(oldMinion, newMinion);
            TransferSkills(oldMinion, newMinion);
            TransferAttributes(oldMinion, newMinion);
        }

        // 转移特质
        public static void TransferTraits(GameObject oldMinion, GameObject newMinion)
        {
            var oldTraits = oldMinion.GetComponent<Traits>();
            var newTraits = newMinion.GetComponent<Traits>();

            if (oldTraits != null && newTraits != null)
            {
                foreach (var trait in oldTraits.TraitList)
                {
                    if (trait.Id != "MinionBaseTrait" && !newTraits.HasTrait(trait))
                    {
                        newTraits.Add(trait);
                    }
                }

                foreach (var trait in newTraits.TraitList)
                {
                    Debug.LogWarning($"{newMinion.name} 继承了 {oldMinion.name} 的特质: {trait.Name} - {trait.Id}");
                }
            }
        }

        // 转移技能
        private static void TransferSkills(GameObject oldMinion, GameObject newMinion)
        {
            var oldResume = oldMinion.GetComponent<MinionResume>();
            var newResume = newMinion.GetComponent<MinionBrainResume>();

            if (oldResume != null && newResume != null)
            {
                foreach (var kvp in oldResume.MasteryBySkillID)
                {
                    if (kvp.Value && !newResume.MasteryBySkillID.ContainsKey(kvp.Key))
                    {
                        newResume.MasteryBySkillID.Add(kvp.Key, true);
                        Debug.LogWarning($"{newResume.name} 继承了 {oldResume.name} 的技能: {kvp.Key}");
                    }
                }
            }
        }

        // 转移属性
        private static void TransferAttributes(GameObject oldMinion, GameObject newMinion)
        {
            var oldAttributes = oldMinion.GetComponent<AttributeLevels>();
            var newAttributes = newMinion.GetComponent<AttributeLevels>();

            if (oldAttributes != null && newAttributes != null)
            {
                foreach (var oldAttribute in oldAttributes)
                {
                    string attributeId = oldAttribute.attribute.Attribute.Id;
                    int oldLevel = oldAttribute.GetLevel();
                    float oldExperience = oldAttribute.experience;

                    var newAttribute = newAttributes.GetAttributeLevel(attributeId);
                    if (newAttribute != null)
                    {
                        int newLevel = newAttribute.GetLevel() + oldLevel;
                        float newExperience = newAttribute.experience + oldExperience;

                        newAttributes.SetLevel(attributeId, newLevel);
                        newAttributes.SetExperience(attributeId, newExperience);
                    }
                    else
                    {
                        newAttributes.SetLevel(attributeId, oldLevel);
                        newAttributes.SetExperience(attributeId, oldExperience);
                    }

                    Debug.Log($"已将属性 {attributeId} 从 {oldMinion.name} 传递到 {newMinion.name}.");
                }
            }
            else
            {
                Debug.LogWarning("新对象上未找到 AttributeLevels 组件.");
            }
        }

        // 设置新复制人的名字
        private static void SetNewMinionName(GameObject oldMinion, GameObject newMinion)
        {
            var oldName = oldMinion.GetComponent<KSelectable>().GetName();
            var newNameable = newMinion.AddOrGet<UserNameable>();
            newNameable.SetName(oldName + " 的大脑");
        }

        // 执行定时任务
        private static void ExecutePeriodicTask()
        {
            DeBuff.ApplyRandomDebuff(cachedMinionGameObjects);
        }

        // 移除无效的 GameObject
        private static void RemoveInvalidGameObjects()
        {
            cachedMinionGameObjects.RemoveWhere(obj => obj == null);
            if (cachedMinionGameObjects.Count < cachedMinionCount)
            {
                Debug.LogError("发现无效的 GameObject 在缓存中");
                cachedMinionCount = cachedMinionGameObjects.Count;
            }
        }
    }
}
