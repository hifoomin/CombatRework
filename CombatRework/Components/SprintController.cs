using RoR2;
using UnityEngine;

namespace CombatRework.Components
{
    public class SprintController : MonoBehaviour
    {
        public float sprintSpeedMult;
        public float sprintSpeedMin = Main.sprintSpeedMin.Value;
        public float sprintSpeedMax = Main.sprintSpeedMax.Value;
        public float sprintSpeedDurationToMax = Main.sprintSpeedDurationToMax.Value;
        public float sprintTimer;
        public float recalcTimer;
        public float recalcInterval = 0.1f;
        public bool shouldIncrementTimer = false;
        public CharacterBody characterBody;

        public void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            sprintSpeedMult = sprintSpeedMin;
        }

        public void FixedUpdate()
        {
            shouldIncrementTimer = characterBody.isSprinting;
            if (shouldIncrementTimer)
            {
                sprintTimer += Time.fixedDeltaTime;
            }
            else
            {
                sprintTimer = 0f;
            }

            recalcTimer += Time.fixedDeltaTime;
            if (recalcTimer >= recalcInterval && sprintSpeedMult != sprintSpeedMax)
            {
                sprintSpeedMin = Main.sprintSpeedMin.Value;
                sprintSpeedMax = Main.sprintSpeedMax.Value;
                characterBody.statsDirty = true;
                recalcTimer = 0f;
            }
        }
    }
}