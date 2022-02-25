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
    }
}
