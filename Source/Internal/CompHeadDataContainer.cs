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
    public class CompHeadDataContainer : ThingComp, IHeadDataContainer
    {
        public CompHeadDataContainer()
        {
        }

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref _headData, "headData");
        }

        public HumanlikeHeadData GetInnerHeadData()
        {
            return _headData;
        }

        public void SetHeadData(HumanlikeHeadData data)
        {
            _headData = data;
        }

        private HumanlikeHeadData _headData;
    }
}
