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
                CreateCavalaryTooltipPropertyIfNeeded(__instance);
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

        private static void CreateCavalaryTooltipPropertyIfNeeded(TooltipVM __instance)
        {
            TooltipProperty[] tooltipProperties = new TooltipProperty[__instance.TooltipPropertyList.Count];
            __instance.TooltipPropertyList.CopyTo(tooltipProperties, 0);
            for (int i = 0; i < tooltipProperties.Length; i++)
            {
                if (tooltipProperties[i].DefinitionLabel.ToLower() == "cavalry")
                {
                    break;
                }
                else if (tooltipProperties[i].DefinitionLabel.ToLower() == "prisoners")
                {
                    __instance.TooltipPropertyList.Insert(i - 2, new TooltipProperty("Cavalry", " 0", 0));
                    break;
                }
            }
        }

        private static void ApplyRealTroopCountTooltipFix(TooltipVM __instance)
        {
            Dictionary<string, int> troopCount = CalculateTroopTypes();
            foreach (TooltipProperty tooltipProperty in __instance.TooltipPropertyList)
            {
                if (tooltipProperty.DefinitionLabel.ToLower() == "prisoners")
                {
                    break;
                }

                if (troopCount.TryGetValue(tooltipProperty.DefinitionLabel.ToLower(), out int troopCountValue))
                {
                    tooltipProperty.ValueLabel += $" ( {troopCountValue} )";
                }
            }
        }

        private static Dictionary<string, int> CalculateTroopTypes()
        {
            MobileParty mobileParty = MobileParty.MainParty;
            TroopRosterElement[] partyList = (TroopRosterElement[])mobileParty.MemberRoster.GetType().GetField("data", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mobileParty.MemberRoster);
            Dictionary<string, int> troopCount = new Dictionary<string, int>();
            string[] troopTypeNames = { "infantry", "cavalry", "ranged", "horse archer" };
            foreach (string troopTypeName in troopTypeNames)
            {
                troopCount.Add(troopTypeName, 0);
            }

            foreach (TroopRosterElement troop in partyList)
            {
                if (troop.Number == 0)
                {
                    break;
                }
                else if (troop.Character.CurrentFormationClass.ToString().ToLower() == "horsearcher")
                {
                    troopCount["horse archer"] += troop.Number;
                }
                else if (troop.Character.IsMounted == true)
                {
                    troopCount["cavalry"] += troop.Number;
                }
                else if (troop.Character.IsInfantry == true)
                {
                    troopCount["infantry"] += troop.Number;
                }
                else if (troop.Character.IsArcher == true)
                {
                    troopCount["ranged"] += troop.Number;
                }
            }
            return troopCount;
        }
    }
}
