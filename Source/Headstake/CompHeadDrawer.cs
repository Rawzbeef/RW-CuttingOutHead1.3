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
    public class CompHeadDrawer : ThingComp
    {
        public IHeadDataContainer HeadContainer => (IHeadDataContainer)parent;

        public CompProperties_HeadDrawer Props => (CompProperties_HeadDrawer)props;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref _uniqueRotation, "rotation");
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            _uniqueRotation = Rand.Range(Props.rotationRange.x, Props.rotationRange.z);
        }

        public override void PostDraw()
        {
            if (HeadContainer.GetInnerHeadData() != null)
            {
                Vector3 parentDrawPos = parent.DrawPos;
                Vector3 loc = new Vector3(parentDrawPos.x + Props.drawOffset.x, Props.altitudeLayer.AltitudeFor() + Props.altitudeSemiOffset, parentDrawPos.z + Props.drawOffset.y);
                
                Mesh mesh = MeshPool.GridPlane(Vector2.one);
                Quaternion quat = Quaternion.identity;
                if (_uniqueRotation != 0)
                {
                    quat = Quaternion.AngleAxis(_uniqueRotation, Vector3.up);
                }

                Graphics.DrawMesh(mesh, loc, quat, HeadContainer.GetInnerHeadData().Material, 0);
            }
        }

        private int _uniqueRotation;
    }
}
