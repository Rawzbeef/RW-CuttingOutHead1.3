using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;

namespace RWBeheading
{
    public class Projectile_Head : Projectile_Explosive
    {
        public override void Tick()
        {
            base.Tick();
            _rotation += 10f;
        }

        public override void Draw()
        {
            Graphic.DrawWorker(DrawPos, Rot4.South, null, null, _rotation);
        }

        private float _rotation = 0f;
    }
}
