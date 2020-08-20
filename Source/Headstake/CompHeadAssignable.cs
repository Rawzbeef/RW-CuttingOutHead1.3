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
    public class CompHeadAssignable : ThingComp
    {
        public Building_Headstake HeadStake => (Building_Headstake)parent;

        public CompHeadAssignable()
        {
            _uniqueRotation = Rand.Range(-15f, 15f);
        }

        public override void PostDraw()
        {
            if (HeadStake.Head != null)
            {
                Vector3 v = HeadStake.DrawPos;
                v.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
                v.z += HeadStakeZOffset;
                HeadStake.Head.Graphic.DrawWorker(v, Rot4.South, null, parent, _uniqueRotation);
            }
        }

        [TweakValue("BHValues", 0f, 1f)]
        private static float HeadStakeZOffset = 0.27f;

        private float _uniqueRotation;
    }
}
