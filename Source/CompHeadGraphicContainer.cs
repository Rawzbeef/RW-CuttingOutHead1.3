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
    public class CompHeadGraphicContainer : ThingComp
    {
        public Graphic_Head CurrentHeadGraphic => _graphicInt;

        public CompHeadGraphicContainer()
        {
        }

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref _graphicInt, "headGraphic");
        }

        public Graphic_Head Pop()
        {
            Graphic_Head head = _graphicInt;
            _graphicInt = null;
            return head;
        }

        public void Push(Graphic_Head grp)
        {
            _graphicInt = grp;
        }

        private Graphic_Head _graphicInt;
    }
}
