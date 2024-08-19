using Database;
using Epic.OnlineServices.Logging;
using Klei.AI;
using KModTool;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static STRINGS.UI.TOOLS.FILTERLAYERS;

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
        public static float KModminionAge = 1f;
        private static float AgeThreshold = KModminionAge * 600f; // 复制人年龄阈值
        public static float Age80PercentThreshold = AgeThreshold * 0.7f; // 年龄达到70%时的阈值
        public static float shuailaoDebufftime = AgeThreshold - Age80PercentThreshold; // 年龄达到70%时的阈值

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
            var newMinionGameObjects = minionGameObjects.Where(obj => obj != null && !deadMinions.Contains(obj)).ToHashSet();

            cachedMinionGameObjects.RemoveWhere(gameObject => !newMinionGameObjects.Contains(gameObject));
            cachedMinionGameObjects.UnionWith(newMinionGameObjects);

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
            // 处理死亡逻辑
            var smi1 = gameObject.GetSMI<DeathMonitor.Instance>();
            if (smi1 != null)
            {
                gameObject.AddOrGet<KPrefabID>().AddTag(new Tag("KModNoMourning"));
                var mydb = AddNewDeathPatch.customDeath;
                smi1.Kill(mydb); // 执行老死操作

                Debug.Log($"{gameObject.name} 已被执行死亡");

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

        // 生成新对象
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
            KModDelayedActionExecutor.Instance.ExecuteAfterDelay(0.1f, () =>
            {
                MutualTransfer(gameObject, newGameObject);
            });


        }

        private static void MutualTransfer(GameObject oldgameObject, GameObject newgameObject)
        {
            Traits oldtraitsComponent = oldgameObject.GetComponent<Traits>();
            Traits newTraitsComponent = newgameObject.GetComponent<Traits>();

            MinionResume oldResume = oldgameObject.GetComponent<MinionResume>();
            MinionBrainResume newResume = newgameObject.GetComponent<MinionBrainResume>();


            if (oldtraitsComponent != null && newTraitsComponent != null)
            {

                CopyTraits(oldtraitsComponent, newTraitsComponent);

                foreach (var trait in newTraitsComponent.TraitList)
                {
                    Debug.LogWarning($"{newgameObject.name}继承了{oldgameObject.name} 的: {trait.Name} - {trait.Id}");
                }
            }

            if (oldResume != null && newResume != null)
            {
                // 复制旧对象的技能到新对象
                CopySkills(oldResume, newResume);

            }
            KSelectable oldcomponent = oldgameObject.GetComponent<KSelectable>();
            //KSelectable newcomponent = newgameObject.GetComponent<KSelectable>();
            //newcomponent.SetName(oldcomponent.GetName());
            newgameObject.AddOrGet<UserNameable>().SetName(oldcomponent.GetName() + "的大脑");


            //Debug.Log($"新对象名字 · {newcomponent.GetName()}");







            // 获取旧对象上的 AttributeLevels 组件
            AttributeLevels oldAttributeLevels = oldgameObject.GetComponent<AttributeLevels>();
            // 获取新对象上的 AttributeLevels 组件
            AttributeLevels newAttributeLevels = newgameObject.GetComponent<AttributeLevels>();
            if (oldAttributeLevels != null)
            {
               

                if (newAttributeLevels != null)
                {
                    // 将旧对象的属性数据添加到新对象上
                    foreach (AttributeLevel oldAttributeLevel in oldAttributeLevels)
                    {
                        string attributeId = oldAttributeLevel.attribute.Attribute.Id;
                        string attributename = oldAttributeLevel.attribute.Name;
                        int oldLevel = oldAttributeLevel.GetLevel();
                        float oldExperience = oldAttributeLevel.experience;
                        Debug.Log($"获得属性 · {attributename} - {attributeId} : {oldLevel}");
                        // 获取新对象上对应的属性等级和经验值
                        AttributeLevel newAttributeLevel = newAttributeLevels.GetAttributeLevel(attributeId);

                        if (newAttributeLevel != null)
                        {
                            // 增加现有属性的等级和经验值
                            int combinedLevel = newAttributeLevel.GetLevel() + oldLevel;
                            float combinedExperience = newAttributeLevel.experience + oldExperience;

                            newAttributeLevels.SetLevel(attributeId, combinedLevel);
                            newAttributeLevels.SetExperience(attributeId, combinedExperience);
                        }
                        else
                        {

                            // 如果新对象上不存在该属性，则直接添加
                            newAttributeLevels.SetLevel(attributeId, oldLevel);
                            newAttributeLevels.SetExperience(attributeId, oldExperience);
                        }

                        Debug.Log($"已将属性 {oldgameObject.name} - {attributeId} 添加到新对象 {newgameObject.name}.");
                    }
                }
                else
                {
                    Debug.LogWarning("新对象上未找到 AttributeLevels 组件.");
                }
            }








        }

        // 进行特质转移
        private static void CopyTraits(Traits sourceTraits, Traits targetTraits)
        {
            foreach (var trait in sourceTraits.TraitList)
            {
                if (trait.Id == "MinionBaseTrait") continue;

                if (!targetTraits.HasTrait(trait))
                {
                    targetTraits.Add(trait);
                }
            }
        }


        // 复制技能方法
        private static void CopySkills(MinionResume oldResume, MinionBrainResume newResume)
        {
            if (oldResume == null || newResume == null) return;

            // 获取旧对象掌握的技能
            Dictionary<string, bool> oldMasteryBySkillID = oldResume.MasteryBySkillID;

            // 遍历旧对象的技能字典
            foreach (KeyValuePair<string, bool> keyValuePair in oldMasteryBySkillID)
            {
                if (keyValuePair.Value) // 如果技能已掌握
                {
                    string skillId = keyValuePair.Key;

                    // 确保新对象的字典中也添加这个技能
                    if (!newResume.MasteryBySkillID.ContainsKey(skillId))
                    {
                        newResume.MasteryBySkillID.Add(skillId, true);
                        Debug.LogWarning($"{newResume.name}继承了{oldResume.name} 的技能为:  {skillId}");
                    }
                }
            }

        }



        private static void AddSkillsFromNewToOld(MinionBrainResume newResume, MinionResume oldResume)
        {
            if (newResume == null || oldResume == null) return;

            // 获取新对象掌握的技能
            Dictionary<string, bool> newMasteryBySkillID = newResume.MasteryBySkillID;

            // 遍历新对象的技能字典
            foreach (KeyValuePair<string, bool> keyValuePair in newMasteryBySkillID)
            {
                if (keyValuePair.Value) // 如果技能已掌握 
                {
                    string skillId = keyValuePair.Key;

                    // 确保旧对象的字典中也添加这个技能
                    if (!oldResume.MasteryBySkillID.ContainsKey(skillId))
                    {
                        oldResume.MasteryBySkillID.Add(skillId, true);
                    }
                }
            }











        }

        // 假设目标对象是 targetGameObject
        public static void AddSkillAttributesToTarget(GameObject oldgameObject, GameObject newgameObject)
        {
            // 获取源对象的技能属性
            List<AttributeInstance> skillAttributes = new List<AttributeInstance>(oldgameObject.GetAttributes().AttributeTable)
                .FindAll(a => a.Attribute.ShowInUI == Klei.AI.Attribute.Display.Skill);

            List<AttributeInstance> targetAttributes = new List<AttributeInstance>(newgameObject.GetAttributes().AttributeTable)
                .FindAll(a => a.Attribute.ShowInUI == Klei.AI.Attribute.Display.Skill);

            foreach (var skillAttribute in skillAttributes)
            {
                // 查找目标对象中是否已经存在相同的属性
                AttributeInstance targetAttributeInstance = null;
                foreach (var targetAttribute in targetAttributes)
                {
                    if (targetAttribute.Attribute.Id == skillAttribute.Attribute.Id)
                    {
                        targetAttributeInstance = targetAttribute;
                        break;
                    }
                }

                if (targetAttributeInstance == null)
                {
                    // 如果目标对象没有对应的属性实例，则创建一个新的属性实例并添加到目标对象
                    var newAttributeInstance = new AttributeInstance(newgameObject, skillAttribute.Attribute);

                    // 将源对象的修改器添加到新实例中
                    for (int i = 0; i < skillAttribute.Modifiers.Count; i++)
                    {
                        var modifier = skillAttribute.Modifiers[i];
                        newAttributeInstance.Add(modifier);
                    }

                    targetAttributes.Add(newAttributeInstance);
                }
                else
                {
                    // 如果目标对象已经有对应的属性实例，则累加其修改器
                    for (int i = 0; i < skillAttribute.Modifiers.Count; i++)
                    {
                        var modifier = skillAttribute.Modifiers[i];
                        bool modifierExists = false;

                        // 查找目标属性实例中是否已经存在相同的修改器
                        for (int j = 0; j < targetAttributeInstance.Modifiers.Count; j++)
                        {
                            var targetModifier = targetAttributeInstance.Modifiers[j];
                            if (targetModifier.AttributeId == modifier.AttributeId && targetModifier.IsMultiplier == modifier.IsMultiplier)
                            {
                                targetModifier.SetValue(targetModifier.Value + modifier.Value);
                                modifierExists = true;
                                break;
                            }
                        }

                        if (!modifierExists)
                        {
                            // 如果目标实例中没有相同的修改器，直接添加
                            targetAttributeInstance.Add(modifier);
                        }
                    }
                }
            }
        }







        //// 复制属性
        //private static void CopyAttributes(Klei.AI.Attributes oldAttributes, Klei.AI.Attributes newAttributes)
        //{
        //    // 清除新对象的所有属性
        //    newAttributes.AttributeTable.Clear();

        //    foreach (var attributeInstance in oldAttributes)
        //    {
        //        // 创建新的 Attribute 实例
        //        Klei.AI.Attribute attribute = Db.Get().Attributes.Get(attributeInstance.Id);
        //        if (attribute != null)
        //        {
        //            AttributeInstance newInstance = newAttributes.Add(attribute);
        //            newInstance.ClearModifiers();

        //            // 复制所有 modifiers
        //            for (int i = 0; i < attributeInstance.Modifiers.Count; i++)
        //            {
        //                AttributeModifier modifier = attributeInstance.Modifiers[i];
        //                newInstance.Add(modifier);
        //            }
        //        }
        //    }
        //}





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
