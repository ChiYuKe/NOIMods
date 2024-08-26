﻿using System;
using System.Collections.Generic;
using DebuffRoulette;
using PeterHan.PLib.UI;
using STRINGS;
using TUNING;
using UnityEngine;

namespace DebuffRoulette
{
    public class KomdBuildingBrainConfig : IEntityConfig, BuildingInterface
    {
        // Token: 0x06000A0D RID: 2573 RVA: 0x0003A786 File Offset: 0x00038986
        public string[] GetDlcIds()
        {
            return DlcManager.AVAILABLE_ALL_VERSIONS;
        }


        public GameObject CreatePrefab()
        {
            string text = "KomdBuildingBrain";
            string text2 = "迁移测试机";
            string text3 = "死亡不可怕，被人遗忘才可怕";
            float num = 2000f;
            EffectorValues tier = global::TUNING.BUILDINGS.DECOR.BONUS.TIER0;
            EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER0;
            GameObject gameObject = EntityTemplates.CreatePlacedEntity(text, text2, text3, num, Assets.GetAnim("geneshuffler_kanim"), "on", Grid.SceneLayer.Building, 4, 3, tier, tier2, SimHashes.Creature, new List<Tag> { GameTags.Gravitas }, 293f);
            gameObject.AddTag(GameTags.NotRoomAssignable);
            PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
            component.SetElement(SimHashes.Unobtanium, true);
            component.Temperature = 294.15f;
            gameObject.AddOrGet<Operational>();
            gameObject.AddOrGet<Notifier>();
            gameObject.AddOrGet<BrainBuild>();
            LoreBearerUtil.AddLoreTo(gameObject, new LoreBearerAction(LoreBearerUtil.NerualVacillator));
            gameObject.AddOrGet<LoopingSounds>();
            gameObject.AddOrGet<Ownable>();
            gameObject.AddOrGet<Prioritizable>();
            gameObject.AddOrGet<Demolishable>();
            Storage storage = gameObject.AddOrGet<Storage>();
            storage.dropOnLoad = true;
          
            ManualDeliveryKG manualDeliveryKG = gameObject.AddOrGet<ManualDeliveryKG>();
            manualDeliveryKG.SetStorage(storage);
            manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;
            manualDeliveryKG.RequestedItemTag = new Tag("KmodMiniBrainCore");
            manualDeliveryKG.refillMass = 1f;
            manualDeliveryKG.MinimumMass = 1f;
            manualDeliveryKG.capacity = 1f;
            KBatchedAnimController kbatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
            kbatchedAnimController.sceneLayer = Grid.SceneLayer.BuildingBack;
            kbatchedAnimController.fgLayer = Grid.SceneLayer.BuildingFront;
            return gameObject;
        }

        public void OnPrefabInit(GameObject inst)
        {
          
            inst.GetComponent<BrainBuild>().workLayer = Grid.SceneLayer.Building;
            inst.GetComponent<Ownable>().slotID = Db.Get().AssignableSlots.GeneShuffler.Id;
            inst.GetComponent<OccupyArea>().objectLayers = new ObjectLayer[] { ObjectLayer.Building };
            inst.GetComponent<Deconstructable>();
        }

        public void OnSpawn(GameObject inst)
        {
        }
    }
}
