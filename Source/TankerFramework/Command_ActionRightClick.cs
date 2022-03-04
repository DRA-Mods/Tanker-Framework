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
            if (!openOnLeftClick)
                base.ProcessInput(ev);
            else if (rightClickFloatMenuOptions.Count == 1)
            {
                rightClickFloatMenuOptions[0].action();
                CurActivateSound?.PlayOneShotOnCamera();
            }
            else
            {
                OpenMenu();
                CurActivateSound?.PlayOneShotOnCamera();
            }
        }
    }
}
