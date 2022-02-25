using System;
using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;
using TankerFramework.Compat;
using UnityEngine;
using Verse;

namespace TankerFramework
{
    public class CompTankerMulti : CompTankerBase
    {
        // Exposed fields
        public Dictionary<TankType, double> storedAmount;
        public Dictionary<TankType, bool> isDraining;
        public Dictionary<TankType, bool> isFilling;

        public override float CapPercent => (float)(storedAmount.Sum(x => x.Value) / Props.storageCap);
        public CompProperties_TankerMulti Props => (CompProperties_TankerMulti)props;

        #region Abstract implementation
        public override bool? IsDraining(TankType type)
        {
            if (type != TankType.All) return isDraining[type];

            var count = isDraining.Values.Count(x => x);
            if (count == 0)
                return false;
            if (count == isDraining.Count)
                return true;
            return null;
        }

        public override void SetDraining(TankType type, bool value)
        {
            if (type != TankType.All) isDraining[type] = value;
            foreach (var tankType in Props.tankTypes) isDraining[tankType] = value;
        }

        public override bool? IsFilling(TankType type)
        {
            if (type != TankType.All) return isFilling[type];

            var count = isFilling.Values.Count(x => x);
            if (count == 0)
                return false;
            if (count == isFilling.Count)
                return true;
            return null;
        }

        public override void SetFilling(TankType type, bool value)
        {
            if (type != TankType.All) isFilling[type] = value;
            foreach (var tankType in Props.tankTypes) isFilling[tankType] = value;
        }

        public override double GetStoredAmount(TankType type)
        {
            if (type != TankType.All) return storedAmount[type];
            return storedAmount.Values.Sum();
        }

        public override double SetStoredAmount(TankType type, double count) => storedAmount[type] = count;
        #endregion

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!CompatManager.AnyActive || Props.tankTypes == null || !Enumerable.Any(Props.tankTypes))
            {
                parent.AllComps.Remove(this);
                return;
            }

            storedAmount = Props.tankTypes.ToDictionary(x => x, _ => 0d);
            isDraining = Props.tankTypes.ToDictionary(x => x, _ => false);
            isFilling = Props.tankTypes.ToDictionary(x => x, _ => false);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            foreach (var content in Props.tankTypes)
            {
                switch (content)
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
                        throw new ArgumentOutOfRangeException(nameof(content), content, "Invalid tanker contents");
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            foreach (var content in Props.tankTypes)
            {
                switch (content)
                {
                    case TankType.Fuel:
                    case TankType.Oil:
                        RimefellerCompat.HandleTick(this, content);
                        break;
                    case TankType.Water:
                        BadHygieneCompat.HandleTick(this, content);
                        break;
                    case TankType.Helixien:
                        VanillaFurnitureExpandedPowerCompat.HandleTick(this, content);
                        break;
                    case TankType.Invalid:
                    case TankType.All:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(content), content, "Invalid tanker contents");
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref storedAmount, nameof(storedAmount), LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref isDraining, nameof(isDraining), LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref isFilling, nameof(isFilling), LookMode.Value, LookMode.Value);

            if (storedAmount.NullOrEmpty())
                storedAmount = Props.tankTypes.ToDictionary(x => x, _ => 0d);
            if (isDraining.NullOrEmpty())
                isDraining = Props.tankTypes.ToDictionary(x => x, _ => false);
            if (isFilling.NullOrEmpty())
                isFilling = Props.tankTypes.ToDictionary(x => x, _ => false);
        }

        #region Gizmos
        private Command_ActionRightClick gizmoDebugFill;
        private Command_ActionRightClick gizmoDebugEmpty;
        private Command_ToggleRightClick gizmoToggleDrain;
        private Command_ToggleRightClick gizmoToggleFill;

        private Command_Action GizmoDebugFill => gizmoDebugFill ??= new Command_ActionRightClick
        {
            openOnLeftClick = true,
            rightClickFloatMenuOptions = storedAmount.Keys.Select(x =>
                new FloatMenuOption($"Fill {x}", () => DebugFill(x))).ToList(),
            defaultLabel = "Dev: Fill",
        };
        private Command_Action GizmoDebugEmpty => gizmoDebugEmpty ??= new Command_ActionRightClick
        {
            action = () => DebugEmpty(TankType.All),
            rightClickFloatMenuOptions = storedAmount.Keys.Select(x =>
                new FloatMenuOption($"Empty {x}", () => DebugEmpty(x))).ToList(),
            defaultLabel = "Dev: Empty",
        };
        private Command_ToggleRightClick GizmoToggleDrain => gizmoToggleDrain ??= new Command_ToggleRightClick
        {
            isActive = index => index == -1 ? isDraining.Any() : isDraining[(TankType)index],
            toggleAction = () =>  ToggleDrain(TankType.All),
            rightClickFloatMenuOptions = Props.tankTypes.Select(x =>
                new FloatMenuOption("TankerFrameworkToggleSpecificDrain".Translate($"TankerFramework{x}".Translate()), () => ToggleDrain(x))).ToList(),
            defaultLabel = "TankerFrameworkToggleDrain".Translate(),
            defaultDesc = "TankerFrameworkToggleDrainDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get(Props.drainGizmoPath),
        };
        private Command_ToggleRightClick GizmoToggleFill => gizmoToggleFill ??= new Command_ToggleRightClick
        {
            isActive = index => index == -1 ? isFilling.Any() : isFilling[(TankType)index],
            toggleAction = () => ToggleFill(TankType.All),
            rightClickFloatMenuOptions = Props.tankTypes.Select(x =>
                new FloatMenuOption("TankerFrameworkToggleSpecificFill".Translate($"TankerFramework{x}".Translate()), () => ToggleFill(x))).ToList(),
            defaultLabel = "TankerFrameworkToggleFill".Translate(),
            defaultDesc = "TankerFrameworkToggleFillDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get(Props.fillGizmoPath),
        };

        [SyncMethod(debugOnly = true)]
        protected void DebugFill(TankType type)
        {
            var total = storedAmount.Where(x => x.Key != type).Sum(x => x.Value);
            storedAmount[type] = Props.storageCap - total;
        }

        [SyncMethod(debugOnly = true)]
        protected void DebugEmpty(TankType type)
        {
            if (type == TankType.All)
            {
                foreach (var tankerType in storedAmount.Keys)
                    storedAmount[tankerType] = 0;
            }
            else
                storedAmount[type] = 0;
        }

        protected void ToggleDrain(TankType type)
        {
            if (type == TankType.All)
            {
                bool? target = null;
                foreach (var (key, value) in isDraining)
                {
                    target ??= !value;
                    isDraining[key] = target.Value;
                }
            }
            else isDraining[type] ^= true;
        }

        protected void ToggleFill(TankType type)
        {
            if (type == TankType.All)
            {
                bool? target = null;
                foreach (var (key, value) in isFilling)
                {
                    target ??= !value;
                    isFilling[key] = target.Value;
                }
            }
            else isFilling[type] ^= true;
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
        #endregion
    }
}
