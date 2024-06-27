using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using RAG;
using UnityEngine;

namespace dvize.Ragdoll
{
    internal class ApplyImpulsePlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyCorpseImpulse));
        }

        [PatchPrefix]

        public static bool Prefix(Player __instance, float ____corpseAppliedForce, Corpse ___Corpse, DamageInfo ___LastDamageInfo)
        {
            //float num = EFTHardSettings.Instance.HIT_FORCE;  //base value is 150f

            /*RagdollMod.Logger.LogError("Penetration Power: " + ___LastDamageInfo.PenetrationPower);
            num *= Mathf.InverseLerp(RagPlugin.MaxAppliedForce.Value, RagPlugin.MinAppliedForce.Value, ___LastDamageInfo.PenetrationPower);*/

            //get value 0-1 based on penetration power of ammo and then apply to max appliedforce value

#if DEBUG
            RagPlugin.Logger.LogError("Penetration Power: " + ___LastDamageInfo.PenetrationPower);
#endif
            float num = Mathf.InverseLerp(5f, 40f, ___LastDamageInfo.PenetrationPower) * RagPlugin.MaxAppliedForce.Value;

            ____corpseAppliedForce = num;

#if DEBUG
            RagPlugin.Logger.LogError("Applied Force: " + num);
#endif
            ___Corpse.Ragdoll.ApplyImpulse(___LastDamageInfo.HitCollider, ___LastDamageInfo.Direction, ___LastDamageInfo.HitPoint, num);



            //skip the original method
            return false;
        }
    }
}

