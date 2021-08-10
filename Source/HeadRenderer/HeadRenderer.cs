using System;
using System.Reflection;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace RWBeheading
{
    public class HeadRenderer : MonoBehaviour
    {
        public void RenderHead(RenderTexture rt, Pawn pawn, RotDrawMode rotDrawMode)
        {
            if (!UnityData.IsInMainThread)
            {
                CustomLogger.NeedCheck("Tried to render a head material from a different thread.");
                return;
            }

            Camera cam = HeadCameraManager.Camera;
            cam.targetTexture = rt;
            Vector3 position = cam.transform.position;

            float orthographicSize = cam.orthographicSize;
            cam.transform.position += new Vector3(0f, 0f, 0.3f);
            cam.orthographicSize = 0.6f;

            this.pawn = pawn;
            this.drawMode = rotDrawMode;
            int tempTick = (int)fieldDamageFlasherTick.GetValue(pawn.Drawer.renderer.graphics.flasher);

            fieldDamageFlasherTick.SetValue(pawn.Drawer.renderer.graphics.flasher, -9999);
            
            cam.Render();

            fieldDamageFlasherTick.SetValue(pawn.Drawer.renderer.graphics.flasher, tempTick);

            this.pawn = null;
            this.drawMode = RotDrawMode.Fresh;
            cam.transform.position = position;
            cam.orthographicSize = orthographicSize;
            cam.targetTexture = null;
        }

        public void OnPostRender()
        {
            try
            {
                methodRenderPawnInternal.Invoke(pawn.Drawer.renderer, new object[] {
                    Vector3.zero,
                    0f,
                    false,
                    Rot4.South,
                    drawMode,
                    PawnRenderFlags.DrawNow
                });
            }
            catch (Exception e)
            {
#if DEBUG
                CustomLogger.ExceptionHandle(e);
#endif
            }
        }

        private Pawn pawn = null;
        private RotDrawMode drawMode = RotDrawMode.Fresh;

        private static MethodInfo methodRenderPawnInternal = AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", parameters: new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags)});

        private static FieldInfo fieldDamageFlasherTick = AccessTools.Field(typeof(DamageFlasher), "lastDamageTick");
    }
}
