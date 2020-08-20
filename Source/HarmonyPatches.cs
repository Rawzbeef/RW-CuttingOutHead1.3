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

            Log.Message("[Beheading] Harmony patches are succeeded.");
        }

        private static FieldInfo fieldThingGraphicInt = AccessTools.Field(typeof(Thing), "graphicInt");

        private static bool Hediff_MissingPart_PostAdd_Prefix(ref DamageInfo dinfo, Pawn ___pawn, Hediff_MissingPart __instance)
        {
            if (Current.ProgramState != ProgramState.Playing || PawnGenerator.IsBeingGenerated(___pawn))
            {
                return true;
            }

            if (!___pawn.RaceProps.Humanlike)
            {
                CustomLogger.Dev("Generate head thing fail, not humanlike : {0} {1}", ___pawn.Name, ___pawn.def.defName);
                return true;
            }
            
            if (dinfo.Def == null || dinfo.Def == DamageDefOf.Bomb)
            {
                CustomLogger.Dev("Generate head thing: {0} {1}, dinfo not adapt", ___pawn.Name, ___pawn.def.defName);
                return true;
            }

            if (__instance.Part.def == BodyPartDefOf.Head)
            {
                HumanlikeHead head = (HumanlikeHead)ThingMaker.MakeThing(ThingDefGenerator_BeheadedHead.GetGeneratedDef(___pawn.def));
                head.Init(___pawn);
                
                CustomLogger.Dev("Generate head thing: {0} {1}", ___pawn.Name, ___pawn.def.defName);
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
                CustomLogger.NeedCheck("Try to patch GenRecipe_MakeRecipeProducts, but there is something problem.");
            }

            return instructions;
        }

        private static void GenRecipe_MakeRecipeProducts_Injection(RecipeDef recipeDef, List<Thing> ingredients, Thing thing)
        {
            IHeadDataContainer dest = thing as IHeadDataContainer;
            if (dest == null)
            {
                return;
            }

            IHeadDataContainer src = ingredients.First(x => x is IHeadDataContainer) as IHeadDataContainer;
            if (src == null)
            {
                return;
            }

            dest.SetHeadData(src.GetInnerHeadData());
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
            var src = actor.CurJob.targetB.Thing as IHeadDataContainer;
            if (src != null)
            {
                CompHeadDataContainer comp = gun.TryGetComp<CompHeadDataContainer>();
                if (comp != null)
                {
                    comp.SetHeadData(src.GetInnerHeadData());
                }
            }
        }
        #endregion

        private static void CompChangeableProjectile_RemoveShell_Postfix(CompChangeableProjectile __instance, Thing __result)
        {
            IHeadDataContainer dest = __result as IHeadDataContainer;
            if (dest != null)
            {
                var compHeadContainer = __instance.parent.TryGetComp<CompHeadDataContainer>();
                if (compHeadContainer != null)
                {
                    dest.SetHeadData(compHeadContainer.GetInnerHeadData());
                    compHeadContainer.SetHeadData(null);
                }
            }
        }

        private static bool Projectile_Launch_Prefix(Thing __instance, Thing launcher, Thing equipment)
        {
            IHeadDataContainer dest = __instance as IHeadDataContainer;

            if (dest != null && equipment != null)
            {
                Building_TurretGun turret = equipment as Building_TurretGun;
                if (turret != null)
                {
                    CompHeadDataContainer comp = turret.gun.TryGetComp<CompHeadDataContainer>();
                    if (comp != null && comp.GetInnerHeadData() != null)
                    {
                        dest.SetHeadData(comp.GetInnerHeadData());
                        comp.SetHeadData(null);
                    }
                }
            }

            return true;
        }
    }
}
