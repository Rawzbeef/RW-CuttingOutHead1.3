using System;
using Verse;
using RimWorld;
using UnityEngine;

namespace RWBeheading
{
    public class BHModSettings : ModSettings
    {
        public static BHModSettings GetGlobalSettings()
        {
            if (_modSettings == null)
            {
                _modSettings = LoadedModManager.GetMod<BHMod>().GetSettings<BHModSettings>();
            }

            return _modSettings;
        }

        public BHModSettings()
        {
            ResetToDefault();
        }

        public void ResetToDefault()
        {
            headstakeFearRange = 10.0f;
            headstakeFearChance = 0.04f;
            headstakeFearChanceDoubledIfSameFaction = true;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref headstakeFearRange, "headstakeFearRange");
            Scribe_Values.Look(ref headstakeFearChance, "headstakeFearChance");
            Scribe_Values.Look(ref headstakeFearChanceDoubledIfSameFaction, "headstakeFearChanceDoubledIfSameFaction");
            base.ExposeData();
        }

        public float headstakeFearRange;
        public float headstakeFearChance;
        public bool headstakeFearChanceDoubledIfSameFaction;
        private static BHModSettings _modSettings = null;
    }

    public class BHMod : Mod
    {
        public BHMod(ModContentPack content) : base(content)
        {
            _settings = GetSettings<BHModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            
            if (listing.ButtonTextLabeled("", "BH_Config_SetDefaults".Translate()))
            {
                _settings.ResetToDefault();
            }

            listing.Label("BH_Config_Headstake_Fear_Range".Translate());
            _settings.headstakeFearRange = Widgets.HorizontalSlider(listing.GetRect(22f), _settings.headstakeFearRange, 0f, 20f, false, rightAlignedLabel: string.Format("{0:f2}", _settings.headstakeFearRange), roundTo: 1f);
            
            listing.Label("BH_Config_Headstake_Fear_Chance".Translate());
            _settings.headstakeFearChance = Widgets.HorizontalSlider(listing.GetRect(22f), _settings.headstakeFearChance, 0f, 1f, false, rightAlignedLabel: string.Format("{0:f2}", _settings.headstakeFearChance));

            listing.Label("BH_Config_Headstake_Fear_Chance_Doubled_Same_Faction".Translate());
            Widgets.CheckboxLabeled(listing.GetRect(22f), "", ref _settings.headstakeFearChanceDoubledIfSameFaction);

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }

        private BHModSettings _settings;
    }
}
