using KSerialization;
using System.Collections.Generic;

namespace DebuffRoulette
{
    public class MinionBrainResume : KMonoBehaviour, ISaveLoadable
    {
        // 存储技能 ID 和其掌握状态的字典
        [Serialize]
        public Dictionary<string, bool> MasteryBySkillID = new Dictionary<string, bool>();

        // 添加技能方法
        public void AddSkill(string skillId, bool isMastered)
        {
            MasteryBySkillID[skillId] = isMastered;
        }

        // 获取技能掌握状态
        public bool IsSkillMastered(string skillId)
        {
            return MasteryBySkillID.TryGetValue(skillId, out bool isMastered) && isMastered;
        }

        // 清除所有技能
        public void ClearSkills()
        {
            MasteryBySkillID.Clear();
        }
    }
}
