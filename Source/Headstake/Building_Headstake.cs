using System.Collections.Generic;
using System.Text;
using Verse;
using RimWorld;

namespace RWBeheading
{
    public class Building_Headstake : Building_Casket, IStoreSettingsParent, IHaulDestination, IHeadDataContainer, IThoughtGiver
    {
        private StorageSettings storageSettings;
        public bool HasFull => Head != null;

        public HumanlikeHead Head
        {
            get
            {
                for (int i = 0; i < innerContainer.Count; i++)
                {
                    HumanlikeHead head = innerContainer[i] as HumanlikeHead;
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
        
        public StorageSettings GetStoreSettings()
        {
            return storageSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public override void TickLong()
        {
            if (Head == null)
            {
                return;
            }

            BHModSettings modSettings = BHModSettings.GetGlobalSettings();
            float r = modSettings.headstakeFearRange * modSettings.headstakeFearRange;
            for (int i = 0; (float)i < r; i++)
            {
                IntVec3 intVec = Position + GenRadial.RadialPattern[i];
                if (!intVec.InBounds(Map) || !GenSight.LineOfSight(intVec, Position, Map, true))
                {
                    continue;
                }

                Pawn pawn = intVec.GetFirstPawn(Map);
                if (pawn != null)
                {
                    if (pawn.health == null || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
                    {
                        continue;
                    }

                    if (pawn.IsColonist)
                    {
                        continue;
                    }

                    if (!BHUtility.CheckPawnCanBeFeared(pawn))
                    {
                        continue;
                    }

                    float chance = modSettings.headstakeFearChance;
                    if (modSettings.headstakeFearChanceDoubledIfSameFaction && pawn.Faction != null && pawn.Faction.def.defName == Head.GetInnerHeadData().FactionDefName)
                    {
                        chance *= 2.0f;
                    }

                    if (Rand.Chance(chance))
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null);
                    }
                }
            }
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

        public override void DrawExtraSelectionOverlays()
        {
            BHModSettings settings = BHModSettings.GetGlobalSettings();
            if (settings.headstakeFearRange > 0f)
            {
                GenDraw.DrawRadiusRing(Position, settings.headstakeFearRange);
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

            HumanlikeHead head = thing as HumanlikeHead;
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
                HumanlikeHead head = thing as HumanlikeHead;

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

        public HumanlikeHeadData GetInnerHeadData()
        {
            if (Head != null)
            {
                return Head.GetInnerHeadData();
            }

            return null;
        }

        public void SetHeadData(HumanlikeHeadData data)
        {
            CustomLogger.Error("[Beheading] Can't set head data to this.", 153487312);
        }

        public Thought_Memory GiveObservedThought()
        {
            if (Head == null)
            {
                return null;
            }

            return Head.GiveObservedThought();
        }
    }
}
