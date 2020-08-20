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
    public class HumanlikeHead : ThingWithComps, IThoughtGiver, IHeadDataContainer
    {
        public HumanlikeHead()
        {
        }

        public string OwnerName => _pawnName;

        public bool IsColonist => _isColonist;
        
        public void Init(Pawn pawn)
        {
            _pawnName = pawn.Label;
            _isColonist = pawn.IsColonist;

            _headData = new HumanlikeHeadData(pawn);
            _compRottable = GetComp<CompRottable>();
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

            Scribe_Deep.Look(ref _headData, "headData");
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (Graphic != null)
            {
                Graphic.Draw(drawLoc, Rot4.South, this, _uniqueRotation);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (_compRottable != null && _headData != null && _compRottable.Stage != _headData.RotStage)
            {
                _headData.RotStage = _compRottable.Stage;
            }
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

        public HumanlikeHeadData GetInnerHeadData()
        {
            return _headData;
        }

        public void SetHeadData(HumanlikeHeadData data)
        {
            if (data != null)
            {
                CustomLogger.NeedCheck("Tried to SetHeadData although has data.");
            }

            _headData = data;
        }

        private string _pawnName = "Dummy";
        private float _uniqueRotation = Rand.Range(-30f, 30f);
        private bool _isColonist = false;

        HumanlikeHeadData _headData;
        CompRottable _compRottable;
    }
}
