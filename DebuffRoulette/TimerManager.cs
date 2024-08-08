﻿using KModTool;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebuffRoulette
{
    public static class RandomDebuffTimerManager
    {
        private static float interval_600 = 600f; // 定时器间隔时间（秒）
        private static float nextTime_600 = 0f;
        private static List<GameObject> cachedMinionGameObjects = new List<GameObject>(); // 缓存的复制人对象
        private static int cachedMinionCount = 0; // 缓存的数量
        private static float lastUpdateTime = 0f; // 上次更新的时间
        private static float updateInterval = 1f; // 定期更新的时间间隔（秒）

        public static void StartTimer()
        {
            nextTime_600 = Time.time + interval_600;
            lastUpdateTime = Time.time;
            Debug.Log("DeBuff定时器启动时间: " + System.DateTime.Now);
        }

        public static void Update()
        {
            // 获取当前的复制人对象列表
            List<GameObject> minionGameObjects = KModMinionUtils.GetAllMinionGameObjects();
            int count = minionGameObjects.Count;

            // 如果复制人对象数量有变化，则更新缓存
            if (count != cachedMinionCount)
            {
                cachedMinionGameObjects = new List<GameObject>(minionGameObjects);
                cachedMinionCount = count;
                Debug.Log("复制人对象数量变化，缓存已更新");
            }

            if (Time.time - lastUpdateTime >= updateInterval)
            {
                ProcessMinionGameObjects(cachedMinionGameObjects);
                lastUpdateTime = Time.time;
            }


            if (Time.time >= nextTime_600)
            {
                ExecuteTask();
                nextTime_600 = Time.time + interval_600;
            }
        }

        private static void ProcessMinionGameObjects(List<GameObject> minionGameObjects)
        {
            float currentCycle = (float)GameClock.Instance.GetCycle();
            float ageThreshold = 16 * 600f;

            foreach (GameObject gameObject in minionGameObjects)
            {
                MinionIdentity minionIdentity = gameObject.GetComponent<MinionIdentity>();
                if (minionIdentity != null)
                {
                    float minionAge = (currentCycle - minionIdentity.arrivalTime) * 600f;
                    if (minionAge >= ageThreshold)
                    {
                        Debug.Log("可以死了");
                        gameObject.GetSMI<DeathMonitor.Instance>().Kill(Db.Get().Deaths.Slain);
                    }
                }
            }
        }

        private static void ExecuteTask()
        {
            Debug.Log("600档位定时器执行时间: " + System.DateTime.Now);
            ApplyRandomDebuff();
        }

        private static void ApplyRandomDebuff()
        {
            Debug.Log("debuff添加ok");
        }
    }
}
