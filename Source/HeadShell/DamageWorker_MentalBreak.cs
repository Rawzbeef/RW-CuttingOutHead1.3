using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RWBeheading
{
    public class DamageWorker_MentalBreak : DamageWorker
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            Pawn pawn = victim as Pawn;
            if (pawn != null && pawn.RaceProps != null && pawn.RaceProps.Humanlike)
            {
                if (!pawn.story.traits.HasTrait(TraitDefOf.Psychopath) &&
                    !pawn.story.traits.HasTrait(TraitDefOf.Bloodlust) &&
                    !pawn.story.traits.HasTrait(TraitDefOf.Cannibal))
                {
                    if (Rand.Bool)
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null);
                    }
                    else
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, null);
                    }
                }
            }

            return new DamageResult();
        }
    }
}
