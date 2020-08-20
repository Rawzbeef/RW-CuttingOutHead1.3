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
    public class CompProperties_HeadAssignable : CompProperties
    {
        public CompProperties_HeadAssignable()
        {
            compClass = typeof(CompHeadAssignable);
        }
    }
}
