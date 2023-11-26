using RoR2;

namespace CombatRework.Hooks
{
    public static class BodyStart
    {
        public static void Init()
        {
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        public static void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (!body.isPlayerControlled)
            {
                return;
            }

            var slowdownController = body.GetComponent<Components.SlowdownController>();
            var sprintController = body.GetComponent<Components.SprintController>();
            if (!slowdownController)
            {
                body.gameObject.AddComponent<Components.SlowdownController>();
            }

            if (!sprintController)
            {
                body.gameObject.AddComponent<Components.SprintController>();
            }
        }
    }
}