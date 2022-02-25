using System;
using Verse;

namespace TankerFramework
{
    [HotSwappable]
    public static class ExtensionMethods
    {
        public static ThingComp GetComp(this ThingWithComps thing, Type type)
        {
            if (thing == null || type == null)
                return null;

            for (var index = 0; index < thing.AllComps.Count; index++)
            {
                var thingComp = thing.AllComps[index];
                if (thingComp != null && type.IsInstanceOfType(thingComp))
                    return thingComp;
            }

            return null;
        }

        public static MapComponent GetComp(this Map map, Type type)
        {
            if (map == null || type == null)
                return null;

            for (var index = 0; index < map.components.Count; index++)
            {
                var thingComp = map.components[index];
                if (thingComp != null && type.IsInstanceOfType(thingComp))
                    return thingComp;
            }

            return null;
        }

        public static string NoModIdSuffix(this string modId)
        {
            while (true)
            {
                if (modId.EndsWith("_steam"))
                {
                    modId = modId.Substring(0, modId.Length - "_steam".Length);
                    continue;
                }

                if (modId.EndsWith("_copy"))
                {
                    modId = modId.Substring(0, modId.Length - "_copy".Length);
                    continue;
                }

                return modId;
            }
        }
    }
}
