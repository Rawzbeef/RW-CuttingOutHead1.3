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
    public class Graphic_Head : Graphic
    {
        public override Material MatSingle => HumanlikeHeadData.GetDefaultMaterial();

        public override Material MatWest => MatSingle;
        public override Material MatSouth => MatSingle;
        public override Material MatEast => MatSingle;
        public override Material MatNorth => MatSingle;
        
        public Graphic_Head()
        {
        }

        public override void Init(GraphicRequest req)
        {
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return MatSingle;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Mesh mesh = MeshPool.GridPlane(Vector2.one);
            Quaternion quat = Quaternion.identity;
            if (extraRotation != 0f)
            {
                quat = Quaternion.AngleAxis(extraRotation, Vector3.up);
            }

            IHeadDataContainer container = thing as IHeadDataContainer;
            if (container != null)
            {
                if (container.GetInnerHeadData() != null)
                {
                    Graphics.DrawMesh(mesh, loc, quat, container.GetInnerHeadData().Material, 0);
                    return;
                }
            }

            Graphics.DrawMesh(mesh, loc, quat, MatSingle, 0);
        }
    }
}
