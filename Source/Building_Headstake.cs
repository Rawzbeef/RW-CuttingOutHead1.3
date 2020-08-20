using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace RWBeheading
{
    public class Building_Headstake : Building_Casket, IStoreSettingsParent, IHaulDestination, IThoughtGiver
    {
        private StorageSettings storageSettings;
        public bool HasFull => Head != null;

        public BeheadedHead Head
        {
            get
            {
                for (int i = 0; i < innerContainer.Count; i++)
                {
                    BeheadedHead head = innerContainer[i] as BeheadedHead;
                    if (head != null)
                    {
                        return head;
                    }
                }
                return null;
            }
        }

        public bool StorageTabVisible
        {
            get
            {
                return !HasFull;
            }
        }

        public Thought_Memory GiveObservedThought()
        {
            if (!HasFull)
            {
                return null;
            }

            Thought_MemoryObservation thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(BHDefOf.BH_Fearful);
            thought_MemoryObservation.Target = this;
            return thought_MemoryObservation;
        }

        public StorageSettings GetStoreSettings()
        {
            return storageSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public override void PostMake()
        {
            base.PostMake();
            storageSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                storageSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
        }

        public override void EjectContents()
        {
            base.EjectContents();
            if (base.Spawned)
            {
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (StorageTabVisible)
            {
                foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings))
                {
                    yield return item;
                }
            }
        }

        public override bool Accepts(Thing thing)
        {
            if (!base.Accepts(thing))
            {
                return false;
            }
            if (HasFull)
            {
                return false;
            }

            BeheadedHead head = thing as BeheadedHead;
            if (head == null)
            {
                return false;
            }
            else if (!storageSettings.AllowedToAccept(thing))
            {
                return false;
            }

            return true;
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                BeheadedHead head = thing as BeheadedHead;

                if (base.Spawned)
                {
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                }
                return true;
            }

            return false;
        }
        
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (HasFull)
            {
                stringBuilder.Append("BH_HeadstakeInspect".Translate(Head.OwnerName));
            }

            return stringBuilder.ToString();
        }
    }
}
