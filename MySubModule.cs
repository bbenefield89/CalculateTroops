using HarmonyLib;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace CalculateTroops
{
    class MySubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            Console.WriteLine("Loading Harmony...");
            Harmony.DEBUG = true;
            Harmony harmony = new Harmony("Informative Party Tooltips");
            harmony.PatchAll();
            Console.WriteLine("Patching...");
            base.OnSubModuleLoad();
        }
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            InformationManager.DisplayMessage(new InformationMessage("Loaded Informative Party Tooltips.", Color.FromUint(4282569842U)));
        }

    }
}
