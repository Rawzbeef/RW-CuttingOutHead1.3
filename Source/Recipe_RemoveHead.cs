using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RWBeheading
{
    public class Recipe_RemoveHead : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return pawn.health.hediffSet.GetNotMissingParts().Where(x => x.def == BodyPartDefOf.Head);
        }
        
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            bool flag = MedicalRecipesUtility.IsClean(pawn, part);
            bool flag2 = IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer != null)
            {
                if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }

                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
                MedicalRecipesUtility.SpawnNaturalPartIfClean(pawn, part, billDoer.Position, billDoer.Map);
                MedicalRecipesUtility.SpawnThingsFromHediffs(pawn, part, billDoer.Position, billDoer.Map);
            }
            pawn.TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 99999f, 999f, -1f, null, part));

            if (flag)
            {
                if (pawn.Dead)
                {
                    ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, PawnExecutionKind.OrganHarvesting);
                }
                ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn);
            }
            if (flag2)
            {
                ReportViolation(pawn, billDoer, pawn.FactionOrExtraMiniOrHomeFaction, -70, "GoodwillChangedReason_RemovedBodyPart".Translate(part.LabelShort));
            }
        }
    }
}
