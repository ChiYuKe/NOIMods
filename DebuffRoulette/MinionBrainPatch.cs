using Database;
using HarmonyLib;
using Klei.AI;
using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ResearchTypes;
using static UnityEngine.GraphicsBuffer;

namespace DebuffRoulette
{
    [HarmonyPatch(typeof(SimpleInfoScreen))]
    [HarmonyPatch("OnPrefabInit")]
    public class SimpleInfoScreenOnPrefabInitPatch
    {
        // 新面板字段
        private static CollapsibleDetailContentPanel newTraitsPanel;
        private static CollapsibleDetailContentPanel newResumePanel;
        private static CollapsibleDetailContentPanel newAttributesPanel;

        [HarmonyPostfix]
        public static void Postfix(SimpleInfoScreen __instance)
        {
            // 使用反射访问 protected 方法
            var createSectionMethod = AccessTools.Method(typeof(SimpleInfoScreen), "CreateCollapsableSection");
            if (createSectionMethod != null)
            {
                // 调用 CreateCollapsableSection 创建新的面板
                try
                {
                    newTraitsPanel = (CollapsibleDetailContentPanel)createSectionMethod.Invoke(__instance, new object[] { "特质知识遗产" });
                    newResumePanel = (CollapsibleDetailContentPanel)createSectionMethod.Invoke(__instance, new object[] { "技能知识遗产" });
                    newAttributesPanel = (CollapsibleDetailContentPanel)createSectionMethod.Invoke(__instance, new object[] { "属性知识遗产" });
                    Debug.Log("新面板创建成功");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"创建新面板时发生异常: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError("未找到 CreateCollapsableSection 方法");
            }
        }

        public static CollapsibleDetailContentPanel GetTraitsNewPanel()
        {
            return newTraitsPanel;
        }
        public static CollapsibleDetailContentPanel GetResumeNewPanel()
        {
            return newResumePanel;
        }
        public static CollapsibleDetailContentPanel GetAttributesNewPanel()
        {
            return newAttributesPanel;
        }
    }



    [HarmonyPatch(typeof(SimpleInfoScreen))]
    [HarmonyPatch("Refresh")]
    public class SimpleInfoScreenRefreshPatch
    {
        [HarmonyPostfix]
        public static void Postfix(SimpleInfoScreen __instance, bool force)
        {
            // 获取 protected 字段 selectedTarget
            FieldInfo selectedTargetField = AccessTools.Field(typeof(SimpleInfoScreen), "selectedTarget");

            if (selectedTargetField != null)
            {
                // 读取 selectedTarget 字段的值
                GameObject selectedTarget = (GameObject)selectedTargetField.GetValue(__instance);

                if (selectedTarget == null)
                {
                    return;
                }

                // 获取新面板
                var traitsNewPanel = SimpleInfoScreenOnPrefabInitPatch.GetTraitsNewPanel();
                var resumeNewPanel = SimpleInfoScreenOnPrefabInitPatch.GetResumeNewPanel();
                var attributesPanel = SimpleInfoScreenOnPrefabInitPatch.GetAttributesNewPanel();

                // 确保面板创建成功
                bool panelsExist = traitsNewPanel != null || resumeNewPanel != null || attributesPanel != null;

                if (!panelsExist)
                {
                    Debug.LogWarning("新面板尚未创建");
                    return;
                }

             
                KPrefabID kprefabid = selectedTarget.GetComponent<KPrefabID>();
                if (kprefabid != null)
                {
                    bool hasTag = selectedTarget.HasTag("KmodMiniBrainCore");

                    // 刷新面板内容或隐藏面板
                    if (hasTag)
                    {
                        NewTraitsRefreshInfoPanel(traitsNewPanel, selectedTarget);
                        NewRefreshResumePanel(resumeNewPanel, selectedTarget);
                        NewAttributesPanel(attributesPanel, selectedTarget);

                    }
                    else
                    {
                        traitsNewPanel.SetActive(false);
                        resumeNewPanel.SetActive(false);
                        attributesPanel.SetActive(false);

                    }
                }
                else
                {
                    traitsNewPanel.SetActive(false);
                    resumeNewPanel.SetActive(false);
                    attributesPanel.SetActive(false);
                }
            }
        }


        public static void NewTraitsRefreshInfoPanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
        {
            if (targetEntity == null)
            {
                return;
            }
            if (targetPanel == null)
            {
                return;
            }

            KPrefabID kprefabid = targetEntity.GetComponent<KPrefabID>();
            if (kprefabid == null)
            {
                targetPanel.SetActive(false);
                return;
            }

            if (!targetEntity.HasTag("KmodMiniBrainCore"))
            {
                targetPanel.SetActive(false);
                return;
            }

            targetPanel.SetActive(true);
            foreach (Trait trait in targetEntity.GetComponent<Traits>().TraitList)
            {
                if (!string.IsNullOrEmpty(trait.Name))
                {
                    targetPanel.SetLabel(trait.Id, trait.Name, trait.GetTooltip());
                }
            }
            targetPanel.Commit();
        }



        private static void NewRefreshResumePanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
        {

            if (targetEntity == null)
            {
                return;
            }
            if (targetPanel == null)
            {
                return;
            }

            KPrefabID kprefabid = targetEntity.GetComponent<KPrefabID>();
            if (kprefabid == null)
            {
                targetPanel.SetActive(false);
                return;
            }

            if (!targetEntity.HasTag("KmodMiniBrainCore"))
            {
                targetPanel.SetActive(false);
                return;
            }

            targetPanel.SetActive(true);
            MinionBrainResume component = targetEntity.GetComponent<MinionBrainResume>();
            // targetPanel.SetTitle(string.Format(UI.DETAILTABS.PERSONALITY.GROUPNAME_RESUME, targetEntity.name.ToUpper()));
            List<Skill> list = new List<Skill>();
            foreach (KeyValuePair<string, bool> keyValuePair in component.MasteryBySkillID)
            {
                if (keyValuePair.Value)
                {
                    Skill skill = Db.Get().Skills.Get(keyValuePair.Key);
                    list.Add(skill);
                }
            }
            targetPanel.SetLabel("mastered_skills_header", UI.DETAILTABS.PERSONALITY.RESUME.MASTERED_SKILLS, UI.DETAILTABS.PERSONALITY.RESUME.MASTERED_SKILLS_TOOLTIP);
            if (list.Count == 0)
            {
                targetPanel.SetLabel("no_skills", "  • " + UI.DETAILTABS.PERSONALITY.RESUME.NO_MASTERED_SKILLS.NAME, string.Format(UI.DETAILTABS.PERSONALITY.RESUME.NO_MASTERED_SKILLS.TOOLTIP, targetEntity.name));
            }
            else
            {
                foreach (Skill skill2 in list)
                {
                    string text = "";
                    foreach (SkillPerk skillPerk in skill2.perks)
                    {
                        text = text + "  • " + skillPerk.Name + "\n";
                    }
                    targetPanel.SetLabel(skill2.Id, "  • " + skill2.Name, skill2.description + "\n" + text);
                }
            }
            targetPanel.Commit();
        }

        private static void NewAttributesPanel(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
        {
            if (targetEntity == null)
            {
                return;
            }
            if (targetPanel == null)
            {
                return;
            }

            KPrefabID kprefabid = targetEntity.GetComponent<KPrefabID>();
            if (kprefabid == null)
            {
                targetPanel.SetActive(false);
                return;
            }

            if (!targetEntity.HasTag("KmodMiniBrainCore"))
            {
                targetPanel.SetActive(false);
                return;
            }

            targetPanel.SetActive(true);

            List<AttributeInstance> list = new List<AttributeInstance>(targetEntity.GetAttributes().AttributeTable).FindAll((AttributeInstance a) => a.Attribute.ShowInUI == Klei.AI.Attribute.Display.Skill);

            if (list.Count > 0)
            {
              
                foreach (AttributeInstance attributeInstance in list)
                {
                    targetPanel.SetLabel(attributeInstance.Attribute.Id, string.Format("{0}: {1}", attributeInstance.Name, attributeInstance.GetFormattedValue()), attributeInstance.GetAttributeValueTooltip());
                  
                }
            }
            targetPanel.Commit();
        }
    }

}
