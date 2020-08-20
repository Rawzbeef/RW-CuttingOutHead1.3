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
    public class CompProperties_HeadContainer : CompProperties
    {
        public CompProperties_HeadContainer()
        {
            compClass = typeof(CompHeadGraphicContainer);
        }
    }
}
