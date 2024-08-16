using Klei.AI;
using System.Collections.Generic;
using UnityEngine;

namespace DebuffRoulette
{
    public class KmodMoniBrainCoreConfig : IEntityConfig
    {
        public string[] GetDlcIds()
        {
            return DlcManager.AVAILABLE_ALL_VERSIONS;
        }

        public GameObject CreatePrefab()
        {
            GameObject gameObject = EntityTemplates.CreateLooseEntity(
                "KmodMiniBrainCore",
                KmodMoniBrainCoreConfig.NAME,
                KmodMoniBrainCoreConfig.DESC,
                5f,
                true,
                Assets.GetAnim("KmodMiniBrainCore_kanim"),
                "object",
                Grid.SceneLayer.Front,
                EntityTemplates.CollisionShape.RECTANGLE,
                0.8f,
                0.6f,
                true,
                0,
                SimHashes.Creature,
                new List<Tag> { GameTags.MiscPickupable }
            );

            KPrefabID prefabID = gameObject.AddOrGet<KPrefabID>();
            prefabID.AddTag(KmodMoniBrainCoreConfig.tag);
            gameObject.AddTag(GameTags.Dead);

            gameObject.AddOrGet<Modifiers>();
            gameObject.AddOrGet<Traits>();

            return gameObject;
        }

        public void OnPrefabInit(GameObject inst) { }

        public void OnSpawn(GameObject inst)
        {
            // 获取 Traits 组件
            Traits traits = inst.GetComponent<Traits>();
            if (traits != null)
            {
                // 拼接 Traits 名称到描述中
                foreach (var trait in traits.TraitList)
                {
                    KmodMoniBrainCoreConfig.DESC += $"\n- {trait.Name}";
                }
            }
        }

        public static void SetDesc(string desc)
        {
            DESC = desc;
        }

        public static string DESC = "生前的全部遗产";
        public static string NAME = "复制人大脑";

        public const string ID = "KmodMiniBrainCore";
        public static readonly Tag tag = TagManager.Create("KmodMiniBrainCore");
        public const float MASS = 5f;
    }
}
