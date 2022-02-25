namespace TankerFramework.Compat
{
    public static class CompatManager
    {
        public static bool AnyActive
            => BadHygieneCompat.IsActive ||
               RimefellerCompat.IsActive ||
               VanillaFurnitureExpandedPowerCompat.IsActive;
    }
}
