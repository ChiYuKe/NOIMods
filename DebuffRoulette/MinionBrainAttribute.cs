using Klei.AI;
using KSerialization;
using System.Collections.Generic;
using UnityEngine;

namespace DebuffRoulette
{
    public class SkillAttributesComponent : MonoBehaviour, ISaveLoadable
    {
        // 用于存储技能信息的字典
        [Serialize]
        public Dictionary<string, float> SkillAttributesById = new Dictionary<string, float>();

        // 添加技能信息
        public void AddSkill(string skillId, float skillValue)
        {
            SkillAttributesById[skillId] = skillValue;
        }

        // 获取技能值
        public float GetSkillValue(string skillId)
        {
            return SkillAttributesById.TryGetValue(skillId, out float skillValue) ? skillValue : 0f;
        }

        // 清除所有技能
        public void ClearSkills()
        {
            SkillAttributesById.Clear();
        }

        // 打印技能属性
        public void PrintSkillAttributes()
        {
            foreach (var skill in SkillAttributesById)
            {
                Debug.Log($"技能 ID: {skill.Key} - 值: {skill.Value}");
            }
        }
    }
}
