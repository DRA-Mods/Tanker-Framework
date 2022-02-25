using System;
using HarmonyLib;
using Verse;

namespace TankerFramework.Compat
{
    [HotSwappable]
    public static class RimefellerCompat
    {
        public static bool IsActive { get; private set; }

        private static Type compPipeType;
        private static FastInvokeHandler pipeNetGetter;

        private static FastInvokeHandler pushFuelMethod;
        private static FastInvokeHandler pullFuelMethod;
        private static FastInvokeHandler pushOilMethod;
        private static FastInvokeHandler pullOilMethod;

        private static Type mapComponentType;
        private static AccessTools.FieldRef<object, bool> markTowerForDrawField;

        public static void Init()
        {
            if (IsActive) return;

            var type = compPipeType = AccessTools.TypeByName("Rimefeller.CompPipe");
            pipeNetGetter = MethodInvoker.GetHandler(AccessTools.PropertyGetter(type, "pipeNet"));

            type = AccessTools.TypeByName("Rimefeller.PipelineNet");
            pushFuelMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PushFuel"));
            pullFuelMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PullFuel"));
            pushOilMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PushCrude"));
            pullOilMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "PullOil"));

            type = mapComponentType = AccessTools.TypeByName("Rimefeller.MapComponent_Rimefeller");
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
                    storedAmount += type switch
                    {
                        TankType.Fuel => (float)pushFuelMethod(pipeNetGetter(compPipe), (float)num),
                        TankType.Oil => (double)pushOilMethod(pipeNetGetter(compPipe), num),
                        _ => num,
                    };

                    tanker.SetStoredAmount(type, storedAmount);
                }
            }
            else if (tanker.IsFilling(type) == true)
            {
                var storedAll = tanker.GetStoredAmount(TankType.All);
                if (storedAll >= tanker.Props.storageCap)
                {
                    tanker.SetFilling(type, false);
                    return;
                }

                var num = Math.Min(tanker.Props.storageCap - storedAll, tanker.Props.fillAmount);
                num = Math.Max(num, 0);

                var success = type switch
                {
                    TankType.Fuel => (bool)pullFuelMethod(pipeNetGetter(compPipe), num),
                    TankType.Oil => (bool)pullOilMethod(pipeNetGetter(compPipe), num),
                    _ => false,
                };

                if (success)
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
