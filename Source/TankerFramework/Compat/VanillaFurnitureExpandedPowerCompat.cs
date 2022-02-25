using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Verse;

namespace TankerFramework.Compat
{
    [HotSwappable]
    public static class VanillaFurnitureExpandedPowerCompat
    {
        public static bool IsActive { get; private set; }

        private static Type compGasType;
        private static FastInvokeHandler gasNetGetter;

        private static FastInvokeHandler storeGasMethod;
        private static FastInvokeHandler drawGasMethod;
        private static AccessTools.FieldRef<object, IList> storagesField;

        private static AccessTools.FieldRef<object, float> storedField;

        private static AccessTools.FieldRef<object, int> capacityField;

        public static void Init()
        {
            if (IsActive) return;

            var type = compGasType = AccessTools.TypeByName("GasNetwork.CompGas");
            gasNetGetter = MethodInvoker.GetHandler(AccessTools.PropertyGetter(type, "Network"));

            type = AccessTools.TypeByName("GasNetwork.GasNet");
            storeGasMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "Store"));
            drawGasMethod = MethodInvoker.GetHandler(AccessTools.Method(type, "Draw"));
            storagesField = AccessTools.FieldRefAccess<IList>(type, "storages");

            storedField = AccessTools.FieldRefAccess<float>(AccessTools.TypeByName("GasNetwork.CompGasStorage"), "_stored");

            capacityField = AccessTools.FieldRefAccess<int>(AccessTools.TypeByName("GasNetwork.CompProperties_GasStorage"), "capacity");

            IsActive = true;
        }

        public static void HandleTick(CompTankerBase tanker, TankType type)
        {
            if (IsActive)
                HandleTickPrivate(tanker, type);
        }

        private static void HandleTickPrivate(CompTankerBase tanker, TankType type)
        {
            var compPipe = tanker.parent.GetComp(compGasType);
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
                num = Math.Min(num, storagesField(gasNetGetter(compPipe)).Cast<ThingComp>().Sum(s => capacityField(s.props) - storedField(s)));
                num = Math.Max(num, 0);
                if (num > 0)
                {
                    storedAmount -= num;
                    storeGasMethod(gasNetGetter(compPipe), (float)num);
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
                num = Math.Min(num, storagesField(gasNetGetter(compPipe)).Cast<ThingComp>().Sum(s => storedField(s)));
                num = Math.Max(num, 0);
                if (num > 0)
                {
                    drawGasMethod(gasNetGetter(compPipe), (float)num);
                    tanker.SetStoredAmount(type, tanker.GetStoredAmount(type) + num);
                }
            }
        }
    }
}
