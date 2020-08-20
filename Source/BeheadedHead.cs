using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace RWBeheading
{
    public class BeheadedHead : ThingWithComps, IThoughtGiver
    {
        public BeheadedHead()
        {
        }

        public string OwnerName => _pawnName;
        public bool IsColonist => _isColonist;

        public override Graphic Graphic
        {
            get
            {
                switch (CurRotDrawMode)
                {
                    case RotDrawMode.Fresh:
                        return _graphicFresh;
                    case RotDrawMode.Rotting:
                        return _graphicRotting;
                    case RotDrawMode.Dessicated:
                        return _graphicDessicated;
                }

                return null;
            }
        }

        public RotDrawMode CurRotDrawMode
        {
            get
            {
                CompRottable comp = GetComp<CompRottable>();
                if (comp != null)
                {
                    if (comp.Stage == RotStage.Rotting)
                    {
                        return RotDrawMode.Rotting;
                    }
                    if (comp.Stage == RotStage.Dessicated)
                    {
                        return RotDrawMode.Dessicated;
                    }
                }
                return RotDrawMode.Fresh;
            }
        }

        public override string LabelNoCount
        {
            get
            {
                return "BH_HeadLabel".Translate(_pawnName);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref _pawnName, "pawnName");
            Scribe_Values.Look(ref _uniqueRotation, "rotation");
            Scribe_Values.Look(ref _isColonist, "isColonist");

            Scribe_Deep.Look(ref _graphicFresh, "graphicFresh");
            Scribe_Deep.Look(ref _graphicRotting, "graphicRotten");
            Scribe_Deep.Look(ref _graphicDessicated, "graphicDessi");
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (Graphic != null)
            {
                Graphic.Draw(drawLoc, Rot4.South, this, _uniqueRotation);
            }
        }

        public void Init(Pawn pawn)
        {
            _pawnName = pawn.Label;
            _isColonist = pawn.IsColonist;

            _graphicFresh = new Graphic_Head(pawn, RotDrawMode.Fresh);
            _graphicRotting = new Graphic_Head(pawn, RotDrawMode.Rotting);
            _graphicDessicated = new Graphic_Head(pawn, RotDrawMode.Dessicated);
        }

        public Thought_Memory GiveObservedThought()
        {
            if (this.StoringThing() == null)
            {
                Thought_MemoryObservation thought_MemoryObservation = (!this.IsNotFresh()) ? ((Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedLayingCorpse)) : ((Thought_MemoryObservation)ThoughtMaker.MakeThought(ThoughtDefOf.ObservedLayingRottingCorpse));
                thought_MemoryObservation.Target = this;
                return thought_MemoryObservation;
            }

            return null;
        }

        private string _pawnName = "Dummy";
        private float _uniqueRotation = Rand.Range(-30f, 30f);
        private bool _isColonist = false;

        private Graphic_Head _graphicFresh;
        private Graphic_Head _graphicRotting;
        private Graphic_Head _graphicDessicated;
    }
}
