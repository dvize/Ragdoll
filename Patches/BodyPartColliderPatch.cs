using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EFT;
using HarmonyLib;
using RootMotion.FinalIK;
using UnityEngine;

namespace RAG
{
    [HarmonyPatch(typeof(BodyPartCollider), "ApplyHit")]
    internal class ApplyHitPatch
    {
        private static readonly Dictionary<Player, PlayerRagdollComponents> playerComponents = new Dictionary<Player, PlayerRagdollComponents>();
        private static readonly Dictionary<Player, Coroutine> playerCoroutines = new Dictionary<Player, Coroutine>();
        private static readonly Dictionary<Player, Collider[]> playerOriginalColliders = new Dictionary<Player, Collider[]>();
        private static readonly Dictionary<Player, IK[]> playerOriginalIK = new Dictionary<Player, IK[]>();

        private const float DAMAGE_THRESHOLD = 30f;
        static void Postfix(BodyPartCollider __instance, DamageInfo damageInfo, GStruct357 shotID, ref GClass1560 __result)
        {
            if (__instance.Player.IsYourPlayer || damageInfo.Damage <= DAMAGE_THRESHOLD || damageInfo.DamageType != EDamageType.Bullet)
            {
                return;
            }

            RagComponent.Logger.LogDebug($"Ragdoll Effect on: {__instance.Player.AIData.Player.name}, id: {__instance.Player.AIData.Player.Id}");

            EnableRagdoll(__instance, __instance.Player.AIData.Player, damageInfo);
        }

        private static IEnumerator MonitorRagdollSettling(Player player)
        {
            yield return new WaitForSeconds(2f);
            DisableRagdoll(player);
            playerCoroutines.Remove(player);
        }
        private static void EnableRagdoll(BodyPartCollider bodypartCollider, Player player, DamageInfo damageInfo)
        {
            CacheRagdollComponents(player); // Ensure components are cached

            SetupIKandColliders(player, true);
            player.MovementContext.PlayerAnimatorEnableFallingDown(true);
            disableInGameRigidBodies(player);
            //ToggleAnimators(player, false);
            //ToggleRenderers(player, true);

            var components = playerComponents[player];
            float hitforce = EFTHardSettings.Instance.HIT_FORCE;
            hitforce *= 0.3f + 0.7f * Mathf.InverseLerp(50f, 20f, damageInfo.PenetrationPower);

            components.Ragdoll.ApplyImpulse(damageInfo.HitCollider, damageInfo.Direction, damageInfo.HitPoint, hitforce);

            if (playerCoroutines.TryGetValue(player, out var coroutine))
            {
                player.StopCoroutine(coroutine);
            }
            coroutine = player.StartCoroutine(MonitorRagdollSettling(player));
            playerCoroutines[player] = coroutine;
        }

        private static void SetupIKandColliders(Player player, bool value)
        {
            var components = playerComponents[player];

            //store the original components just in case
            if (!playerOriginalColliders.ContainsKey(player))
            {
                playerOriginalColliders[player] = components.Colliders;
            }

            if (!playerOriginalIK.ContainsKey(player))
            {
                playerOriginalIK[player] = components.IKComponents;
            }

            if (value)
            {
                foreach (var collider in components.Colliders)
                {
                    collider.enabled = true;
                }
                foreach (var ik in components.IKComponents)
                {
                    ik.enabled = false;
                }

                return;
            }
            else
            {
                foreach (var collider in components.Colliders)
                {
                    collider.enabled = playerOriginalColliders[player].Contains(collider);
                }

                foreach (var ik in components.IKComponents)
                {
                    ik.enabled = playerOriginalIK[player].Contains(ik);
                }
            }
        }

        internal static void DisableRagdoll(Player player)
        {
            if (player.AIData.BotOwner.IsDead)
            {
                return;
            }

            enableInGameRigidBodies(player);
            //ToggleAnimators(player, true);

            //move player to playermesh position and set to prone
            //player.Transform.position = player.PlayerBody.transform.position;

            //player.MovementContext.PlayerAnimatorEnableFallingDown(true);
            player.MovementContext.IsInPronePose = true;

        }
        private static void CacheRagdollComponents(Player player)
        {
            if (!playerComponents.ContainsKey(player))
            {
                var components = new PlayerRagdollComponents(player);
                playerComponents[player] = components;
            }
        }

        private static void ToggleRenderers(Player player, bool value)
        {
            if (value)
            {
                Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    //RagComponent.Logger.LogWarning("Setting Renderer True: " + renderer.name + " on " + player.AIData.Player.name + " with id: " + player.AIData.Player.Id);
                    renderer.enabled = true;
                }
            }
            else
            {
                Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    //RagComponent.Logger.LogWarning("Setting Renderer False: " + renderer.name + " on " + player.AIData.Player.name + " with id: " + player.AIData.Player.Id);
                    renderer.enabled = false;
                }
            }
        }

        private static void ToggleAnimators(Player player, bool value)
        {
            if (!value)
            {
                foreach (var animator in player._animators)
                {
                    //RagComponent.Logger.LogWarning("Setting Animator False: " + animator.name + " on " + player.AIData.Player.name + " with id: " + player.AIData.Player.Id);
                    animator.enabled = false;
                }
            }
            else
            {
                foreach (var animator in player._animators)
                {
                    //RagComponent.Logger.LogWarning("Setting Animator True: " + animator.name + " on " + player.AIData.Player.name + " with id: " + player.AIData.Player.Id);
                    animator.enabled = true;
                }
            }
        }

        private static void disableInGameRigidBodies(Player player)
        {
            SetRigidBodyState(player, true);
        }
        private static void enableInGameRigidBodies(Player player)
        {
            SetRigidBodyState(player, false);
        }

        private static void SetRigidBodyState(Player player, bool value)
        {
            if (playerComponents[player].RigidbodySpawners != null)
            {
                foreach (var spawner in playerComponents[player].RigidbodySpawners)
                {
                    var rigidbody = Traverse.Create(spawner).Field("rigidbody_0").GetValue<Rigidbody>();

                    if (value)
                    {
                        spawner.Rigidbody.velocity = Vector3.zero;
                        spawner.Rigidbody.angularVelocity = Vector3.zero;
                    }

                    rigidbody.isKinematic = value;
                }
            }
        }
    }


}
