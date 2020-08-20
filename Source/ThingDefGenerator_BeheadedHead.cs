using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace RWBeheading
{
    [StaticConstructorOnStartup]
    public static class ThingDefGenerator_BeheadedHead
    {
        static ThingDefGenerator_BeheadedHead()
        {
            foreach (ThingDef def in GenerateThingDefs())
            {
                DefGenerator.AddImpliedDef(def);
            }
        }

        public static string GetDefName(ThingDef pawnDef)
        {
            return "BeheadedHead_" + pawnDef.defName;
        }

        public static ThingDef GetGeneratedDef(ThingDef pawnDef)
        {
            if (!_cachedHeadDefs.ContainsKey(pawnDef.defName))
            {
                return null;
            }

            return _cachedHeadDefs[pawnDef.defName];
        }

        public static IEnumerable<ThingDef> GenerateThingDefs()
        {
            foreach (ThingDef td in DefDatabase<ThingDef>.AllDefs.ToList())
            {
                if (td.category == ThingCategory.Pawn)
                {
                    if (!td.race.Humanlike)
                    {
                        continue;
                    }

                    ThingDef thingDef = new ThingDef();
                    thingDef.category = ThingCategory.Item;
                    thingDef.thingClass = typeof(HumanlikeHead);
                    thingDef.graphicData = new GraphicData();
                    thingDef.graphicData.texPath = "Things/HeadDummy";
                    thingDef.graphicData.graphicClass = typeof(Graphic_Head);
                    thingDef.selectable = true;
                    thingDef.rotatable = true;
                    thingDef.tickerType = TickerType.Rare;
                    thingDef.altitudeLayer = AltitudeLayer.ItemImportant;
                    thingDef.scatterableOnMapGen = false;
                    thingDef.SetStatBaseValue(StatDefOf.Beauty, -50f);
                    thingDef.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);
                    thingDef.alwaysHaulable = true;
                    thingDef.soundPickup = SoundDefOf.Corpse_Drop;
                    thingDef.soundDrop = SoundDefOf.Corpse_Drop;
                    thingDef.pathCost = DefGenerator.StandardItemPathCost;
                    thingDef.socialPropernessMatters = false;
                    thingDef.tradeability = Tradeability.None;
                    thingDef.messageOnDeteriorateInStorage = false;
                    thingDef.inspectorTabs = new List<Type>();
                    thingDef.comps.Add(new CompProperties_Forbiddable());
                    thingDef.recipes = new List<RecipeDef>();
                    thingDef.shortHash = (ushort)(td.shortHash ^ 12312);
                    thingDef.menuHidden = false;

                    thingDef.defName = GetDefName(td);
                    thingDef.label = "BH_HeadLabel".Translate(td.label);
                    thingDef.description = "BH_HeadDesc".Translate(td.label);
                    thingDef.soundImpactDefault = td.soundImpactDefault;
                    thingDef.SetStatBaseValue(StatDefOf.MarketValue, 0);
                    thingDef.SetStatBaseValue(StatDefOf.Flammability, td.GetStatValueAbstract(StatDefOf.Flammability));
                    thingDef.SetStatBaseValue(StatDefOf.MaxHitPoints, td.BaseMaxHitPoints * 0.3f);
                    thingDef.SetStatBaseValue(StatDefOf.Mass, td.statBases.GetStatOffsetFromList(StatDefOf.Mass) * 0.3f);
                    thingDef.modContentPack = td.modContentPack;
                    thingDef.ingestible = null;

                    if (td.race.IsFlesh)
                    {
                        CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
                        compProperties_Rottable.daysToRotStart = 2.5f;
                        compProperties_Rottable.daysToDessicated = 5f;
                        compProperties_Rottable.rotDamagePerDay = 2f;
                        compProperties_Rottable.dessicatedDamagePerDay = 0.7f;
                        thingDef.comps.Add(compProperties_Rottable);

                        CompProperties_SpawnerFilth compProperties_SpawnerFilth = new CompProperties_SpawnerFilth();
                        compProperties_SpawnerFilth.filthDef = ThingDefOf.Filth_CorpseBile;
                        compProperties_SpawnerFilth.spawnCountOnSpawn = 0;
                        compProperties_SpawnerFilth.spawnMtbHours = 0f;
                        compProperties_SpawnerFilth.spawnRadius = 0.1f;
                        compProperties_SpawnerFilth.spawnEveryDays = 1f;
                        compProperties_SpawnerFilth.requiredRotStage = RotStage.Rotting;
                        thingDef.comps.Add(compProperties_SpawnerFilth);
                    }

                    thingDef.thingCategories = new List<ThingCategoryDef>();
                    thingDef.thingCategories.Add(BHDefOf.HeadsHumanlike);

                    BHDefOf.HeadsHumanlike.childThingDefs.Add(thingDef);

                    var node = ThingCategoryNodeDatabase.allThingCategoryNodes.First(x => x.catDef == BHDefOf.HeadsHumanlike);
                    
                    _cachedHeadDefs.Add(td.defName, thingDef);
                    yield return thingDef;
                }
            }

            // Resolve References one more.
            foreach (ThingDef td in DefDatabase<ThingDef>.AllDefs)
            {
                if (td.building != null && td.building.fixedStorageSettings != null)
                {
                    td.building.fixedStorageSettings.filter.ResolveReferences();
                    td.building.defaultStorageSettings.filter.ResolveReferences();
                }
            }

            foreach (RecipeDef rd in DefDatabase<RecipeDef>.AllDefs)
            {
                foreach (var ic in rd.ingredients)
                {
                    ic.filter.ResolveReferences();
                }

                if (rd.fixedIngredientFilter != null)
                {
                    rd.fixedIngredientFilter.ResolveReferences();
                }

                if (rd.defaultIngredientFilter != null)
                {
                    rd.defaultIngredientFilter.ResolveReferences();
                }
            }
        }

        private static Dictionary<string, ThingDef> _cachedHeadDefs = new Dictionary<string, ThingDef>();
    }
}
