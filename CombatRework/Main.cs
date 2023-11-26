using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RoR2.Skills;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;

namespace CombatRework
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public const string PluginAuthor = "HIFU";
        public const string PluginName = "AttackSlowdown";
        public const string PluginVersion = "1.0.0";

        public static ConfigEntry<float> slowdownPercent { get; set; }
        public static ConfigEntry<float> slowdownLinger { get; set; }
        public static ConfigEntry<bool> slowdownAgile { get; set; }
        public static ConfigEntry<bool> slowdownScale { get; set; }
        public static ConfigEntry<float> slowdownJumpPercent { get; set; }
        public static ConfigEntry<float> slowdownJumpLinger { get; set; }

        public static ConfigEntry<bool> changeCombatSkills { get; set; }

        public static ConfigEntry<bool> fixHurtBoxes { get; set; }

        public static ConfigEntry<float> sprintSpeedMin { get; set; }
        public static ConfigEntry<float> sprintSpeedMax { get; set; }
        public static ConfigEntry<float> sprintSpeedDurationToMax { get; set; }

        public static ManualLogSource ASLogger;

        public static BuffDef slowdownBuff;
        public static BuffDef slowdownJumpBuff;

        public void Awake()
        {
            ASLogger = base.Logger;

            AddBuffs();
            AddConfig();

            if (changeCombatSkills.Value)
            {
                var engiHarpoon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiHarpoons.asset").WaitForCompletion();
                engiHarpoon.isCombatSkill = true;
            }

            if (fixHurtBoxes.Value)
            {
                var acrid = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBody.prefab").WaitForCompletion();
                var mainHurtBoxAcrid = acrid.transform.GetChild(0).GetChild(2).Find("TempHurtbox").GetComponent<SphereCollider>();
                mainHurtBoxAcrid.transform.localPosition = new Vector3(0f, 7f, 2f);
                mainHurtBoxAcrid.radius = 5.26f * 0.75f;

                var mult = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotBody.prefab").WaitForCompletion();
                var mainHurtMult = mult.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).Find("tempHurtBox").GetComponent<CapsuleCollider>();
                mainHurtMult.radius = 3.74f * 0.8f;
                mainHurtMult.height = 12.04f * 0.8f;

                var rex = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Treebot/TreebotBody.prefab").WaitForCompletion();
                var mainHurtRex = rex.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).Find("TempHurtbox").GetComponent<CapsuleCollider>();
                mainHurtRex.radius = 1.42f * 0.66f;
                mainHurtRex.height = 4.26f * 0.66f;
            }

            Hooks.BodyStart.Init();
            Hooks.Movement.Init();
            Hooks.RecalculateStats.Init();
        }

        public void AddConfig()
        {
            slowdownPercent = Config.Bind("Slowdown", "Slowdown Percent", 0.4f, "Decimal. The percent of slow applied while using a combat skill.");
            slowdownLinger = Config.Bind("Slowdown", "Slowdown Linger", 0.75f, "How long slowdown should linger for after using a combat skill.");
            slowdownScale = Config.Bind("Slowdown", "Slowdown Scale", true, "Should slowdown scale with the main hurtbox size to not feel as shit on survivors with a big hurtbox?");
            slowdownJumpPercent = Config.Bind("Slowdown", "Slowdown Jump Percent", 1f, "Decimal. The percent of jump force slow applied while using a combat skill.");
            slowdownJumpLinger = Config.Bind("Slowdown", "Slowdown Jump Linger", 2f, "How long jump slowdown should linger for after using a combat skill.");
            slowdownAgile = Config.Bind("Slowdown", "Slowdown Agile", false, "Should slowdown apply to Agile skills?");

            changeCombatSkills = Config.Bind("Changes for Slowdown", "Change Combat Skills", true, "Changes some combat skills to not count as combat skills.");
            fixHurtBoxes = Config.Bind("Changes for Slowdown", "Change Large Hurt Boxes", true, "Changes Acrid's, REX' and MUL-T's hurt boxes to be smaller.");

            sprintSpeedMin = Config.Bind("Changes to Sprinting", "Minimum Sprint Speed Multplier", 0.9f, "Decimal. This is a total multiplier, so the default 1.45x sprint speed is multiplied by this amount.");
            sprintSpeedMax = Config.Bind("Changes to Sprinting", "Maximum Sprint Speed Multplier", 1.1f, "Decimal. This is a total multiplier, so the default 1.45x sprint speed is multiplied by this amount.");
            sprintSpeedDurationToMax = Config.Bind("Changes to Sprinting", "Duration to achieve Maximum Sprint Speed Multiplier", 3f, "");

            ModSettingsManager.AddOption(new StepSliderOption(slowdownPercent, new StepSliderConfig() { min = 0f, max = 10f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(slowdownLinger, new StepSliderConfig() { min = 0f, max = 5f, increment = 0.05f }));
            ModSettingsManager.AddOption(new CheckBoxOption(slowdownScale));
            ModSettingsManager.AddOption(new StepSliderOption(slowdownJumpPercent, new StepSliderConfig() { min = 0f, max = 1f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(slowdownJumpLinger, new StepSliderConfig() { min = 0f, max = 5f, increment = 0.05f }));
            ModSettingsManager.AddOption(new CheckBoxOption(slowdownAgile));

            ModSettingsManager.AddOption(new StepSliderOption(sprintSpeedMin, new StepSliderConfig() { min = 0f, max = 2f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(sprintSpeedMax, new StepSliderConfig() { min = 0f, max = 2f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(sprintSpeedDurationToMax, new StepSliderConfig() { min = 0.1f, max = 10f, increment = 0.1f }));
        }

        public void DefaultValues()
        {
        }

        public void InfernoValues()
        {
        }

        public void AddBuffs()
        {
            slowdownBuff = ScriptableObject.CreateInstance<BuffDef>();
            slowdownBuff.isCooldown = false;
            slowdownBuff.isDebuff = true;
            slowdownBuff.isHidden = false;
            slowdownBuff.canStack = false;
            slowdownBuff.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdSlow50.asset").WaitForCompletion().iconSprite;
            slowdownBuff.buffColor = new Color32(174, 53, 56, 255);
            slowdownBuff.name = "Attack Slowdown";

            ContentAddition.AddBuffDef(slowdownBuff);

            slowdownJumpBuff = ScriptableObject.CreateInstance<BuffDef>();
            slowdownJumpBuff.isCooldown = false;
            slowdownJumpBuff.isDebuff = true;
            slowdownJumpBuff.isHidden = false;
            slowdownJumpBuff.canStack = false;
            slowdownJumpBuff.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdSlow50.asset").WaitForCompletion().iconSprite;
            slowdownJumpBuff.buffColor = new Color32(174, 53, 104, 255);
            slowdownJumpBuff.name = "Jump Slowdown";

            ContentAddition.AddBuffDef(slowdownJumpBuff);
        }
    }
}