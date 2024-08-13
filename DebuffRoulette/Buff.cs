using Database;
using KModTool;

namespace DebuffRoulette
{
   
    internal class Buff
    {
       
        public static void Register(ModifierSet parent)
        {
            Attributes attributes = Db.Get().Attributes;
            Amounts amounts = Db.Get().Amounts;
            new KModEffectConfigurator("shuailao", 3600f, false)
               .SetEffectName("衰老")
               .SetEffectDescription("人老难免有不中用的时候")
               .AddAttributeModifier(attributes.Athletics.Id, -6f, false, false, true)// 运动
               .AddAttributeModifier(attributes.Strength.Id, -5f, false, false, true)//力量
               .AddAttributeModifier(attributes.Digging.Id, -5f, false, false, true)// 挖掘
               .AddAttributeModifier(attributes.Immunity.Id, -2f, false, false, true)// 免疫系统
               .ApplyTo(parent);


            new KModEffectConfigurator("debuff1", 3600f, false)
              .SetEffectName("测试1")
              .SetEffectDescription("这是DeBuff添加测试")

              .ApplyTo(parent);


            new KModEffectConfigurator("debuff2", 3600f, false)
           .SetEffectName("测试2")
           .SetEffectDescription("这是DeBuff添加测试")

           .ApplyTo(parent);


            new KModEffectConfigurator("debuff3", 3600f, false)
           .SetEffectName("测试3")
           .SetEffectDescription("这是DeBuff添加测试")

           .ApplyTo(parent);


            new KModEffectConfigurator("debuff4", 3600f, false)
           .SetEffectName("测试4")
           .SetEffectDescription("这是DeBuff添加测试")

           .ApplyTo(parent);



        }
    }
}