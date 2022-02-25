using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TankerFramework
{
    public class Command_ActionRightClick : Command_Action
    {
        public bool openOnLeftClick = false;
        public List<FloatMenuOption> rightClickFloatMenuOptions;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => rightClickFloatMenuOptions;

        public void OpenMenu()
        {
            if (rightClickFloatMenuOptions == null) return;
            var menu = new FloatMenu(rightClickFloatMenuOptions);
            Find.WindowStack.Add(menu);
        }

        public override void ProcessInput(Event ev)
        {
            if (!openOnLeftClick || rightClickFloatMenuOptions.Count <= 1)
                base.ProcessInput(ev);
            else
            {
                OpenMenu();
                CurActivateSound?.PlayOneShotOnCamera();
            }
        }
    }
}
