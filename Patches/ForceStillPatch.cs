using System;
using System.Reflection;
using System.Threading.Tasks;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.NextObservedPlayer;
using HarmonyLib;
using RAG;
using Systems.Effects;
using UnityEngine;
using static EFT.Player.Class1058;

namespace dvize.Ragdoll
{
    internal class ForceStillPatch : ModulePatch
    {
        private static int timeDelay;
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass814), nameof(GClass814.method_6));
        }

        [PatchPrefix]
        internal static bool Prefix(GClass814 __instance, GClass2226 deathCommand, ObservedPlayerView ___observedPlayerView_0, ObservedCorpse ___observedCorpse_0)
        {
            // Run the partial original method
            RunOriginalMethod(__instance, deathCommand, ___observedPlayerView_0, ___observedCorpse_0);

            // Delay the ForceStill call if the corpse is visible
            bool flag = ___observedPlayerView_0.ObservedPlayerController.Culling.IsVisible;
            if (flag)
            {
                DelayedForceStill(___observedCorpse_0);
            }

            // Skip the original method
            return false;
        }

        private static async void DelayedForceStill(ObservedCorpse observedCorpse_0)
        {
            timeDelay = RagPlugin.RagdollTimeInSec.Value * 1000;
            await Task.Delay(timeDelay);
            observedCorpse_0.ForceStill();
        }

        private static void RunOriginalMethod(GClass814 instance, GClass2226 deathCommand, ObservedPlayerView observedPlayerView_0, ObservedCorpse observedCorpse_0)
        {
            if (Singleton<Effects>.Instantiated)
            {
                Singleton<Effects>.Instance.EffectsCommutator.StopBleedingForPlayer(observedPlayerView_0);
            }
            instance.IsAlive = false;
            observedPlayerView_0.BodyAnimator.enabled = false;
            Collider[] colliders = observedPlayerView_0.Colliders;
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = true;
            }
            observedPlayerView_0.ApplyDeath();
            if (Singleton<GameWorld>.Instantiated)
            {
                Singleton<GameWorld>.Instance.UnregisterPlayer(observedPlayerView_0);
            }
            bool flag = !observedPlayerView_0.ObservedPlayerController.Culling.IsVisible;
            observedCorpse_0 = instance.method_7(deathCommand.CorpseImpulse.OverallVelocity, flag);
            observedCorpse_0.SetSpecificSettings(observedPlayerView_0.PlayerBones.RightPalm);
            Singleton<GameWorld>.Instance.ObservedPlayersCorpes.Add(observedPlayerView_0.ObservedPlayerController.EquipmentViewController.Equipment.Id.GetHashCode(), observedCorpse_0);
            if (!flag)
            {
                instance.method_11(deathCommand.CorpseImpulse.BodyPartColliderType, deathCommand.CorpseImpulse.Direction, deathCommand.CorpseImpulse.Point, deathCommand.CorpseImpulse.Force);
            }
            if (observedPlayerView_0.BundleAnimationBones.StationaryWeapon != null)
            {
                observedPlayerView_0.BundleAnimationBones.ApplyDeath();
                observedPlayerView_0.ObservedPlayerController.HandsController.ResetAnimatorOnUnspawnHands();
                observedPlayerView_0.ObservedPlayerController.HandsController.DestroyHandsController();
                instance.method_10();
            }
            else
            {
                observedCorpse_0.SetItemInHandsLootedCallback(new Action(instance.method_10));
                instance.method_8(observedPlayerView_0.ObservedPlayerController.HandsController.ItemInHands, observedPlayerView_0.ObservedPlayerController.HandsController.CurrentWeaponPrefab.gameObject, deathCommand.CorpseImpulse.OverallVelocity);
            }
        }
    }
}

