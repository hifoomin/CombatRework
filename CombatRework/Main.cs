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
        public const string PluginName = "CombatRework";
        public const string PluginVersion = "1.0.0";

        public static ConfigEntry<bool> important1 { get; set; }
        public static ConfigEntry<bool> important2 { get; set; }
        public static ConfigEntry<bool> important3 { get; set; }
        public static ConfigEntry<bool> important4 { get; set; }
        public static ConfigEntry<bool> important5 { get; set; }
        public static ConfigEntry<bool> important6 { get; set; }

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
        public static ConfigEntry<float> huntressSprintSpeedMin { get; set; }
        public static ConfigEntry<float> huntressSprintSpeedMax { get; set; }
        public static ConfigEntry<float> sprintSpeedDurationToMax { get; set; }

        public static ConfigEntry<float> hyperbolicSpeedCap { get; set; }

        public static ConfigEntry<bool> enemyAIChanges { get; set; }

        public static ManualLogSource ASLogger;

        public static BuffDef slowdownBuff;
        public static BuffDef slowdownJumpBuff;

        public static BodyIndex huntressBodyIndex;

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

            On.RoR2.BodyCatalog.Init += BodyCatalog_Init;

            Hooks.BodyStart.Init();
            Hooks.Movement.Init();
            Hooks.RecalculateStats.Init();

            Hooks.CharacterMaster.Init();
        }

        private void BodyCatalog_Init(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();
            huntressBodyIndex = BodyCatalog.FindBodyIndex("HuntressBody(Clone)");
        }

        public void AddConfig()
        {
            important1 = Config.Bind("_! IMPORTANT !", "Important", true, "All players must have the same config in order for the mod to work properly in multiplayer!");
            important2 = Config.Bind("Slowdown", "Important", true, "All players must have the same config in order for the mod to work properly in multiplayer!");
            important3 = Config.Bind("Changes for Slowdown", "Important", true, "All players must have the same config in order for the mod to work properly in multiplayer!");
            important4 = Config.Bind("Changes to Sprinting", "Important", true, "All players must have the same config in order for the mod to work properly in multiplayer!");
            important5 = Config.Bind("Changes to AI", "Important", true, "All players must have the same config in order for the mod to work properly in multiplayer!");
            important6 = Config.Bind("Changes to AI", "Important", true, "All players must have the same config in order for the mod to work properly in multiplayer!");

            slowdownPercent = Config.Bind("Slowdown", "Slowdown Percent", 1f, "Decimal. The percent of slow applied while using a combat skill.");
            slowdownLinger = Config.Bind("Slowdown", "Slowdown Linger", 0.75f, "How long slowdown should linger for after using a combat skill.");
            slowdownScale = Config.Bind("Slowdown", "Slowdown Scales?", true, "Should slowdown scale with the main hurtbox size to not feel as shit on survivors with a big hurtbox?");
            slowdownJumpPercent = Config.Bind("Slowdown", "Slowdown Jump Percent", 1f, "Decimal. The percent of jump force slow applied while using a combat skill.");
            slowdownJumpLinger = Config.Bind("Slowdown", "Slowdown Jump Linger", 1.25f, "How long jump slowdown should linger for after using a combat skill.");
            slowdownAgile = Config.Bind("Slowdown", "Slowdown Affects Agile?", false, "Should slowdown apply to Agile skills?");

            changeCombatSkills = Config.Bind("Changes for Slowdown", "Change Combat Skills?", true, "Changes some combat skills to not count as combat skills.");
            fixHurtBoxes = Config.Bind("Changes for Slowdown", "Change Large Hurt Boxes?", true, "Changes Acrid's, REX' and MUL-T's hurt boxes to be smaller.");

            sprintSpeedMin = Config.Bind("Changes to Sprinting", "Minimum Sprint Speed Multiplier", 0.8f, "Decimal. This is a total multiplier, so the default 1.45x sprint speed is multiplied by this amount.");
            sprintSpeedMax = Config.Bind("Changes to Sprinting", "Maximum Sprint Speed Multiplier", 1.4f, "Decimal. This is a total multiplier, so the default 1.45x sprint speed is multiplied by this amount.");
            huntressSprintSpeedMin = Config.Bind("Changes to Sprinting", "Huntress Minimum Sprint Speed Multiplier", 0.85f, "Decimal. This is a total multiplier, so the default 1.45x sprint speed is multiplied by this amount.");
            huntressSprintSpeedMax = Config.Bind("Changes to Sprinting", "Huntress Maximum Sprint Speed Multiplier", 1.1f, "Decimal. This is a total multiplier, so the default 1.45x sprint speed is multiplied by this amount.");
            sprintSpeedDurationToMax = Config.Bind("Changes to Sprinting", "Time to achieve Maximum Sprint Speed Multiplier", 5f, "");

            hyperbolicSpeedCap = Config.Bind("Changes to Speed", "Hyperbolic Speed Cap", 35f, "The Maximum Speed in m/s that items, buffs, etc slowly approach, but never reach, and never overcome. Set to a negative value to disable.");

            enemyAIChanges = Config.Bind("Changes to AI", "Enable Enemy AI Changes?", true, "Changes monsters to have longer range for the most part");

            ModSettingsManager.AddOption(new CheckBoxOption(important2));
            ModSettingsManager.AddOption(new CheckBoxOption(important4));

            ModSettingsManager.AddOption(new GenericButtonOption("Default Preset", "Slowdown", "Click me to change your preset to Default. Settings don't update visually until you re-open this menu.", ">    <", DefaultValues));
            ModSettingsManager.AddOption(new GenericButtonOption("Inferno Preset", "Slowdown", "Click me to change your preset to Inferno Settings don't update visually until you re-open this menu.", ">    <", InfernoValues));
            ModSettingsManager.AddOption(new StepSliderOption(slowdownPercent, new StepSliderConfig() { min = 0f, max = 10f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(slowdownLinger, new StepSliderConfig() { min = 0f, max = 5f, increment = 0.05f }));
            ModSettingsManager.AddOption(new CheckBoxOption(slowdownScale));
            ModSettingsManager.AddOption(new StepSliderOption(slowdownJumpPercent, new StepSliderConfig() { min = 0f, max = 1f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(slowdownJumpLinger, new StepSliderConfig() { min = 0f, max = 5f, increment = 0.05f }));
            ModSettingsManager.AddOption(new CheckBoxOption(slowdownAgile));

            ModSettingsManager.AddOption(new StepSliderOption(sprintSpeedMin, new StepSliderConfig() { min = 0f, max = 2f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(sprintSpeedMax, new StepSliderConfig() { min = 0f, max = 2f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(huntressSprintSpeedMin, new StepSliderConfig() { min = 0f, max = 2f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(huntressSprintSpeedMax, new StepSliderConfig() { min = 0f, max = 2f, increment = 0.05f }));
            ModSettingsManager.AddOption(new StepSliderOption(sprintSpeedDurationToMax, new StepSliderConfig() { min = 0.1f, max = 10f, increment = 0.1f }));

            ModSettingsManager.AddOption(new StepSliderOption(hyperbolicSpeedCap, new StepSliderConfig() { min = -5f, max = 50f, increment = 5f }));

            ModSettingsManager.AddOption(new CheckBoxOption(enemyAIChanges));
        }

        public void DefaultValues()
        {
            slowdownPercent.Value = 1f;
            slowdownLinger.Value = 0.75f;
            slowdownScale.Value = true;
            slowdownJumpPercent.Value = 1f;
            slowdownJumpLinger.Value = 1.25f;
            slowdownAgile.Value = false;
            changeCombatSkills.Value = true;
            fixHurtBoxes.Value = true;
            sprintSpeedMin.Value = 0.8f;
            sprintSpeedMax.Value = 1.4f;
            huntressSprintSpeedMin.Value = 0.85f;
            huntressSprintSpeedMax.Value = 1.1f;
            sprintSpeedDurationToMax.Value = 5f;
            hyperbolicSpeedCap.Value = 35f;
            enemyAIChanges.Value = true;
        }

        public void InfernoValues()
        {
            slowdownPercent.Value = 0.35f;
            slowdownLinger.Value = 0.45f;
            slowdownScale.Value = true;
            slowdownJumpPercent.Value = 1f;
            slowdownJumpLinger.Value = 0.8f;
            slowdownAgile.Value = false;
            changeCombatSkills.Value = true;
            fixHurtBoxes.Value = true;
            sprintSpeedMin.Value = 0.8f;
            sprintSpeedMax.Value = 1.4f;
            huntressSprintSpeedMin.Value = 0.85f;
            huntressSprintSpeedMax.Value = 1.1f;
            sprintSpeedDurationToMax.Value = 3f;
            hyperbolicSpeedCap.Value = 35f;
            enemyAIChanges.Value = true;
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