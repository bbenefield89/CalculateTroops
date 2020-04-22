using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;

namespace CalculateTroops
{
    [HarmonyPatch(typeof(TooltipVM), "OpenTooltip")]
    public class OpenTooltip
    {
        [HarmonyPostfix]
        private static void OpenTooltipPostfix(TooltipVM __instance, Type type, object[] args)
        {
            try
            {
                ApplyRealTroopCountTooltipFix(__instance);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n\n\n\n\n");
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                Debug.WriteLine("\n\n\n\n\n");
            }
        }

        private static void ApplyRealTroopCountTooltipFix(TooltipVM __instance)
        {
            Dictionary<string, int> troopCount = CalculateTroopTypes();
            foreach (TooltipProperty tooltipProperty in __instance.TooltipPropertyList)
            {
                string toolTipPropertyLabel = tooltipProperty.DefinitionLabel.ToLower();

                if (toolTipPropertyLabel == "prisoners")
                {
                    break;
                }

                if (troopCount.TryGetValue(toolTipPropertyLabel, out int troopCountValue))
                {
                    tooltipProperty.ValueLabel += $" ( {troopCountValue} )";
                }
            }
        }

        private static Dictionary<string, int> CalculateTroopTypes()
        {
            Dictionary<string, int> troopCount = BuildTroopCountDictionary();
            TroopRosterElement[] partyList = (TroopRosterElement[]) MobileParty.MainParty.MemberRoster.GetType()
                .GetField("data", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(MobileParty.MainParty.MemberRoster);

            foreach (TroopRosterElement troop in partyList)
            {
                if (troop.Number == 0)
                {
                    break;
                }

                string troopType = troop.Character.CurrentFormationClass.ToString().ToLower();

                if (troopType.Length > 10)
                {
                    troopType = troopType.Insert(5, " ");
                }

                troopCount[troopType] += troop.Number;
            }
            return troopCount;
        }

        private static Dictionary<string, int> BuildTroopCountDictionary()
        {
            Dictionary<string, int> troopCount = new Dictionary<string, int>(8);
            string[] troopTypeNames = { "infantry", "cavalry", "ranged", "horse archer", "skirmisher", "heavy infantry", "light cavalry", "heavy cavalry" };
            foreach (string troopTypeName in troopTypeNames)
            {
                troopCount.Add(troopTypeName, 0);
            }
            return troopCount;
        }
    }
}
