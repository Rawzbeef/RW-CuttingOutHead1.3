using System;
using Verse;
using RimWorld;

namespace RWBeheading
{
    public class PlaceWorker_ShowHeadsteakRadius : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            BHModSettings settings = BHModSettings.GetGlobalSettings();
            if (settings.headstakeFearRange > 0f)
            {
                GenDraw.DrawRadiusRing(loc, settings.headstakeFearRange);
            }
            return true;
        }
    }
}
