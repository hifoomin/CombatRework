using CombatRework.Components;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace CombatRework.Hooks
{
    public static class Movement
    {
        public static void Init()
        {
            On.RoR2.CharacterMotor.OnLanded += CharacterMotor_OnLanded;
            On.RoR2.CharacterMotor.OnLeaveStableGround += CharacterMotor_OnLeaveStableGround;
            IL.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private static void CharacterBody_RecalculateStats(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdfld<CharacterBody>("sprintingSpeedMultiplier"),
                x => x.MatchMul()))
            {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CharacterBody, float>>((orig, self) =>
                {
                    var sprintController = self.GetComponent<SprintController>();
                    if (sprintController)
                    {
                        var sprintSpeed = Mathf.Lerp(sprintController.sprintSpeedMin, sprintController.sprintSpeedMax, Mathf.Clamp01(Mathf.Min(sprintController.sprintTimer, sprintController.sprintSpeedDurationToMax)));
                        sprintController.sprintSpeedMult = sprintSpeed;
                        orig *= sprintSpeed;
                    }
                    return orig;
                });
            }
            else
            {
                Main.ASLogger.LogError("Failed to apply Sprinting Changes Hook");
            }
        }

        public static void GenericCharacterMain_ProcessJump(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<GenericCharacterMain>("jumpInputReceived")))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool, GenericCharacterMain, bool>>((orig, self) =>
                {
                    if (Main.slowdownJumpPercent.Value >= 1f)
                    {
                        var body = self.characterBody;
                        if (body && body.HasBuff(Main.slowdownJumpBuff))
                        {
                            return false;
                        }
                    }

                    return orig;
                });
            }
            else
            {
                Main.ASLogger.LogError("Failed to apply Jump Hook");
            }

            c.Index = 0;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<EntityState>("get_characterBody"),
                x => x.MatchLdloc(out _)))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, GenericCharacterMain, float>>((orig, self) =>
                {
                    var body = self.characterBody;
                    if (body && body.HasBuff(Main.slowdownJumpBuff))
                    {
                        return 0f;
                    }
                    return orig;
                });
            }
            else
            {
                Main.ASLogger.LogError("Failed to apply Jump Speed Hook");
            }
        }

        public static void CharacterMotor_OnLeaveStableGround(On.RoR2.CharacterMotor.orig_OnLeaveStableGround orig, CharacterMotor self)
        {
            var body = self.body;
            if (body && (body.HasBuff(Main.slowdownBuff) || body.HasBuff(Main.slowdownJumpBuff)))
            {
                self.body.statsDirty = true;
            }
            orig(self);
        }

        public static void CharacterMotor_OnLanded(On.RoR2.CharacterMotor.orig_OnLanded orig, CharacterMotor self)
        {
            var body = self.body;
            if (body && body.HasBuff(Main.slowdownBuff) || body.HasBuff(Main.slowdownJumpBuff))
            {
                self.body.statsDirty = true;
            }
            orig(self);
        }
    }
}