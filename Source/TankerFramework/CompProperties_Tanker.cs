using System.Collections.Generic;
using Verse;

namespace TankerFramework
{
    public class CompProperties_Tanker : CompProperties_TankerBase
    {
        public TankType contents = TankType.Invalid;

        public CompProperties_Tanker() => compClass = typeof(CompTanker);

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef)) 
                yield return configError;

            if (contents is <= TankType.Invalid or >= TankType.All)
                yield return $"{contents} is of illegal type: {contents}";
        }
    }
}
