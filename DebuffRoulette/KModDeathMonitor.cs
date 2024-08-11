using System;
using STRINGS;
using UnityEngine;

namespace DebuffRoulette
{
    public class KModDeathMonitor : GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>
    {
       
        public override void InitializeStates(out StateMachine.BaseState default_state)
        {
            default_state = this.alive;
            base.serializable = StateMachine.SerializeType.Both_DEPRECATED;
            this.alive.ParamTransition<Death>(this.death, this.dying_duplicant, (KModDeathMonitor.Instance smi, Death p) => p != null && smi.IsDuplicant).ParamTransition<Death>(this.death, this.dying_creature, (KModDeathMonitor.Instance smi, Death p) => p != null && !smi.IsDuplicant);
            this.dying_duplicant.ToggleAnims("anim_emotes_default_kanim", 0f).ToggleTag(GameTags.Dying).ToggleChore((KModDeathMonitor.Instance smi) => new DieChore(smi.master, this.death.Get(smi)), this.die);
            this.dying_creature.ToggleBehaviour(GameTags.Creatures.Die, (KModDeathMonitor.Instance smi) => true, delegate (KModDeathMonitor.Instance smi)
            {
                smi.GoTo(this.dead_creature);
            });


            this.die.ToggleTag(GameTags.Dying)
                .Enter("Die", delegate (KModDeathMonitor.Instance smi)
                {
                    smi.gameObject.AddTag(GameTags.PreventChoreInterruption);
                    // Death death = this.death.Get(smi);
                    if (smi.IsDuplicant)
                    {
                        // DeathMessage deathMessage = new DeathMessage(smi.gameObject, death);
                        KFMOD.PlayOneShot(GlobalAssets.GetSound("Death_Notification_localized", false), smi.master.transform.GetPosition(), 1f);
                        KFMOD.PlayUISound(GlobalAssets.GetSound("Death_Notification_ST", false));
                        // Messenger.Instance.QueueMessage(deathMessage);
                    }
                })
                .TriggerOnExit(GameHashes.Died, null)
                .GoTo(this.dead);


            this.dead.ToggleAnims("anim_emotes_default_kanim", 0f).DefaultState(this.dead.ground).ToggleTag(GameTags.Dead)
                .Enter(delegate (KModDeathMonitor.Instance smi)
                {
                    smi.ApplyDeath();
                    Game.Instance.Trigger(282337316, smi.gameObject);
                });

            this.dead.ground.Enter(delegate (KModDeathMonitor.Instance smi)
            {
                Death death2 = this.death.Get(smi);
                if (death2 == null)
                {
                    death2 = Db.Get().Deaths.Generic;
                }
                if (smi.IsDuplicant)
                {
                    smi.GetComponent<KAnimControllerBase>().Play(death2.loopAnim, KAnim.PlayMode.Loop, 1f, 0f);
                }
            }).EventTransition(GameHashes.OnStore, this.dead.carried, (KModDeathMonitor.Instance smi) => smi.IsDuplicant && smi.HasTag(GameTags.Stored));
            this.dead.carried.ToggleAnims("anim_dead_carried_kanim", 0f).PlayAnim("idle_default", KAnim.PlayMode.Loop).EventTransition(GameHashes.OnStore, this.dead.ground, (KModDeathMonitor.Instance smi) => !smi.HasTag(GameTags.Stored));
            this.dead_creature.Enter(delegate (KModDeathMonitor.Instance smi)
            {
                smi.gameObject.AddTag(GameTags.Dead);
            }).PlayAnim("idle_dead", KAnim.PlayMode.Loop);
        }

        // Token: 0x04005147 RID: 20807
        public GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.State alive;

        // Token: 0x04005148 RID: 20808
        public GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.State dying_duplicant;

        // Token: 0x04005149 RID: 20809
        public GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.State dying_creature;

        // Token: 0x0400514A RID: 20810
        public GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.State die;

        // Token: 0x0400514B RID: 20811
        public KModDeathMonitor.Dead dead;

        // Token: 0x0400514C RID: 20812
        public KModDeathMonitor.Dead dead_creature;

        public StateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.ResourceParameter<Death> death;

        public class Def : StateMachine.BaseDef
        {
        }

        public class Dead : GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.State
        {
            // Token: 0x0400514E RID: 20814
            public GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.State ground;

            // Token: 0x0400514F RID: 20815
            public GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.State carried;
        }

        public new class Instance : GameStateMachine<KModDeathMonitor, KModDeathMonitor.Instance, IStateMachineTarget, KModDeathMonitor.Def>.GameInstance
        {
            // Token: 0x06006CAA RID: 27818 RVA: 0x00043FD5 File Offset: 0x000421D5
            public Instance(IStateMachineTarget master, KModDeathMonitor.Def def)
                : base(master, def)
            {
                this.isDuplicant = base.GetComponent<MinionIdentity>();
            }

            public bool IsDuplicant
            {
                get
                {
                    return this.isDuplicant;
                }
            }

            public void Kill(Death death)
            {
                base.sm.death.Set(death, base.smi, false);
            }

            public void PickedUp(object data = null)
            {
                if (data is Storage || (data != null && (bool)data))
                {
                    base.smi.GoTo(base.sm.dead.carried);
                }
            }

            public bool IsDead()
            {
                return base.smi.IsInsideState(base.smi.sm.dead);
            }

            public void ApplyDeath()
            {
                if (this.isDuplicant)
                {
                    Game.Instance.assignmentManager.RemoveFromAllGroups(base.GetComponent<MinionIdentity>().assignableProxy.Get());
                    base.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().DuplicantStatusItems.Dead, base.smi.sm.death.Get(base.smi));
                    float num = 600f - GameClock.Instance.GetTimeSinceStartOfReport();
                    ReportManager.Instance.ReportValue(ReportManager.ReportType.PersonalTime, num, string.Format(UI.ENDOFDAYREPORT.NOTES.PERSONAL_TIME, DUPLICANTS.CHORES.IS_DEAD_TASK), base.smi.master.gameObject.GetProperName());
                    Pickupable component = base.GetComponent<Pickupable>();
                    if (component != null)
                    {
                        component.UpdateListeners(true);
                    }
                }
                base.GetComponent<KPrefabID>().AddTag(GameTags.Corpse, false);
            }

            private bool isDuplicant;
        }
    }
}
