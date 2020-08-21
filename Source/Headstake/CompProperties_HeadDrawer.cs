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
    public class CompProperties_HeadDrawer : CompProperties
    {
        public Vector2 drawOffset = Vector2.zero;
        public AltitudeLayer altitudeLayer = AltitudeLayer.Item;
        public float altitudeSemiOffset = 0f;

        public IntVec2 rotationRange = IntVec2.Zero;
        
        public CompProperties_HeadDrawer()
        {
            compClass = typeof(CompHeadDrawer);
        }
    }
}
