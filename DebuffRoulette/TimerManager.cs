using System;
using UnityEngine;

namespace DebuffRoulette
{
    public static class RandomDebuffTimerManager
    {
        private static float interval = 10f; // 定时器间隔时间（秒）
        private static float nextTime = 0f;

        public static void StartTimer()
        {
            nextTime = Time.time + interval;
            Debug.Log("定时器启动时间: " + System.DateTime.Now);
        }

        public static void Update()
        {
            Debug.Log("当前时间: " + Time.time + "，下次执行时间: " + nextTime);
            if (Time.time >= nextTime)
            {
                ExecuteTask();
                nextTime = Time.time + interval;
            }
        }

        private static void ExecuteTask()
        {
            Debug.Log("定时器执行时间: " + System.DateTime.Now);
            
            ApplyRandomDebuff();
        }

        private static void ApplyRandomDebuff()
        {
           
            Debug.Log("debuff添加ok");
        }
    }
}
