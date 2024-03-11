using System;
using System.Collections.Generic;
using EFT;
using EFT.AssetsManager;
using RootMotion.FinalIK;
using UnityEngine;

namespace RAG
{
    internal class PlayerRagdollComponents
    {

        private const CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        private const float maxDepenetrationVelocity = 0.5f;

        public CharacterJointSpawner[] JointSpawners;
        public RigidbodySpawner[] RigidbodySpawners;
        public List<PlayerRigidbodySleepHierarchy> SleepHierarchyList;
        public RagdollClass Ragdoll;
        public Collider[] Colliders;
        public IK[] IKComponents;
        public static float lastBulletDamageTimes;
        public PlayerRagdollComponents(Player player)
        {
            try
            {
                JointSpawners = player.gameObject.GetComponentsInChildren<CharacterJointSpawner>();
                RigidbodySpawners = player.gameObject.GetComponentsInChildren<RigidbodySpawner>();
                SleepHierarchyList = PlayerPoolObject.CreatePlayerRigidbodySleepHierarchy(RigidbodySpawners);
                Colliders = player.GetComponentsInChildren<Collider>();
                IKComponents = player.GetComponentsInChildren<IK>();
                Ragdoll = new RagdollClass(RigidbodySpawners, JointSpawners, SleepHierarchyList, player.Velocity, maxDepenetrationVelocity, collisionDetectionMode,
                    null, null, player.PlayerBody, null, null, true, false);
            }
            catch (Exception e)
            {
                RagComponent.Logger.LogError(e);
            }
        }
    }
}
