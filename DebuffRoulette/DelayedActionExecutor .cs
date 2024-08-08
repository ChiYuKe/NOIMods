using UnityEngine;
using System.Collections;

namespace DebuffRoulette
{
    public class DelayedActionExecutor : KMonoBehaviour
    {
        private static DelayedActionExecutor instance;

        public static DelayedActionExecutor Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("DelayedActionExecutor");
                    instance = go.AddComponent<DelayedActionExecutor>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public void ExecuteAfterDelay(float delay, System.Action action)
        {
            StartCoroutine(DelayedExecution(delay, action));
        }

        private IEnumerator DelayedExecution(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
