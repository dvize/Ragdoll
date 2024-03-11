using System;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI.Ragfair;
using RootMotion.FinalIK;
using UnityEngine;

namespace RAG
{
    internal class RagComponent : MonoBehaviour
    {
        internal static ManualLogSource Logger;
        internal static GameWorld gameWorld;
        internal static Player player;

        private void Awake()
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(RagComponent));
        }

        private void Start()
        {
            player = gameWorld.MainPlayer;

            EFTHardSettings.Instance.DEBUG_CORPSE_PHYSICS = true;

            Player.OnPlayerDeadStatic += Player_OnPlayerDeadStatic;
        }

        private void Player_OnPlayerDeadStatic(Player player, IPlayer arg2, DamageInfo damageInfo, EBodyPart bodypart)
        {

        }

        private void Update()
        {

        }

        public static void Enable()
        {
            if (Singleton<IBotGame>.Instantiated)
            {
                //add component to gameWorld
                gameWorld = Singleton<GameWorld>.Instance;
                gameWorld.gameObject.AddComponent<RagComponent>();
                Logger.LogDebug("RagComponent Enabled");

            }
        }

        bool IsKeyPressed(KeyboardShortcut key)
        {
            if (!UnityInput.Current.GetKeyDown(key.MainKey))
            {
                return false;
            }
            foreach (var modifier in key.Modifiers)
            {
                if (!UnityInput.Current.GetKey(modifier))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
