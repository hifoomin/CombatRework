using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CombatRework.Hooks
{
    public class CharacterMaster
    {
        public static void Init()
        {
            RoR2.CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
        }

        private static void CharacterMaster_onStartGlobal(RoR2.CharacterMaster master)
        {
            if (!Main.enemyAIChanges.Value)
            {
                return;
            }

            if (master.teamIndex == TeamIndex.Monster || master.teamIndex == TeamIndex.Void)
            {
                // Main.InfernoLogger.LogError("master " + master.name + " IS on team monster or void");

                var body = master.GetBody();
                if (!body)
                {
                    // Main.InfernoLogger.LogError("couldnt find body of " + master.name);
                    return;
                }

                if (body.isChampion)
                {
                    master.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
                }

                if (body.baseJumpPower < 20f)
                {
                    body.baseJumpPower = 20f;
                }

                var BaseAI = master.GetComponent<BaseAI>();
                if (!BaseAI)
                {
                    // Main.InfernoLogger.LogError("couldnt find baseai of " + master.name);
                    return;
                }

                if (master.name != "GolemMaster(Clone)" && master.name != "ClayBruiserMaster(Clone)")
                {
                    BaseAI.fullVision = true;
                    BaseAI.aimVectorDampTime = 0.031f;
                    BaseAI.aimVectorMaxSpeed = 250f;
                    BaseAI.enemyAttentionDuration = 1.5f;
                }
                else
                {
                    BaseAI.fullVision = true;
                    BaseAI.aimVectorDampTime = 0.09f;
                    BaseAI.aimVectorMaxSpeed = 250f;
                    BaseAI.enemyAttentionDuration = 1.5f;
                }

                switch (master.name)
                {
                    default:
                        break;

                    case "BeetleQueenMaster(Clone)":

                        AISkillDriver BeetleQueenChase2 = (from x in master.GetComponents<AISkillDriver>()
                                                           where x.customName == "Chase"
                                                           select x).First();

                        BeetleQueenChase2.minDistance = 0f;

                        AISkillDriver BeetleQueenFireFuckwards = (from x in master.GetComponents<AISkillDriver>()
                                                                  where x.customName == "SpawnWards"
                                                                  select x).First();

                        BeetleQueenFireFuckwards.maxDistance = 100f;

                        BeetleQueenFireFuckwards.maxUserHealthFraction = Mathf.Infinity;
                        break;

                    case "GravekeeperMaster(Clone)":

                        AISkillDriver GrovetenderRunAndShoot = (from x in master.GetComponents<AISkillDriver>()
                                                                where x.customName == "RunAndShoot"
                                                                select x).First();

                        GrovetenderRunAndShoot.movementType = AISkillDriver.MovementType.StrafeMovetarget;

                        AISkillDriver GrovetenderHook = (from x in master.GetComponents<AISkillDriver>()
                                                         where x.customName == "Hooks"
                                                         select x).First();

                        GrovetenderHook.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        GrovetenderHook.maxUserHealthFraction = Mathf.Infinity;
                        GrovetenderHook.minDistance = 13f;

                        AISkillDriver GrovetenderFuckAround = (from x in master.GetComponents<AISkillDriver>()
                                                               where x.customName == "WaitAroundUntilSkillIsBack"
                                                               select x).First();

                        GrovetenderFuckAround.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        break;

                    case "ImpBossMaster(Clone)":

                        AISkillDriver ImpOverlordGroundPound = (from x in master.GetComponents<AISkillDriver>()
                                                                where x.customName == "GroundPound"
                                                                select x).First();

                        ImpOverlordGroundPound.maxDistance = 12f;

                        AISkillDriver ImpOverlordSpike = (from x in master.GetComponents<AISkillDriver>()
                                                          where x.customName == "FireVoidspikesWhenInRange"
                                                          select x).First();

                        ImpOverlordSpike.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        ImpOverlordSpike.minDistance = 16f;

                        AISkillDriver ImpOverlordTeleport = (from x in master.GetComponents<AISkillDriver>()
                                                             where x.customName == "BlinkToTarget"
                                                             select x).First();

                        ImpOverlordTeleport.minDistance = 33f;
                        break;

                    case "BrotherMaster(Clone)":

                        AISkillDriver MithrixFireShards = (from x in master.GetComponents<AISkillDriver>()
                                                           where x.customName == "Sprint and FireLunarShards"
                                                           select x).First();

                        MithrixFireShards.minDistance = 0f;
                        MithrixFireShards.maxUserHealthFraction = Mathf.Infinity;

                        AISkillDriver MithrixSprint = (from x in master.GetComponents<AISkillDriver>()
                                                       where x.customName == "Sprint After Target"
                                                       select x).First();

                        MithrixSprint.minDistance = 40f;

                        AISkillDriver DashStrafe = (from x in master.GetComponents<AISkillDriver>()
                                                    where x.customName == "DashStrafe"
                                                    select x).First();

                        DashStrafe.nextHighPriorityOverride = MithrixFireShards;

                        break;

                    case "BrotherHurtMaster(Clone)":

                        AISkillDriver MithrixWeakSlam = (from x in master.GetComponents<AISkillDriver>()
                                                         where x.customName == "SlamGround"
                                                         select x).First();
                        MithrixWeakSlam.maxUserHealthFraction = Mathf.Infinity;
                        MithrixWeakSlam.movementType = AISkillDriver.MovementType.StrafeMovetarget;

                        AISkillDriver MithrixWeakShards = (from x in master.GetComponents<AISkillDriver>()
                                                           where x.customName == "Shoot"
                                                           select x).First();
                        MithrixWeakShards.movementType = AISkillDriver.MovementType.StrafeMovetarget;

                        break;

                    case "TitanMaster(Clone)":
                        AISkillDriver StoneTitanLaser = (from x in master.GetComponents<AISkillDriver>()
                                                         where x.skillSlot == SkillSlot.Special
                                                         select x).First();
                        StoneTitanLaser.maxUserHealthFraction = Mathf.Infinity;
                        StoneTitanLaser.minDistance = 16f;
                        StoneTitanLaser.movementType = AISkillDriver.MovementType.StrafeMovetarget;

                        AISkillDriver StoneTitanRockTurret = (from x in master.GetComponents<AISkillDriver>()
                                                              where x.skillSlot == SkillSlot.Utility
                                                              select x).First();
                        StoneTitanRockTurret.maxUserHealthFraction = 0.8f;
                        StoneTitanRockTurret.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        break;

                    case "MinorConstructMaster(Clone)":
                        AISkillDriver AlphaConstructFire = (from x in master.GetComponents<AISkillDriver>()
                                                            where x.customName == "Shooty"
                                                            select x).First();
                        AlphaConstructFire.maxDistance = 100f;
                        break;

                    case "BeetleGuardMaster(Clone)":
                        AISkillDriver BeetleGuardFireSunder = (from x in master.GetComponents<AISkillDriver>()
                                                               where x.customName == "FireSunder"
                                                               select x).First();
                        BeetleGuardFireSunder.maxDistance = 100f;

                        //master.inventory.GiveItem(RoR2Content.Items.SecondarySkillMagazine, 2);
                        break;

                    case "FlyingVerminMaster(Clone)":
                        AISkillDriver BlindPestSpit = (from x in master.GetComponents<AISkillDriver>()
                                                       where x.skillSlot == SkillSlot.Primary
                                                       select x).First();
                        BlindPestSpit.minDistance = 10f;
                        BlindPestSpit.maxDistance = 40f;
                        break;

                    case "ClayBruiserMaster(Clone)":
                        AISkillDriver ClayTemplarShoot = (from x in master.GetComponents<AISkillDriver>()
                                                          where x.customName == "WalkAndShoot"
                                                          select x).First();
                        ClayTemplarShoot.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        break;

                    case "LemurianBruiserMaster(Clone)":
                        AISkillDriver ElderLemurianStop = (from x in master.GetComponents<AISkillDriver>()
                                                           where x.customName == "StopAndShoot"
                                                           select x).First();
                        ElderLemurianStop.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                        break;

                    case "GreaterWispMaster(Clone)":
                        AISkillDriver GreaterWispShoot = (from x in master.GetComponents<AISkillDriver>()
                                                          where x.minDistance == 15f
                                                          select x).First();
                        GreaterWispShoot.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        GreaterWispShoot.maxDistance = 100f;
                        break;

                    case "ImpMaster(Clone)":
                        AISkillDriver ImpSlash = (from x in master.GetComponents<AISkillDriver>()
                                                  where x.customName == "Slash"
                                                  select x).First();
                        ImpSlash.maxDistance = 12f;
                        break;

                    case "LemurianMaster(Clone)":
                        AISkillDriver LemurianBite = (from x in master.GetComponents<AISkillDriver>()
                                                      where x.customName == "ChaseAndBiteOffNodegraph"
                                                      select x).First();
                        LemurianBite.maxDistance = 8f;

                        AISkillDriver LemurianBiteSlow = (from x in master.GetComponents<AISkillDriver>()
                                                          where x.customName == "ChaseAndBiteOffNodegraphWhileSlowingDown"
                                                          select x).First();
                        LemurianBiteSlow.maxDistance = 0f;

                        AISkillDriver LemurianShoot = (from x in master.GetComponents<AISkillDriver>()
                                                       where x.customName == "StrafeAndShoot"
                                                       select x).First();
                        LemurianShoot.minDistance = 10f;
                        LemurianShoot.maxDistance = 100f;

                        AISkillDriver LemurianStrafe = (from x in master.GetComponents<AISkillDriver>()
                                                        where x.customName == "StrafeIdley"
                                                        select x).First();

                        LemurianStrafe.minDistance = 10f;
                        LemurianStrafe.maxDistance = 100f;
                        break;

                    case "WispMaster(Clone)":
                        AISkillDriver LesserWispSomething = (from x in master.GetComponents<AISkillDriver>()
                                                             where x.minDistance == 0
                                                             select x).First();
                        LesserWispSomething.maxDistance = 10f;

                        AISkillDriver LesserWispSomething2 = (from x in master.GetComponents<AISkillDriver>()
                                                              where x.maxDistance == 30
                                                              select x).First();
                        LesserWispSomething2.minDistance = 10f;
                        break;

                    case "LunarExploderMaster(Clone)":
                        AISkillDriver LunarExploderShoot = (from x in master.GetComponents<AISkillDriver>()
                                                            where x.customName == "StrafeAndShoot"
                                                            select x).First();
                        LunarExploderShoot.maxDistance = 100f;
                        AISkillDriver LunarExploderSprintShoot = (from x in master.GetComponents<AISkillDriver>()
                                                                  where x.customName == "SprintNodegraphAndShoot"
                                                                  select x).First();
                        LunarExploderSprintShoot.maxDistance = 100f;

                        break;

                    case "GolemMaster(Clone)":
                        AISkillDriver StoneGolemShootLaser = (from x in master.GetComponents<AISkillDriver>()
                                                              where x.skillSlot == SkillSlot.Secondary
                                                              select x).First();
                        StoneGolemShootLaser.selectionRequiresAimTarget = true;
                        StoneGolemShootLaser.activationRequiresAimTargetLoS = true;
                        StoneGolemShootLaser.activationRequiresAimConfirmation = true;
                        StoneGolemShootLaser.maxDistance = 100f;
                        StoneGolemShootLaser.minDistance = 0f;
                        break;

                    case "NullifierMaster(Clone)":
                        AISkillDriver VoidReaverPanicFire = (from x in master.GetComponents<AISkillDriver>()
                                                             where x.customName == "PanicFireWhenClose"
                                                             select x).First();
                        VoidReaverPanicFire.movementType = AISkillDriver.MovementType.ChaseMoveTarget;

                        AISkillDriver VoidReaverTrack = (from x in master.GetComponents<AISkillDriver>()
                                                         where x.customName == "FireAndStrafe"
                                                         select x).First();
                        VoidReaverTrack.movementType = AISkillDriver.MovementType.ChaseMoveTarget;

                        AISkillDriver VoidReaverStop = (from x in master.GetComponents<AISkillDriver>()
                                                        where x.customName == "FireAndChase"
                                                        select x).First();
                        VoidReaverStop.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                        break;

                    case "MegaConstructMaster(Clone)":
                        AISkillDriver XiConstructLazer = (from x in master.GetComponents<AISkillDriver>()
                                                          where x.skillSlot == SkillSlot.Primary
                                                          select x).First();
                        XiConstructLazer.maxDistance = 60f;
                        XiConstructLazer.minDistance = 0f;
                        AISkillDriver XiConstructShield = (from x in master.GetComponents<AISkillDriver>()
                                                           where x.skillSlot == SkillSlot.Utility
                                                           select x).First();
                        XiConstructShield.maxDistance = 60f;
                        XiConstructShield.minDistance = 0f;
                        AISkillDriver XiConstructSummon = (from x in master.GetComponents<AISkillDriver>()
                                                           where x.skillSlot == SkillSlot.Special
                                                           select x).First();
                        XiConstructSummon.maxDistance = 60f;
                        XiConstructSummon.minDistance = 0f;
                        AISkillDriver XiConstructStrafeStep = (from x in master.GetComponents<AISkillDriver>()
                                                               where x.customName == "StrafeStep"
                                                               select x).First();
                        XiConstructSummon.maxDistance = 60f;
                        XiConstructSummon.minDistance = 0f;
                        break;

                    case "GupMaster(Clone)":
                        AISkillDriver GupSpike = (from x in master.GetComponents<AISkillDriver>()
                                                  where x.customName == "Spike"
                                                  select x).First();
                        GupSpike.maxDistance = 14f;
                        break;

                    case "ClayGrenadierMaster(Clone)":
                        AISkillDriver ClayApothecaryFaceslam = (from x in master.GetComponents<AISkillDriver>()
                                                                where x.customName == "FaceSlam"
                                                                select x).First();
                        ClayApothecaryFaceslam.maxDistance = 35f;
                        break;

                    case "ParentMaster(Clone)":
                        AISkillDriver ParentTeleport = (from x in master.GetComponents<AISkillDriver>()
                                                        where x.customName == "Teleport"
                                                        select x).First();
                        ParentTeleport.maxUserHealthFraction = 1f;

                        break;

                    case "LunarWispMaster(Clone)":
                        AISkillDriver LunarWispBackUp = (from x in master.GetComponents<AISkillDriver>()
                                                         where x.customName == "Back Up"
                                                         select x).First();
                        LunarWispBackUp.maxDistance = 13f;
                        AISkillDriver LunarWispChase = (from x in master.GetComponents<AISkillDriver>()
                                                        where x.customName == "Chase"
                                                        select x).First();
                        LunarWispChase.minDistance = 25f;
                        break;

                    case "MiniMushroomMaster(Clone)":
                        AISkillDriver SporeGrenade = (from x in master.GetComponents<AISkillDriver>()
                                                      where x.customName == "Spore Grenade"
                                                      select x).First();
                        SporeGrenade.maxDistance = 60f;
                        AISkillDriver MushrumPath = (from x in master.GetComponents<AISkillDriver>()
                                                     where x.customName == "Path"
                                                     select x).First();
                        MushrumPath.shouldSprint = true;
                        AISkillDriver PathStrafe = (from x in master.GetComponents<AISkillDriver>()
                                                    where x.customName == "PathStrafe"
                                                    select x).First();
                        PathStrafe.shouldSprint = true;
                        break;

                    case "VagrantMaster(Clone)":
                        AISkillDriver VagrantChase = (from x in master.GetComponents<AISkillDriver>()
                                                      where x.customName == "Chase"
                                                      select x).First();
                        VagrantChase.minDistance = 25f;
                        break;

                    case "TitanGoldMaster(Clone)":
                        AISkillDriver AurelioniteLaser = (from x in master.GetComponents<AISkillDriver>()
                                                          where x.skillSlot == SkillSlot.Special
                                                          select x).First();
                        AurelioniteLaser.maxUserHealthFraction = Mathf.Infinity;
                        AurelioniteLaser.minDistance = 16f;
                        AurelioniteLaser.movementType = AISkillDriver.MovementType.StrafeMovetarget;

                        AISkillDriver AurelioniteTurret = (from x in master.GetComponents<AISkillDriver>()
                                                           where x.skillSlot == SkillSlot.Utility
                                                           select x).First();
                        AurelioniteTurret.maxUserHealthFraction = 0.8f;
                        AurelioniteTurret.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        break;

                    case "RoboBallBossMaster(Clone)":
                        AISkillDriver EnableEyebeam = (from x in master.GetComponents<AISkillDriver>()
                                                       where x.skillSlot == SkillSlot.Special
                                                       select x).First();
                        EnableEyebeam.maxUserHealthFraction = 0.5f;
                        break;

                    case "ScavMaster(Clone)":
                        AISkillDriver Sit = (from x in master.GetComponents<AISkillDriver>()
                                             where x.customName == "Sit"
                                             select x).First();
                        Sit.maxUserHealthFraction = 0.75f;
                        AISkillDriver FireCannon = (from x in master.GetComponents<AISkillDriver>()
                                                    where x.customName == "FireCannon"
                                                    select x).First();
                        FireCannon.maxDistance = 100f;
                        AISkillDriver ThrowSack = (from x in master.GetComponents<AISkillDriver>()
                                                   where x.customName == "ThrowSack"
                                                   select x).First();
                        ThrowSack.maxDistance = 100f;
                        AISkillDriver UseEquipmentAndFireCannon = (from x in master.GetComponents<AISkillDriver>()
                                                                   where x.customName == "UseEquipmentAndFireCannon"
                                                                   select x).First();
                        UseEquipmentAndFireCannon.maxDistance = 100f;
                        break;

                    case "VoidBarnacleMaster(Clone)":
                        AISkillDriver Shooty = (from x in master.GetComponents<AISkillDriver>()
                                                where x.customName == "Shooty"
                                                select x).First();
                        Shooty.maxDistance = 100f;
                        break;

                    case "SuperRoboBallBossMaster(Clone)":
                        AISkillDriver FireAndStop = (from x in master.GetComponents<AISkillDriver>()
                                                     where x.customName == "FireAndStop"
                                                     select x).First();
                        FireAndStop.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                        break;
                }
            }
        }
    }
}