using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;

namespace RWBeheading
{
    public class Projectile_Head : Projectile_Explosive, IHeadDataContainer
    {
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look(ref _rotation, "rotation");
            Scribe_Deep.Look(ref _headData, "headData");
        }

        public override void Tick()
        {
            base.Tick();
            _rotation += 10f;
        }

        public override void Draw()
        {
            Graphic.DrawWorker(DrawPos, Rot4.South, null, this, _rotation);
        }

        public HumanlikeHeadData GetInnerHeadData()
        {
            return _headData;
        }

        public void SetHeadData(HumanlikeHeadData data)
        {
            if (_headData != null)
            {
                CustomLogger.NeedCheck("Tried to SetHeadData although has data.");
            }

            _headData = data;
        }

        private float _rotation = 0f;

        private HumanlikeHeadData _headData;
    }
}
