using System;
using Verse;
using RimWorld;

namespace RWBeheading
{
    public static class BHUtility
    {
        public static bool CheckPawnCanBeFeared(Pawn pawn)
        {
            if (pawn != null && pawn.RaceProps != null && pawn.RaceProps.Humanlike)
            {
                if (!pawn.story.traits.HasTrait(TraitDefOf.Psychopath) &&
                    !pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) &&
                    !pawn.story.traits.HasTrait(TraitDefOf.Cannibal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
