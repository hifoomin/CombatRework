using UnityEngine.Networking;
using UnityEngine;
using RoR2;

namespace CombatRework.Components
{
    public class SlowdownController : MonoBehaviour
    {
        public CharacterBody characterBody;
        public HurtBox mainHurtBox;
        public BoxCollider boxCollider;
        public SphereCollider sphereCollider;
        public CapsuleCollider capsuleCollider;

        public bool isUsingNonAgileSkill = false;
        public bool shouldApplyBuff = false;
        public bool shouldApplyJumpBuff = false;

        public void Start()
        {
            characterBody = GetComponent<CharacterBody>();

            mainHurtBox = characterBody.mainHurtBox;
            boxCollider = mainHurtBox.GetComponent<BoxCollider>();
            if (!boxCollider)
                sphereCollider = mainHurtBox.GetComponent<SphereCollider>();
            if (!sphereCollider)
                capsuleCollider = mainHurtBox.GetComponent<CapsuleCollider>();

            characterBody.onSkillActivatedServer += CharacterBody_onSkillActivatedServer;
        }

        private void CharacterBody_onSkillActivatedServer(GenericSkill skill)
        {
            if (skill.skillDef.cancelSprintingOnActivation)
            {
                isUsingNonAgileSkill = true;
            }
            else
            {
                isUsingNonAgileSkill = false;
            }
        }

        public void SetSlowdown(bool shouldApplySlow, bool shouldApplyJump)
        {
            if (shouldApplySlow)
            {
                if (NetworkServer.active)
                {
                    if (!characterBody.HasBuff(Main.slowdownBuff))
                    {
                        characterBody.AddBuff(Main.slowdownBuff);
                    }
                }
            }
            else
            {
                if (NetworkServer.active)
                {
                    if (characterBody.HasBuff(Main.slowdownBuff))
                    {
                        characterBody.RemoveBuff(Main.slowdownBuff);
                    }
                }
            }

            if (shouldApplyJumpBuff)
            {
                if (NetworkServer.active)
                {
                    if (!characterBody.HasBuff(Main.slowdownJumpBuff))
                    {
                        characterBody.AddBuff(Main.slowdownJumpBuff);
                    }
                }
            }
            else
            {
                if (NetworkServer.active)
                {
                    if (characterBody.HasBuff(Main.slowdownJumpBuff))
                    {
                        characterBody.RemoveBuff(Main.slowdownJumpBuff);
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            shouldApplyBuff = characterBody.outOfCombatStopwatch <= Main.slowdownLinger.Value && (Main.slowdownAgile.Value || isUsingNonAgileSkill);
            shouldApplyJumpBuff = characterBody.outOfCombatStopwatch <= Main.slowdownJumpLinger.Value && (Main.slowdownAgile.Value || isUsingNonAgileSkill);
            SetSlowdown(shouldApplyBuff, shouldApplyJumpBuff);
        }

        public void OnDisable()
        {
            SetSlowdown(false, false);
        }

        public void OnDestroy()
        {
            SetSlowdown(false, false);
            characterBody.onSkillActivatedServer -= CharacterBody_onSkillActivatedServer;
        }
    }
}