using System;
using HarmonyLib;
using Verse;

namespace TankerFramework.Compat
{
    [HotSwappable]
    public static class BadHygieneCompat
    {
        public static bool IsActive { get; private set; }

        private static Type compPipeType;
        private static FastInvokeHandler pipeNetGetter;

        private static FastInvokeHandler pushWaterMethod;
        private static FastInvokeHandler pullWaterMethod;

        private static Type mapComponentType;
        private static AccessTools.FieldRef<object, bool> markTowerForDrawField;

        public static void Init()
        {
            if (IsActive) return;

            var type = compPipeType = AccessTools.TypeByName("DubsBadHygiene.CompPipe");
            pipeNetGetter = MethodInvoker.GetHandler(AccessTools.PropertyGetter(type, "pipeNet"));

            type = AccessTools.TypeByName("DubsBadHygiene.PlumbingNet");
            pushWaterMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PushWater"));
            pullWaterMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PullWater"));

            type = mapComponentType = AccessTools.TypeByName("DubsBadHygiene.MapComponent_Hygiene");
            markTowerForDrawField = AccessTools.FieldRefAccess<bool>(type, "MarkTowersForDraw");

            IsActive = true;
        }

        public static void HandleTick(CompTankerBase tanker, TankType type)
        {
            if (IsActive)
                HandleTickPrivate(tanker, type);
        }

        private static void HandleTickPrivate(CompTankerBase tanker, TankType type)
        {
            var compPipe = tanker.parent.GetComp(compPipeType);
            if (compPipe == null) return;

            if (tanker.IsDraining(type) == true)
            {
                var storedAmount = tanker.GetStoredAmount(type);
                if (storedAmount <= 0)
                {
                    tanker.SetDraining(type, false);
                    return;
                }

                var num = Math.Min(storedAmount, tanker.Props.drainAmount);
                if (num > 0)
                {
                    storedAmount -= num;
                    storedAmount += (float)pushWaterMethod(pipeNetGetter(compPipe), (float)num);
                    tanker.SetStoredAmount(type, storedAmount);
                }
            }
            else if (tanker.IsFilling(type) == true)
            {
                var storedAmount = tanker.GetStoredAmount(TankType.All);
                if (storedAmount >= tanker.Props.storageCap)
                {
                    tanker.SetFilling(type, false);
                    return;
                }

                var num = Math.Min(tanker.Props.storageCap - storedAmount, tanker.Props.fillAmount);
                num = Math.Max(num, 0);

                if ((bool) pullWaterMethod(pipeNetGetter(compPipe), (float) num, 0))
                    tanker.SetStoredAmount(type, tanker.GetStoredAmount(type) + num);
            }
        }

        public static void MarkForDrawing(Map map)
        {
            var mapComp = map.GetComp(mapComponentType);
            if (mapComp != null)
                markTowerForDrawField(mapComp) = true;
        }
    }
}
