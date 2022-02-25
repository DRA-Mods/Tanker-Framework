using System.Collections.Generic;
using Verse;

namespace TankerFramework
{
    public abstract class CompProperties_TankerBase : CompProperties
    {
        public double storageCap = 10000;
        public double fillAmount = 0.5;
        public double drainAmount = 0.5;

        public string fillGizmoPath = "Things/UI/Fill";
        public string drainGizmoPath = "Things/UI/Drain";

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef)) 
                yield return configError;

            if (storageCap == 0)
                yield return $"{storageCap} cannot be 0";
            else if (storageCap < 0)
                storageCap = -storageCap;

            if (fillAmount == 0)
                yield return $"{fillAmount} cannot be 0";
            else if (fillAmount < 0)
                fillAmount = -fillAmount;

            if (drainAmount == 0)
                yield return $"{drainAmount} cannot be 0";
            else if (drainAmount < 0)
                drainAmount = -drainAmount;

            if (string.IsNullOrWhiteSpace(fillGizmoPath))
                yield return $"{fillGizmoPath} is empty";

            if (string.IsNullOrWhiteSpace(drainGizmoPath))
                yield return $"{drainGizmoPath} is empty";
        }
    }
}
