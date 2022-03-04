using System;
using Verse;

namespace TankerFramework.Compat
{
    public static class CompatManager
    {
        public static bool AnyActive
            => BadHygieneCompat.IsActive ||
               RimefellerCompat.IsActive ||
               VanillaFurnitureExpandedPowerCompat.IsActive;

        public static bool IsActive(TankType tankType)
        {
            return tankType switch
            {
                TankType.Water => BadHygieneCompat.IsActive,
                TankType.Oil or TankType.Fuel => RimefellerCompat.IsActive,
                TankType.Helixien => VanillaFurnitureExpandedPowerCompat.IsActive,
                _ => true,
            };
        }

        public static TaggedString GetTranslatedTankName(TankType tankType)
        {
            return (tankType switch
            {
                TankType.Fuel => "TankerFrameworkFuelStorage",
                TankType.Oil => "TankerFrameworkOilStorage",
                TankType.Water => "TankerFrameworkWaterStorage",
                TankType.Helixien => "TankerFrameworkHelixienStorage",
                TankType.Invalid or TankType.All or _ => throw new ArgumentOutOfRangeException(nameof(tankType), tankType, "Invalid tanker contents"),
            }).Translate();
        }
    }
}
