using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using TankerFramework;
using Verse;

namespace CompTankerCompat.HarmonyPatches
{
    internal static class Harmony_Rimefeller_DrawOverlay
    {
        private static AccessTools.FieldRef<object, bool> markTowersForDrawField;
        private static AccessTools.FieldRef<object, IList> pipeNetsField;

        private static AccessTools.FieldRef<object, List<ThingWithComps>> pipedThingsField;

        [UsedImplicitly]
        public static MethodInfo TargetMethod()
        {
            var target = AccessTools.TypeByName("Rimefeller.MapComponent_Rimefeller");
            if (target == null) return null;

            markTowersForDrawField = AccessTools.FieldRefAccess<bool>(target, "MarkTowersForDraw");
            pipeNetsField = AccessTools.FieldRefAccess<IList>(target, "PipeNets");

            var type = AccessTools.TypeByName("Rimefeller.PipelineNet");
            pipedThingsField = AccessTools.FieldRefAccess<List<ThingWithComps>>(type, "PipedThings");

            return AccessTools.Method(target, "MapComponentUpdate");
        }

        [UsedImplicitly]
        public static void Prefix(MapComponent __instance)
        {
            if (!markTowersForDrawField(__instance)) return;

            foreach (var pipeNet in pipeNetsField(__instance))
            {
                foreach (var thing in pipedThingsField(pipeNet))
                {
                    var comp = thing.GetComp<CompTankerBase>();
                    if (comp != null) comp.drawOverlay = true;
                }
            }
        }
    }
}
