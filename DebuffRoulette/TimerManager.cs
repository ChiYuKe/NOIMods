using Database;
using Klei.AI;
using KModTool;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebuffRoulette
{
    public static class RandomDebuffTimerManager
    {
        private static readonly float Interval600 = 600f; // 定时器间隔时间（秒）
        private static float nextTime600 = 0f; // 下次执行600秒定时器任务的时间
        private static HashSet<GameObject> cachedMinionGameObjects = new HashSet<GameObject>(); // 缓存的复制人对象集合
        private static HashSet<GameObject> deadMinions = new HashSet<GameObject>(); // 已死亡角色集合
        private static int cachedMinionCount = 0; // 缓存的复制人数量
        private static float lastUpdateTime = 0f; // 上次更新的时间
        private static readonly float UpdateInterval = 1f; // 定期更新的时间间隔（秒）
        public static float KModminionAge = 2f;
        private static float AgeThreshold = KModminionAge * 600f; // 复制人年龄阈值
        public static float Age80PercentThreshold = AgeThreshold * 0.7f; // 年龄达到70%时的阈值

        // 启动定时器，设置首次执行时间和日志记录启动时间
        public static void StartTimer()
        {
            nextTime600 = Time.time + Interval600; // 设置下次600秒任务的时间
            lastUpdateTime = Time.time; // 记录启动时间
            Debug.Log("DeBuff定时器启动时间: " + System.DateTime.Now);
        }

        // 更新方法，每秒钟调用一次
        public static void Update()
        {
            List<GameObject> minionGameObjects = KModMinionUtils.GetAllMinionGameObjects(); // 获取所有复制人对象
            int count = minionGameObjects.Count;

            // 如果缓存的复制人数量发生变化，则更新缓存
            if (count != cachedMinionCount)
            {
                UpdateCache(minionGameObjects);
            }

            // 根据定时器间隔调用处理方法
            if (Time.time - lastUpdateTime >= UpdateInterval)
            {
                ProcessMinionGameObjects();
                lastUpdateTime = Time.time;
            }

            // 如果达到600秒，执行定时任务
            if (Time.time >= nextTime600)
            {
                ExecuteTask();
                nextTime600 = Time.time + Interval600;
            }
        }

        // 更新缓存，移除死亡或无效对象，保留存活的对象
        private static void UpdateCache(List<GameObject> minionGameObjects)
        {
            cachedMinionGameObjects.RemoveWhere(gameObject => gameObject == null || deadMinions.Contains(gameObject)); // 移除无效或已死亡对象
            cachedMinionGameObjects.UnionWith(minionGameObjects.Where(obj => obj != null && !deadMinions.Contains(obj))); // 添加存活的对象
            cachedMinionCount = cachedMinionGameObjects.Count; // 更新缓存数量
            Debug.Log($"复制人对象数量变化，人数缓存已更新 {cachedMinionCount}");
        }

        // 处理复制人对象的死亡和效果应用逻辑
        private static void ProcessMinionGameObjects()
        {
            foreach (GameObject gameObject in cachedMinionGameObjects.ToList()) // 使用 ToList() 以避免在遍历过程中修改集合
            {
                if (gameObject == null || deadMinions.Contains(gameObject)) continue; // 跳过无效或已死亡对象

                var ageInstance = Db.Get().Amounts.Age.Lookup(gameObject); // 获取年龄实例
                if (ageInstance == null) continue; // 如果没有年龄实例，跳过

                float currentAge = ageInstance.value * 600; // 当前年龄（秒）

                if (currentAge >= AgeThreshold) // 检查是否已达到死亡阈值
                {
                    HandleDeath(gameObject); // 处理死亡逻辑
                }
                else if (currentAge >= Age80PercentThreshold) // 检查是否已达到 80% 年龄阈值
                {
                    DeBuff.ApplyDebuff(gameObject); // 应用衰老效果
                }
            }
        }





        // 处理死亡逻辑，包括移除效果和生成新对象
        private static void HandleDeath(GameObject gameObject)
        {
            Effects effectsComponent = gameObject.GetComponent<Effects>();
            if (effectsComponent != null && effectsComponent.HasEffect("shuailao")) // 如果有衰老效果
            {
                effectsComponent.Remove("shuailao"); // 移除衰老效果
            }

            var smi1 = gameObject.GetSMI<DeathMonitor.Instance>();
            if (smi1 != null)
            {
                var mydb = AddNewDeathPatch.customDeath;
                smi1.Kill(mydb); // 执行死亡操作
                Debug.Log($"{gameObject.name} 已被执行死亡");

                // 从缓存中移除并标记为已死亡
                cachedMinionGameObjects.Remove(gameObject);
                deadMinions.Add(gameObject);

                // 延迟更新缓存和检查无效对象
                KModDelayedActionExecutor.Instance.ExecuteAfterDelay(0.1f, () =>
                {
                    UpdateCache(KModMinionUtils.GetAllMinionGameObjects());
                    CheckForInvalidGameObjects();
                });

                // 延迟生成新对象
                KModDelayedActionExecutor.Instance.ExecuteAfterDelay(2f, () =>
                {
                    if (gameObject != null)
                    {
                        GenerateNewObject(gameObject, gameObject.transform.position);
                    }
                });
            }
            else
            {
                Debug.LogWarning("DeathMonitor.Instance 为空，无法执行 Kill 操作");
            }
        }

        // 生成新对象并复制 Traits
        private static void GenerateNewObject(GameObject gameObject, Vector3 position)
        {
            GameObject prefab = Assets.GetPrefab(new Tag("KmodMiniBrainCore"));
            if (prefab == null)
            {
                Debug.LogError("未找到 KmodMiniBrainCore 的预制件.");
                return;
            }

            GameObject newGameObject = GameUtil.KInstantiate(prefab, position, Grid.SceneLayer.Ore, null, 0);

            if (newGameObject == null)
            {
                Debug.LogError("无法实例化对象.");
                return;
            }

            newGameObject.SetActive(true);

            var traitsComponent = gameObject.GetComponent<Traits>();
            var newTraitsComponent = newGameObject.GetComponent<Traits>();

            if (traitsComponent != null && newTraitsComponent != null)
            {
                foreach (var trait in traitsComponent.TraitList)
                {
                    if (!newTraitsComponent.HasTrait(trait))
                    {
                        newTraitsComponent.Add(trait);
                        Debug.LogWarning($"成功添加 Traits 信息: {trait.Name}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("无法检索 Traits 组件.");
            }
        }

        // 执行定时任务，例如应用随机衰老效果
        private static void ExecuteTask()
        {
            DeBuff.ApplyRandomDebuff(cachedMinionGameObjects);
        }

        // 检查并移除缓存中的无效 GameObject
        private static void CheckForInvalidGameObjects()
        {
            cachedMinionGameObjects.RemoveWhere(obj => obj == null); // 移除无效对象
            if (cachedMinionGameObjects.Count < cachedMinionCount)
            {
                Debug.LogError("发现无效的 GameObject 在缓存中");
                cachedMinionCount = cachedMinionGameObjects.Count; // 更新缓存计数
            }
        }
    }
}
