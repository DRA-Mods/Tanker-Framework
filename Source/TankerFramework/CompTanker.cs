using System;
using System.Collections.Generic;
using System.Text;
using Multiplayer.API;
using TankerFramework.Compat;
using UnityEngine;
using Verse;

namespace TankerFramework
{
    [HotSwappable]
    public class CompTanker : CompTankerBase
    {
        // Gizmos
        private Command_Action gizmoDebugFill;
        private Command_Action gizmoDebugEmpty;
        private Command_Toggle gizmoToggleDrain;
        private Command_Toggle gizmoToggleFill;

        // Exposed fields
        public double storedAmount = 0;
        public bool isDraining = false;
        public bool isFilling = false;

        public override float CapPercent => (float)(storedAmount / Props.storageCap);
        public new CompProperties_Tanker Props => (CompProperties_Tanker)props;

        #region Abstract implementation
        public override bool? IsDraining(TankType type) => isDraining;
        public override void SetDraining(TankType type, bool value) => isDraining = value;
        public override bool? IsFilling(TankType type) => isFilling;
        public override void SetFilling(TankType type, bool value) => isFilling = value;
        public override double GetStoredAmount(TankType type) => storedAmount;
        public override double SetStoredAmount(TankType type, double count) => storedAmount = count;
        #endregion

        private Command_Action GizmoDebugFill => gizmoDebugFill ??= new Command_Action
        {
            action = DebugFill,
            defaultLabel = "Dev: Fill",
        };
        private Command_Action GizmoDebugEmpty => gizmoDebugEmpty ??= new Command_Action
        {
            action = DebugEmpty,
            defaultLabel = "Dev: Empty",
        };
        private Command_Toggle GizmoToggleDrain => gizmoToggleDrain ??= new Command_Toggle
        {
            isActive = () => isDraining,
            toggleAction = ToggleDrain,
            defaultLabel = "TankerFrameworkToggleDrain".Translate(),
            defaultDesc = "TankerFrameworkToggleDrainDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get(Props.drainGizmoPath),
        };
        private Command_Toggle GizmoToggleFill => gizmoToggleFill ??= new Command_Toggle
        {
            isActive = () => isFilling,
            toggleAction = ToggleFill,
            defaultLabel = "TankerFrameworkToggleFill".Translate(),
            defaultDesc = "TankerFrameworkToggleFillDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get(Props.fillGizmoPath),
        };

        [SyncMethod]
        internal void ToggleFill()
        {
            isDraining = false;
            isFilling = !isFilling;
        }

        [SyncMethod]
        internal void ToggleDrain()
        {
            isFilling = false;
            isDraining = !isDraining;
        }

        [SyncMethod(debugOnly = true)]
        internal void DebugFill() => storedAmount = Props.storageCap;

        [SyncMethod(debugOnly = true)]
        internal void DebugEmpty() => storedAmount = 0;

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            switch (Props.contents)
            {
                case TankType.Fuel:
                case TankType.Oil:
                    RimefellerCompat.MarkForDrawing(parent.Map);
                    break;
                case TankType.Water:
                    BadHygieneCompat.MarkForDrawing(parent.Map);
                    break;
                case TankType.Helixien:
                    break;
                case TankType.Invalid:
                default:
                    throw new ArgumentOutOfRangeException(nameof(Props.contents), Props.contents, "Invalid tanker contents");
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            switch (Props.contents)
            {
                case TankType.Fuel:
                case TankType.Oil:
                    RimefellerCompat.HandleTick(this, Props.contents);
                    break;
                case TankType.Water:
                    BadHygieneCompat.HandleTick(this, Props.contents);
                    break;
                case TankType.Helixien:
                    VanillaFurnitureExpandedPowerCompat.HandleTick(this, Props.contents);
                    break;
                case TankType.Invalid:
                case TankType.All:
                default:
                    throw new ArgumentOutOfRangeException(nameof(Props.contents), Props.contents, "Invalid tanker contents");
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            yield return GizmoToggleDrain;
            yield return GizmoToggleFill;

            if (Prefs.DevMode)
            {
                yield return GizmoDebugEmpty;
                yield return GizmoDebugFill;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref storedAmount, nameof(storedAmount), 0);
            Scribe_Values.Look(ref isDraining, nameof(isDraining), false);
            Scribe_Values.Look(ref isFilling, nameof(isFilling), false);
        }

        public override string CompInspectStringExtra()
        {
            if (!parent.Spawned) return string.Empty;

            var stringBuilder = new StringBuilder();

            var text = (Props.contents switch
            {
                TankType.Fuel => "TankerFuelStorage",
                TankType.Oil => "TankerOilStorage",
                TankType.Water => "TankerWaterStorage",
                TankType.Helixien => "TankerHelixienStorage",
                TankType.Invalid or TankType.All or _ => throw new ArgumentOutOfRangeException(nameof(Props.contents), Props.contents, "Invalid tanker contents"),
            }).Translate();

            stringBuilder.Append(text);
            stringBuilder.Append(' ');
            stringBuilder.Append(storedAmount.ToString("0.0"));
            stringBuilder.Append('/');
            stringBuilder.Append(Props.storageCap);
            stringBuilder.AppendLine();

            if (isFilling)
                stringBuilder.AppendLine("TankerFrameworkFillingInspect".Translate());
            else if (isDraining)
                stringBuilder.AppendLine("TankerFrameworkDrainingInspect".Translate());

            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
