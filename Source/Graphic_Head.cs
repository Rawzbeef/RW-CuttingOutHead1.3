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
    public class Graphic_Head : Graphic, IExposable
    {
        public override Material MatSingle => InnerMaterial;

        public override Material MatWest => InnerMaterial;
        public override Material MatSouth => InnerMaterial;
        public override Material MatEast => InnerMaterial;
        public override Material MatNorth => InnerMaterial;

        private Material InnerMaterial
        {
            get
            {
                if (_material == null)
                {
                    GenerateMaterial();
                }

                return _material;
            }
        }
        
        public void ExposeData()
        {
            Scribe_Values.Look(ref _pawnThingID, "pawnThingID");
            Scribe_Deep.Look(ref _tex, "texData");
        }

        public Graphic_Head()
        {
        }

        public Graphic_Head(Pawn pawn, RotDrawMode drawMode)
        {
            _pawnThingID = pawn.ThingID;
            GenerateHeadTexture(pawn, drawMode);
            GenerateMaterial();
        }

        public override void Init(GraphicRequest req)
        {
        }

        public void CopyFrom(Graphic_Head head)
        {
            _pawnThingID = head._pawnThingID;
            _tex = head._tex;
            _material = head._material;
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return InnerMaterial;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Mesh mesh = MeshPool.GridPlane(Vector2.one);
            Quaternion quat = Quaternion.identity;
            if (extraRotation != 0f)
            {
                quat = Quaternion.AngleAxis(extraRotation, Vector3.up);
            }
            
            Graphics.DrawMesh(mesh, loc, quat, InnerMaterial, 0);
        }
        
        private void GenerateHeadTexture(Pawn pawn, RotDrawMode drawMode)
        {
            RenderTexture rt = new RenderTexture(128, 128, 24)
            {
                filterMode = FilterMode.Bilinear
            };

            HeadCameraManager.Renderer.RenderHead(rt, pawn, drawMode);

            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            tex.name = "_HeadTex_" + _pawnThingID;

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            rt.Release();

            _tex = new Base64Texture();
            _tex.Texture = tex;
        }

        private void GenerateMaterial()
        {
            if (_dummyMaterial == null)
            {
                _dummyMaterial = MaterialPool.MatFrom(ContentFinder<Texture2D>.Get("Things/HeadDummy"), ShaderDatabase.Cutout, Color.white);
            }

            if (_material == null || _material.name == GenerateMaterialName("dummy"))
            {
                if (_tex != null)
                {
                    _material = new Material(ShaderDatabase.Cutout);
                    _material.name = GenerateMaterialName(_pawnThingID);
                    _material.mainTexture = _tex.Texture;
                    _material.color = Color.white;
                }
                else
                {
                    _material = _dummyMaterial;
                }
            }
        }

        private string GenerateMaterialName(string v)
        {
            return ShaderDatabase.Cutout + "_HeadMat_" + v;
        }

        private string _pawnThingID;
        private Base64Texture _tex = null;

        [Unsaved]
        private Material _material = null;

        [Unsaved]
        private static Material _dummyMaterial = null;
    }
}
