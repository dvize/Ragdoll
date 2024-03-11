using System;
using System.Reflection;
using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using HarmonyLib;

namespace RAG
{
    [BepInPlugin("com.dvize.Ragdoll", "dvize.Ragdoll", "1.0.0")]
    //[BepInDependency("com.spt-aki.core", "3.7.4")]
    public class RagPlugin : BaseUnityPlugin
    {
        private static Harmony _harmony;
        internal void Awake()
        {
            _harmony = new Harmony("com.dvize.Ragdoll");
            _harmony.PatchAll(typeof(ApplyHitPatch));

            new NewGamePatch().Enable();
        }

        
    }

    //re-initializes each new game
    internal class NewGamePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));

        [PatchPrefix]
        public static void PatchPrefix()
        {
            RagComponent.Enable();
        }
    }
}

