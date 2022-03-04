using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace TankerFramework.HarmonyPatches
{
    [HarmonyPatch]
    public static class Harmony_CreateSrtsThing
    {
        [UsedImplicitly]
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            var type = AccessTools.TypeByName("SRTS.CompLaunchableSRTS");
            yield return AccessTools.Method(type, "TryLaunch");

            type = AccessTools.TypeByName("SRTS.CompBombFlyer");
            yield return AccessTools.Method(type, "TryLaunchBombRun");
        }

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions, MethodBase parentMethod)
        {
            var target = AccessTools.Method(typeof(Harmony_CreateSrtsThing), nameof(SetupThing));
            var targetIndex = parentMethod.Name == "TryLaunch" ? 10 : 7;

            foreach (var ci in codeInstructions)
            {
                yield return ci;

                if (ci.opcode == OpCodes.Stloc_S && ci.operand is LocalBuilder builder && builder.LocalIndex == targetIndex)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, ci.operand);
                    yield return new CodeInstruction(OpCodes.Call, target);
                }
            }
        }

        private static void SetupThing(ThingComp parent, Thing thing)
        {
            var comps = parent.parent.GetComps<CompTankerBase>()?.ToArray();
            if (comps == null || comps.Length == 0 || thing is not ThingWithComps thingWithComps) return;

            var newComps = thingWithComps.GetComps<CompTankerBase>().ToArray();

            foreach (var comp in comps)
            {
                foreach (var newComp in newComps)
                {
                    if (newComp.TransferFrom(comp))
                        break;
                }
            }
        }
    }
}
