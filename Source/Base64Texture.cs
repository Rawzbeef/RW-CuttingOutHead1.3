using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RWBeheading
{
    public class Base64Texture : IExposable
    {
        public Texture2D Texture
        {
            get
            {
                if (_tex == null)
                {
                    _tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                    _tex.LoadRawTextureData(_texRawData);
                    _tex.Apply();

                    _texRawData = null;
                }

                return _tex;
            }
            set
            {
                _tex = value;
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                string b64Str = string.Empty;
                if (_tex != null)
                {
                    byte[] rawTextureData = _tex.GetRawTextureData();
                    b64Str = Convert.ToBase64String(rawTextureData);
                }

                Scribe.saver.WriteElement("encoded", b64Str);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string b64Str = ScribeExtractor.ValueFromNode(Scribe.loader.curXmlParent["encoded"], "");
                if (!b64Str.NullOrEmpty())
                {
                    _texRawData = Convert.FromBase64String(b64Str);
                }
            }
        }

        [Unsaved]
        private Texture2D _tex;

        [Unsaved]
        private byte[] _texRawData;
    }
}
