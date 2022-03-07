using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TankerFramework
{
    public abstract class CompTankerBase : ThingComp
    {
        public bool drawOverlay = false;

        public abstract float CapPercent { get; }

        public CompProperties_TankerBase Props => (CompProperties_TankerBase)props;

        public abstract bool? IsDraining(TankType type);
        public abstract void SetDraining(TankType type, bool value);
        public abstract bool? IsFilling(TankType type);
        public abstract void SetFilling(TankType type, bool value);
        public abstract double GetStoredAmount(TankType type);
        public abstract void SetStoredAmount(TankType type, double count);
        public abstract bool CanStoreType(TankType type);
        public abstract IEnumerable<TankType> GetAllStoreableTypes();
        public abstract bool TransferFrom(CompTankerBase other);

        public override void PostDraw()
        {
            base.PostDraw();
            if (!drawOverlay) return;

            var r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + Vector3.up * 0.1f;
            r.size = ModResources.BarSize;
            r.fillPercent = CapPercent;
            r.filledMat = ModResources.BarFilledMat;
            r.unfilledMat = ModResources.BarUnfilledMat;
            r.margin = 0.15f;
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);

            drawOverlay = false;
        }
    }
}
