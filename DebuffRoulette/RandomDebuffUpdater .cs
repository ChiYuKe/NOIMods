﻿
using UnityEngine;

namespace DebuffRoulette
{
    public class RandomDebuffUpdater : KMonoBehaviour
    {
        private void Update()
        {
            RandomDebuffTimerManager.Update();
        }
    }

    public static class ModEntry
    {
        public static void Initialize()
        {
            // 在游戏对象上添加RandomDebuffUpdater组件
            GameObject timerUpdaterObject = new GameObject("RandomDebuffUpdater");
            UnityEngine.Object.DontDestroyOnLoad(timerUpdaterObject);
            timerUpdaterObject.AddComponent<RandomDebuffUpdater>();
            RandomDebuffTimerManager.StartTimer();

          
        }
    }
}
