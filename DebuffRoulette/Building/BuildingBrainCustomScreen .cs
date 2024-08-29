using UnityEngine;
using UnityEngine.UI;

namespace DebuffRoulette
{
    public class BuildingBrainCustomScreen : KScreen
    {
        private GameObject contentObject;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Debug.Log("BuildingBrainCustomScreen: OnPrefabInit");
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Debug.Log("BuildingBrainCustomScreen: Activated");
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            Debug.Log("BuildingBrainCustomScreen: Deactivated");
        }

     
    }
}
