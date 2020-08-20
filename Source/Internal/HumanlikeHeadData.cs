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
    public class HumanlikeHeadData : IExposable
    {
        public string ThingID => _pawnThingID;

        public RotStage RotStage
        {
            get
            {
                return _pawnRotStage;
            }
            set
            {
                _pawnRotStage = value;
            }
        }

        public Material Material
        {
            get
            {
                switch(_pawnRotStage)
                {
                    case RotStage.Fresh:
                        if (_matFresh == null)
                        {
                            if (_texFresh != null)
                            {
                                _matFresh = GenerateMaterial(_texFresh);
                            }
                            else
                            {
                                return GetDefaultMaterial();
                            }
                        }

                        return _matFresh;

                    case RotStage.Rotting:
                        if (_matRotting == null)
                        {
                            if (_texRotting != null)
                            {
                                _matRotting = GenerateMaterial(_texRotting);
                            }
                            else
                            {
                                return GetDefaultMaterial();
                            }
                        }

                        return _matRotting;

                    case RotStage.Dessicated:
                        if (_matSkull == null)
                        {
                            if (_texSkull != null)
                            {
                                _matSkull = GenerateMaterial(_texSkull);
                            }
                            else
                            {
                                return GetDefaultMaterial();
                            }
                        }

                        return _matSkull;
                }

                return null;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _pawnThingID, "pawnThingID");
            Scribe_Values.Look(ref _pawnRotStage, "pawnRotStage");

            Scribe_Deep.Look(ref _texFresh, "fleshTex");
            Scribe_Deep.Look(ref _texRotting, "rottenTex");
            Scribe_Deep.Look(ref _texSkull, "skullTex");
        }

        public HumanlikeHeadData()
        {
        }

        public HumanlikeHeadData(Pawn pawn)
        {
            _pawnThingID = pawn.ThingID;
            _pawnRotStage = RotStage.Fresh;

            _texFresh = GenerateHeadTexture(pawn, RotDrawMode.Fresh);
            _texRotting = GenerateHeadTexture(pawn, RotDrawMode.Rotting);
            _texSkull = GenerateHeadTexture(pawn, RotDrawMode.Dessicated);
        }

        public static Material GetDefaultMaterial()
        {
            return MaterialPool.MatFrom("Things/HeadDummy", ShaderDatabase.Cutout);
        }

        private Base64Texture GenerateHeadTexture(Pawn pawn, RotDrawMode drawMode)
        {
            CustomLogger.Dev("Try to generate head texture: {0}, {1}", pawn.Name, drawMode);

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

            var b64tex = new Base64Texture();
            b64tex.Texture = tex;
            return b64tex;
        }

        private Material GenerateMaterial(Base64Texture tex)
        {
            if (tex != null)
            {
                Material mat = new Material(ShaderDatabase.Cutout);
                mat.name = GenerateMaterialName(_pawnThingID);
                mat.mainTexture = tex.Texture;
                mat.color = Color.white;
                return mat;
            }
            else
            {
                return null;
            }
        }

        private string GenerateMaterialName(string v)
        {
            return ShaderDatabase.Cutout + "_HeadMat_" + v;
        }

        private string _pawnThingID = null;
        private RotStage _pawnRotStage = RotStage.Fresh;

        private Base64Texture _texFresh;
        private Base64Texture _texRotting;
        private Base64Texture _texSkull;

        [Unsaved]
        private Material _matFresh;
        [Unsaved]
        private Material _matRotting;
        [Unsaved]
        private Material _matSkull;

        

    }
}
