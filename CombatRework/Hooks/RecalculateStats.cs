using R2API;
using RoR2;
using UnityEngine;

namespace CombatRework.Hooks
{
    public static class RecalculateStats
    {
        public static void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        public static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                if (sender.HasBuff(Main.slowdownBuff))
                {
                    var slowdownController = sender.GetComponent<Components.SlowdownController>();
                    if (slowdownController)
                    {
                        float colliderSize = slowdownController.capsuleCollider ? slowdownController.capsuleCollider.radius : (slowdownController.sphereCollider ? slowdownController.sphereCollider.radius : (slowdownController.boxCollider ? slowdownController.boxCollider.size.magnitude : 1f));
                        args.moveSpeedReductionMultAdd += Main.slowdownPercent.Value / Mathf.Sqrt(colliderSize);
                    }
                }
                if (sender.HasBuff(Main.slowdownJumpBuff) && Main.slowdownJumpPercent.Value < 1f)
                {
                    args.jumpPowerMultAdd -= Main.slowdownJumpPercent.Value;
                }
            }
        }
    }
}