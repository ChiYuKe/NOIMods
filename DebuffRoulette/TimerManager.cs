using Database;
using FMOD;
using HarmonyLib;
using Klei.AI;
using KModTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using UnityEngine;

namespace DebuffRoulette
{
    public static class RandomDebuffTimerManager
    {
        private static readonly float Interval600 = 600f; // 定时器间隔时间（秒）
        private static float nextTime600 = 0f; // 下次执行600秒定时器任务的时间
        private static HashSet<GameObject> cachedMinionGameObjects = new HashSet<GameObject>(); // 缓存的复制人对象集合
        private static int cachedMinionCount = 0; // 缓存的复制人数量
        private static float lastUpdateTime = 0f; // 上次更新的时间
        private static readonly float UpdateInterval = 1f; // 定期更新的时间间隔（秒）
        public static float KModminionAge = 3f;
        private static float AgeThreshold = KModminionAge * 600f; // 复制人年龄阈值
        public static float Age80PercentThreshold = AgeThreshold * 0.7f; // 年龄达到70%时的阈值

        // 启动定时器
        public static void StartTimer()
        {
           
            nextTime600 = Time.time + Interval600; // 设置下次600秒任务的时间
            lastUpdateTime = Time.time; // 记录启动时间
            Debug.Log("DeBuff定时器启动时间: " + System.DateTime.Now);
        }

        // 更新方法，每帧调用
        public static void Update()
        {
            List<GameObject> minionGameObjects = KModMinionUtils.GetAllMinionGameObjects();
            int count = minionGameObjects.Count;

            // 如果复制人数量发生变化，更新缓存
            if (count != cachedMinionCount)
            {
                Debug.Log($"人数更新前 {cachedMinionCount}");
                UpdateCache(minionGameObjects);

            }

            // 定期处理复制人对象和应用Debuff
            if (Time.time - lastUpdateTime >= UpdateInterval)
            {
                ProcessMinionGameObjects();
                lastUpdateTime = Time.time;
            }

            // 每周期秒执行一次任务
            if (Time.time >= nextTime600)
            {
                ExecuteTask();
                nextTime600 = Time.time + Interval600;


               

            }
        }

        // 更新缓存中的复制人对象
        private static void UpdateCache(List<GameObject> minionGameObjects)
        {
            cachedMinionGameObjects.Clear(); // 清空旧的缓存
            cachedMinionGameObjects.UnionWith(minionGameObjects.FindAll(obj => obj != null)); // 更新缓存
            cachedMinionCount = cachedMinionGameObjects.Count;
            Debug.Log($"复制人对象数量变化，人数缓存已更新 {cachedMinionCount}");
        }

        // 处理复制人对象，应用Debuff和处理死亡
        private static void ProcessMinionGameObjects()
        {
            foreach (GameObject gameObject in cachedMinionGameObjects)
            {
                if (gameObject == null) continue;

                MinionIdentity minionIdentity = gameObject.GetComponent<MinionIdentity>();
                if (minionIdentity == null) continue;

                Effects effectsComponent = gameObject.GetComponent<Effects>();
                


                var ageInstance = Db.Get().Amounts.Age.Lookup(gameObject);
                float currentAge = ageInstance.value * 600;
                // Debug.Log($"当前时间{currentAge} /  {AgeThreshold}");

                // 如果年龄达到80%阈值，则应用衰老效果
                if (currentAge >= Age80PercentThreshold)
                {
                    if (effectsComponent != null && !effectsComponent.HasEffect("shuailao"))
                    {
                        effectsComponent.Add("shuailao", true);
                        NotifyDeathApplied(gameObject);
                    }
                }

                if (currentAge >= AgeThreshold)
                {
                    HandleDeath(gameObject);
                }
            }
        }

        // 创建并添加通知
        private static void NotifyDeathApplied(GameObject gameObject)
        {
            Notifier notifier = gameObject.AddOrGet<Notifier>();
            Notification notification = new Notification(
                DebuffRoulette.STRINGS.MISC.NOTIFICATIONS.DEATHROULETTE.NAME,
                NotificationType.MessageImportant,
                (notificationList, data) => notificationList.ReduceMessages(false),
                "/t• " + gameObject.GetProperName(), true, 0f, null, null, null, true, false, false
            );
            notifier.Add(notification, "");
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

        // 处理复制人死亡
        private static void HandleDeath(GameObject gameObject)
        {
            // var smi = InitializeDeathMonitor(gameObject);


            var smi1 = gameObject.GetSMI<DeathMonitor.Instance>();
            if (smi1 != null)
            {
                var db = Db.Get().Deaths.Generic;
                var mydb = AddNewDeathPatch.customDeath;

                smi1.Kill(mydb);        
                Debug.Log($"{gameObject.name} 已被执行死亡");

                // 延迟更新缓存
                KModDelayedActionExecutor.Instance.ExecuteAfterDelay(0.1f, () =>
                {
                    UpdateCache(KModMinionUtils.GetAllMinionGameObjects());
                    CheckForInvalidGameObjects();
                });


                // 在2秒后生成物品
                KModDelayedActionExecutor.Instance.ExecuteAfterDelay(2f, () =>
                {
                    if (gameObject != null)
                    {
                        GenerateNewObject(gameObject.transform.position);
                    }
                });

                // 在3秒后移除gameObject
                KModDelayedActionExecutor.Instance.ExecuteAfterDelay(10f, () =>
                {
                    if (gameObject != null)
                    {
                        UnityEngine.Object.Destroy(gameObject);
                    }
                });
            }
            else
            {
                Debug.LogWarning("DeathMonitor.Instance 为空，无法执行 Kill 操作");
            }
        }

        // 检查缓存中的无效GameObject
        private static void CheckForInvalidGameObjects()
        {
            cachedMinionGameObjects.RemoveWhere(obj => obj == null);
            if (cachedMinionGameObjects.Count < cachedMinionCount)
            {
                Debug.LogError("发现无效的 GameObject 在缓存中");
                cachedMinionCount = cachedMinionGameObjects.Count; // 更新缓存计数
            }
        }

        // 生成新对象
        private static void GenerateNewObject(Vector3 position)
        {
            Debug.Log("延迟执行了");

            Grid.SceneLayer sceneLayer = Grid.SceneLayer.Ore;
            GameObject prefab = Assets.GetPrefab(new Tag("iron"));
            GameObject newGameObject = GameUtil.KInstantiate(prefab, position, sceneLayer, null, 0);

            newGameObject.SetActive(true); // 激活新对象
        }

        // 执行600秒定时任务
        private static void ExecuteTask()
        {
            Debug.Log("600档位定时器执行时间: " + System.DateTime.Now);
            ApplyRandomDebuff(); // 应用随机Debuff
        }



        // 应用随机Debuff
        private static void ApplyRandomDebuff()
        {
            List<string> debuffTypes = new List<string> { "debuff1", "debuff2", "debuff3" , "debuff4" };
            int minionCount = cachedMinionGameObjects.Count;
            int numToSelect = 3;

            if (minionCount >= numToSelect)
            {
                // 打乱小人列表，随机选择前 numToSelect 个小人
                List<GameObject> selectedMinions = cachedMinionGameObjects.OrderBy(x => UnityEngine.Random.value).Take(numToSelect).ToList();

                foreach (GameObject gameObject in selectedMinions)
                {
                    if (gameObject == null) continue;

                    Effects effectsComponent = gameObject.GetComponent<Effects>();
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

        public static class DeathsManager
        {
            public static Death CombatDeath { get; private set; }

            static DeathsManager()
            {
                CombatDeath = new Death("KModCombat", null, STRINGS.DUPLICANTS.DEATHS.NAME, STRINGS.DUPLICANTS.DEATHS.DESCRIPTION, "dead_on_back", "dead_on_back");
            }
        }

        [HarmonyPatch(typeof(Db))]
        public static class Db_Initialize_Patch
        {
            // 定义一个后缀方法，在 Db.Initialize 方法执行后调用
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            public static void Postfix(Db __instance)
            {
                // 在这里添加你的自定义逻辑
                // 例如将 CombatDeath 添加到游戏的死亡列表中
                if (__instance.Deaths != null)
                {
                    __instance.Deaths.Add(DeathsManager.CombatDeath);
                }
                else
                {
                    Console.WriteLine("Error: Deaths is null in Db instance.");
                }
            }
        }


    }
}
