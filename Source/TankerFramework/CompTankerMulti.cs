using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using TankerFramework.Compat;
using UnityEngine;
using Verse;

namespace TankerFramework
{
    [HotSwappable]
    public class CompTankerMulti : CompTankerBase
    {
        // Exposed fields
        public Dictionary<TankType, double> storedAmount;
        public Dictionary<TankType, bool> isDraining;
        public Dictionary<TankType, bool> isFilling;

        public override float CapPercent => (float)(storedAmount.Sum(x => x.Value) / Props.storageCap);
        public new CompProperties_TankerMulti Props => (CompProperties_TankerMulti)props;

        #region Abstract implementation
        public override bool? IsDraining(TankType type)
        {
            if (!parent.Spawned) return null;
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
            if (!parent.Spawned) return;
            if (type != TankType.All) isDraining[type] = value;
            else foreach (var tankType in Props.tankTypes) isDraining[tankType] = value;
        }

        public override bool? IsFilling(TankType type)
        {
            if (!parent.Spawned) return null;
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
            if (!parent.Spawned) return;
            if (type != TankType.All) isFilling[type] = value;
            else foreach (var tankType in Props.tankTypes) isFilling[tankType] = value;
        }

        public override double GetStoredAmount(TankType type)
        {
            if (!parent.Spawned) return 0;
            if (type != TankType.All) return storedAmount[type];
            return storedAmount.Values.Sum();
        }

        public override void SetStoredAmount(TankType type, double count)
        {
            if (!parent.Spawned) return;
            storedAmount[type] = count;
        }

        public override bool TransferFrom(CompTankerBase other)
        {
            if (other is not CompTankerMulti tanker)
                return false;
            if (Props.tankTypes.Count != tanker.Props.tankTypes.Count)
                return false;

            for (var i = 0; i < Props.tankTypes.Count; i++)
            {
                if (Props.tankTypes[i] != tanker.Props.tankTypes[i])
                    return false;
            }

            storedAmount = tanker.storedAmount;
            return true;
        }
        #endregion

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!CompatManager.AnyActive || Props.tankTypes == null || !Enumerable.Any(Props.tankTypes))
            {
                parent.AllComps.Remove(this);
                return;
            }

            if (storedAmount.NullOrEmpty())
                storedAmount = Props.tankTypes.ToDictionary(x => x, _ => 0d);
            if (isDraining.NullOrEmpty())
                isDraining = Props.tankTypes.ToDictionary(x => x, _ => false);
            if (isFilling.NullOrEmpty())
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

        public override string CompInspectStringExtra()
        {
            if (!parent.Spawned || Props.tankTypes.NullOrEmpty()) return string.Empty;

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("TankerFrameworkTotalStorage".Translate(storedAmount.Values.Sum().ToString("0.0"), Props.storageCap));

            foreach (var type in Props.tankTypes)
            {
                stringBuilder.Append(CompatManager.GetTranslatedTankName(type));
                stringBuilder.Append(' ');
                stringBuilder.Append(storedAmount[type].ToString("0.0"));

                if (isFilling[type])
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append("TankerFrameworkFillingInspect".Translate());
                    stringBuilder.Append(')');
                }
                else if (isDraining[type])
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append("TankerFrameworkDrainingInspect".Translate());
                    stringBuilder.Append(')');
                }

                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }

        #region Gizmos
        private Command_ActionRightClick gizmoDebugFill;
        private Command_ActionRightClick gizmoDebugEmpty;
        private Command_ToggleRightClick gizmoToggleDrain;
        private Command_ToggleRightClick gizmoToggleFill;

        private Command_ActionRightClick GizmoDebugFill => gizmoDebugFill ??= new Command_ActionRightClick
        {
            openOnLeftClick = true,
            rightClickFloatMenuOptions = storedAmount.Keys.Select(x =>
                new FloatMenuOption($"Fill {x}", () => DebugFill(x))).ToList(),
            defaultLabel = "Dev: Fill",
        };
        private Command_ActionRightClick GizmoDebugEmpty => gizmoDebugEmpty ??= new Command_ActionRightClick
        {
            action = () => DebugEmpty(TankType.All),
            rightClickFloatMenuOptions = storedAmount.Keys.Select(x =>
                new FloatMenuOption($"Empty {x}", () => DebugEmpty(x))).ToList(),
            defaultLabel = "Dev: Empty",
        };
        private Command_ToggleRightClick GizmoToggleDrain => gizmoToggleDrain ??= new Command_ToggleRightClick
        {
            isActive = () =>
            {
                var draining = isDraining.Count(x => x.Value);
                if (draining == 0)
                    return false;
                if (draining == isDraining.Count)
                    return true;
                return null;
            },
            toggleAction = () => ToggleDrain(TankType.All),
            rightClickFloatMenuOptions = Props.tankTypes.Select(x =>
                new FloatMenuOption("TankerFrameworkToggleSpecificDrain".Translate($"TankerFramework{x}".Translate()), () => ToggleDrain(x))).ToList(),
            defaultLabel = "TankerFrameworkToggleDrain".Translate(),
            defaultDesc = "TankerFrameworkToggleDrainDesc".Translate(),
            icon = ContentFinder<Texture2D>.Get(Props.drainGizmoPath),
        };
        private Command_ToggleRightClick GizmoToggleFill => gizmoToggleFill ??= new Command_ToggleRightClick
        {
            isActive = () =>
            {
                var filling = isFilling.Count(x => x.Value);
                if (filling == 0)
                    return false;
                if (filling == isFilling.Count)
                    return true;
                return null;
            },
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
                foreach (var tankerType in Props.tankTypes)
                    storedAmount[tankerType] = 0;
            }
            else
                storedAmount[type] = 0;
        }

        [SyncMethod]
        protected void ToggleDrain(TankType type)
        {
            if (type == TankType.All)
            {
                var target = !isDraining.Any(x => x.Value);
                foreach (var tankType in Props.tankTypes)
                {
                    isDraining[tankType] = target;
                    isFilling[tankType] = false;
                }
            }
            else
            {
                isDraining[type] ^= true;
                isFilling[type] = false;
            }
        }

        [SyncMethod]
        protected void ToggleFill(TankType type)
        {
            if (type == TankType.All)
            {
                var target = !isFilling.Any(x => x.Value);
                foreach (var tankType in Props.tankTypes)
                {
                    isFilling[tankType] = target;
                    isDraining[tankType] = false;
                }
            }
            else
            {
                isFilling[type] ^= true;
                isDraining[type] = false;
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
        #endregion
    }
}
