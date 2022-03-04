using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TankerFramework
{
    public class Command_ToggleRightClick : Command_ActionRightClick
    {
        public Func<bool?> isActive;

        public Action toggleAction;

        public override SoundDef CurActivateSound
        {
            get
            {
                if (isActive() == true)
                    return SoundDefOf.Checkbox_TurnedOff;
                return SoundDefOf.Checkbox_TurnedOn;
            }
        }

        public override void ProcessInput(Event ev)
        {
            if (!openOnLeftClick || rightClickFloatMenuOptions.Count <= 1)
                toggleAction();
            else
                OpenMenu();

            CurActivateSound?.PlayOneShotOnCamera();
        }

        public override GizmoResult GizmoOnGUI(Vector2 loc, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(loc, maxWidth, parms);
            var rect = new Rect(loc.x, loc.y, this.GetWidth(maxWidth), 75f);
            var position = new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f);
            var image = isActive() switch
            {
                true => Widgets.CheckboxOnTex,
                false => Widgets.CheckboxOffTex,
                null => Widgets.CheckboxPartialTex,
            };
            GUI.DrawTexture(position, image);
            return result;
        }

        public override bool InheritInteractionsFrom(Gizmo other) => false;
    }
}
