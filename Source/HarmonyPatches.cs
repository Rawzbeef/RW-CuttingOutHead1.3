using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

using HarmonyLib;

namespace RWBeheading
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static Type MakeRecipeProductsInnerType = null;
        private static Type MakeNewToilsInnerType = null;
        private static MethodInfo MakeNewToils_InitAction = null;

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("rimworld.gguake.beheading");

            harmony.Patch(AccessTools.Method(typeof(Hediff_MissingPart), "PostAdd"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Hediff_MissingPart_PostAdd_Prefix)));


            harmony.Patch(AccessTools.Method(typeof(GenRecipe), "MakeRecipeProducts"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(GenRecipe_MakeRecipeProducts_Transpiler)));

            harmony.Patch(AccessTools.Method(MakeRecipeProductsInnerType, "MoveNext"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(GenRecipe_MakeRecipeProducts_ImplicitMoveNext_Transpiler)));


            harmony.Patch(AccessTools.Method(typeof(JobDriver_ManTurret), "MakeNewToils"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobDriver_ManTurret_MakeNewToils_Transpiler)));

            harmony.Patch(AccessTools.Method(MakeNewToilsInnerType, "MoveNext"),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobDriver_ManTurret_MakeNewToils_ImplicitMoveNext_Transpiler)));

            harmony.Patch(MakeNewToils_InitAction,
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobDriver_ManTurret_MakeNewToils_ImplicitInitAction_Transpiler)));


            harmony.Patch(AccessTools.Method(typeof(CompChangeableProjectile), "RemoveShell"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompChangeableProjectile_RemoveShell_Postfix)));


            harmony.Patch(AccessTools.Method(typeof(Projectile), "Launch", new Type[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(Thing), typeof(ThingDef) }),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Projectile_Launch_Prefix)));

            Log.Message("[Beheading] Harmony patch succeeded.");
        }

        private static FieldInfo fieldThingGraphicInt = AccessTools.Field(typeof(Thing), "graphicInt");

        private static bool Hediff_MissingPart_PostAdd_Prefix(ref DamageInfo dinfo, Pawn ___pawn, Hediff_MissingPart __instance)
        {
            if (!___pawn.RaceProps.Humanlike)
            {
                return true;
            }

            if (Current.ProgramState != ProgramState.Playing || PawnGenerator.IsBeingGenerated(___pawn))
            {
                return true;
            }

            if (___pawn.RaceProps.FleshType != FleshTypeDefOf.Normal)
            {
                return true;
            }
            
            if (dinfo.Def == null)
            {
                return true;
            }

            /*
            if (dinfo.Def.defName != DamageDefOf.SurgicalCut.defName && dinfo.Def.defName != DamageDefOf.Cut.defName)
            {
                return true;
            }
            */

            if (__instance.Part.def == BodyPartDefOf.Head)
            {
                BeheadedHead head = (BeheadedHead)ThingMaker.MakeThing(ThingDefGenerator_BeheadedHead.GetGeneratedDef(___pawn.def));
                head.Init(___pawn);

                GenPlace.TryPlaceThing(head, ___pawn.PositionHeld, ___pawn.MapHeld, ThingPlaceMode.Near, rot: Rot4.South);
            }

            return true;
        }

        #region GenRecipe.MakeRecipeProducts : implicit method patch
        private static IEnumerable<CodeInstruction> GenRecipe_MakeRecipeProducts_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            bool find = false;
            foreach (var inst in codeInstructions)
            {
                if (inst.opcode == OpCodes.Newobj && !find)
                {
                    ConstructorInfo cinfo = inst.operand as ConstructorInfo;
                    MakeRecipeProductsInnerType = cinfo.DeclaringType;
                    find = true;
                }

                yield return inst;
            }
        }

        private static IEnumerable<CodeInstruction> GenRecipe_MakeRecipeProducts_ImplicitMoveNext_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            FieldInfo fieldIngredients = null, fieldRecipeDef = null;
            List<CodeInstruction> instructions = codeInstructions.ToList();
            for (int i = 0; i < instructions.Count; ++i)
            {
                if (fieldIngredients == null)
                {
                    if (instructions[i].opcode == OpCodes.Ldloc_S &&
                        instructions[i + 1].opcode == OpCodes.Ldarg_0 &&
                        instructions[i + 2].opcode == OpCodes.Ldfld)
                    {
                        FieldInfo f = instructions[i + 2].operand as FieldInfo;
                        if (f != null && f.FieldType == typeof(List<Thing>))
                        {
                            fieldIngredients = f;
                        }
                    }
                }

                if (fieldRecipeDef == null)
                {
                    if (instructions[i].opcode == OpCodes.Ldfld)
                    {
                        FieldInfo f = instructions[i].operand as FieldInfo;
                        if (f != null && f.FieldType == typeof(RecipeDef))
                        {
                            fieldRecipeDef = f;
                        }
                    }
                }
            }

            bool patch = false;
            for (int i = 0; i < instructions.Count; ++i)
            {
                if (instructions[i].opcode == OpCodes.Call && (instructions[i].operand as MethodInfo) == AccessTools.Method(typeof(GenRecipe), "PostProcessProduct"))
                {
                    List<CodeInstruction> injections = new List<CodeInstruction>();

                    injections.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    injections.Add(new CodeInstruction(OpCodes.Ldfld, fieldRecipeDef));
                    injections.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    injections.Add(new CodeInstruction(OpCodes.Ldfld, fieldIngredients));
                    injections.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));
                    injections.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(GenRecipe_MakeRecipeProducts_Injection))));
                    instructions.InsertRange(i + 2, injections);
                    patch = true;
                    break;
                }
            }

            if (!patch)
            {
                Log.Warning("[Beheading] Try to patch GenRecipe_MakeRecipeProducts, but there is something problem.");
            }

            return instructions;
        }

        private static void GenRecipe_MakeRecipeProducts_Injection(RecipeDef recipeDef, List<Thing> ingredients, Thing thing)
        {
            if (!ingredients.Any(x => x is BeheadedHead))
            {
                return;
            }

            BeheadedHead head = ingredients.Where(x => x is BeheadedHead).First() as BeheadedHead;
            Graphic_Head headGraphic = thing.Graphic as Graphic_Head;

            if (headGraphic != null)
            {
                headGraphic.CopyFrom((Graphic_Head)head.Graphic);
            }
        }
        #endregion

        #region JobDriver_ManTurret : implicit method patch
        private static IEnumerable<CodeInstruction> JobDriver_ManTurret_MakeNewToils_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            bool find = false;
            foreach (var inst in codeInstructions)
            {
                if (inst.opcode == OpCodes.Newobj && !find)
                {
                    ConstructorInfo cinfo = inst.operand as ConstructorInfo;
                    MakeNewToilsInnerType = cinfo.DeclaringType;
                    find = true;
                }

                yield return inst;
            }
        }

        private static IEnumerable<CodeInstruction> JobDriver_ManTurret_MakeNewToils_ImplicitMoveNext_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            int k = 0;
            List<CodeInstruction> instructions = codeInstructions.ToList();
            for (int i = 0; i < instructions.Count; ++i)
            {
                if (instructions[i].opcode == OpCodes.Stfld && (instructions[i].operand as FieldInfo) == AccessTools.Field(typeof(Toil), "initAction"))
                {
                    k++;
                    if (k == 2)
                    {
                        MakeNewToils_InitAction = instructions[i - 2].operand as MethodInfo;
                        break;
                    }
                }
            }

            return instructions;
        }

        private static IEnumerable<CodeInstruction> JobDriver_ManTurret_MakeNewToils_ImplicitInitAction_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = codeInstructions.ToList();

            for (int i = 0; i < instructions.Count; ++i)
            {
                if (instructions[i].opcode == OpCodes.Callvirt &&
                    (instructions[i].operand as MethodInfo) == AccessTools.Method(typeof(CompChangeableProjectile), "LoadShell"))
                {
                    List<CodeInstruction> injections = new List<CodeInstruction>();
                    injections.Add(new CodeInstruction(OpCodes.Ldloc_1));
                    injections.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Building_TurretGun), "gun")));
                    injections.Add(new CodeInstruction(OpCodes.Ldloc_0));
                    injections.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), "JobDriver_ManTurret_MakeNewToils_ImplicitInitAction_Injection")));

                    instructions.InsertRange(i + 1, injections);
                    break;
                }
            }

            return instructions;
        }

        private static void JobDriver_ManTurret_MakeNewToils_ImplicitInitAction_Injection(Building_TurretGun gun, Pawn actor)
        {
            Graphic_Head headGraphic = actor.CurJob.targetB.Thing.Graphic as Graphic_Head;
            if (headGraphic != null)
            {
                CompHeadGraphicContainer comp = gun.TryGetComp<CompHeadGraphicContainer>();
                if (comp != null)
                {
                    comp.Push(headGraphic);
                }
            }
        }
        #endregion

        private static void CompChangeableProjectile_RemoveShell_Postfix(CompChangeableProjectile __instance, Thing __result)
        {
            Graphic_Head headGraphic = __result.Graphic as Graphic_Head;
            if (headGraphic != null)
            {
                var compHeadContainer = __instance.parent.TryGetComp<CompHeadGraphicContainer>();
                if (compHeadContainer != null)
                {
                    headGraphic.CopyFrom(compHeadContainer.Pop());
                }
            }
        }

        private static bool Projectile_Launch_Prefix(Thing __instance, Thing launcher, Thing equipment)
        {
            Graphic_Head headGraphic = __instance.Graphic as Graphic_Head;
            if (headGraphic != null && equipment != null)
            {
                Building_TurretGun turret = equipment as Building_TurretGun;
                if (turret != null)
                {
                    CompHeadGraphicContainer comp = turret.gun.TryGetComp<CompHeadGraphicContainer>();
                    if (comp != null && comp.CurrentHeadGraphic != null)
                    {
                        headGraphic.CopyFrom(comp.Pop());
                    }
                }
            }

            return true;
        }
    }
}
