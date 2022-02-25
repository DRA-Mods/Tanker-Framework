using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TankerFramework
{
    public class CompProperties_TankerMulti : CompProperties_TankerBase
    {
        public List<TankType> tankTypes;

        public CompProperties_TankerMulti() => compClass = typeof(CompTankerMulti);

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef))
                yield return configError;

            if (tankTypes.NullOrEmpty())
                yield return $"{tankTypes} cannot be empty";
            else if (tankTypes.Count == 1 && tankTypes[0] == TankType.All)
            {
                tankTypes.Clear();
                tankTypes.AddRange(Enumerable.Range(1, (int) (TankType.All - 3)).Select(x => (TankType)x));
            }
            else
            {
                foreach (var contents in tankTypes)
                {
                    if (contents is <= TankType.Invalid or >= TankType.All)
                        yield return $"{tankTypes} contains contents of illegal type: {contents}";
                }

                tankTypes.RemoveAll(contents => contents is <= TankType.Invalid or >= TankType.All);
            }
        }
    }
}
