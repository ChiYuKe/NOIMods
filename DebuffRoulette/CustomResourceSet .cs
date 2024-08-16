using Klei.AI;
using System.Collections.Generic;
using UnityEngine;

public class TraitManager
{
    private readonly Traits _sourceTraitsComponent;
    private readonly Traits _targetTraitsComponent;

    public TraitManager(Traits sourceTraitsComponent, Traits targetTraitsComponent)
    {
        _sourceTraitsComponent = sourceTraitsComponent;
        _targetTraitsComponent = targetTraitsComponent;
    }

    public void TransferTraits()
    {
        GameObject prefab = Assets.GetPrefab("KmodMiniBrainCore");
        if (_sourceTraitsComponent == null || _targetTraitsComponent == null)
        {
            Debug.LogError("Source or Target Traits component is null.");
            return;
        }

        foreach (var trait in _sourceTraitsComponent.TraitList)
        {
            _targetTraitsComponent.Add(trait);
            PrintTraitInfo(trait);
        }
    }

    private void PrintTraitInfo(Klei.AI.Trait trait)
    {
        Debug.Log("Trait 信息:");
        Debug.Log($"ID: {trait.Id}");
        Debug.Log($"名称: {trait.Name}");
        Debug.Log($"描述: {trait.description}");
        Debug.Log($"评分: {trait.Rating}");
        Debug.Log($"是否需要保存: {trait.ShouldSave}");
        Debug.Log($"正面特质: {trait.PositiveTrait}");
        Debug.Log($"有效初始特质: {trait.ValidStarterTrait}");

        Debug.Log("忽略的效果:");
        foreach (var effect in trait.ignoredEffects)
        {
            string effectName = Strings.Get("STRINGS.DUPLICANTS.MODIFIERS." + effect.ToUpper() + ".NAME");
            Debug.Log($"效果: {effectName}");
        }

        Debug.Log("禁用的工作组:");
        if (trait.disabledChoreGroups != null)
        {
            foreach (var choreGroup in trait.disabledChoreGroups)
            {
                Debug.Log($"禁用的工作组: {choreGroup.Name}");
            }
        }

        string tooltip = trait.GetTooltip();
        Debug.Log($"工具提示: {tooltip}");

        string extendedTooltip = trait.GetExtendedTooltipStr();
        Debug.Log($"扩展提示: {extendedTooltip}");
    }
}
