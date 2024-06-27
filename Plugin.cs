using System.Reflection;
using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using dvize.Ragdoll;
using EFT;

namespace RAG
{
    [BepInPlugin("com.dvize.Ragdoll", "dvize.Ragdoll", "1.0.0")]
    public class RagPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Logger;
        internal static ConfigEntry<int> RagdollTimeInSec;
        internal static ConfigEntry<int> MaxAppliedForce;
        
        internal void Awake()
        {
            Logger = base.Logger;

            RagdollTimeInSec = Config.Bind("Settings", "Ragdoll Time", 15, "Time that ragdoll is played before corpse is replaced");
            MaxAppliedForce = Config.Bind("Settings", "Maximum Force Applied", 150, "Maximum force applied");

            new ForceStillPatch().Enable();
            new ApplyImpulsePlayerPatch().Enable();

            //EFTHardSettings.Instance.DEBUG_CORPSE_PHYSICS = true;
        }
    }

 
}

