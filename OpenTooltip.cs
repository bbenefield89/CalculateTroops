using HarmonyLib;
using System;
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
            var (inf, cav, arch, horseArch) = CalculateTroopTypes();
            foreach (TooltipProperty tooltipProperty in __instance.TooltipPropertyList)
            {
                if (tooltipProperty.DefinitionLabel.ToLower() == "infantry")
                {
                    tooltipProperty.ValueLabel += $" ( {inf} )";
                }
                else if (tooltipProperty.DefinitionLabel.ToLower() == "cavalry")
                {
                    tooltipProperty.ValueLabel += $" ( {cav} )";
                }
                else if (tooltipProperty.DefinitionLabel.ToLower() == "ranged")
                {
                    tooltipProperty.ValueLabel += $" ( {arch} )";
                }
                else if (tooltipProperty.DefinitionLabel.ToLower() == "horse archer")
                {
                    tooltipProperty.ValueLabel += $" ( {horseArch} )";
                }
                else if (tooltipProperty.DefinitionLabel.ToLower() == "prisoners")
                {
                    break;
                }
            }
        }

        private static (int, int, int, int) CalculateTroopTypes()
        {
            MobileParty mobileParty = MobileParty.MainParty;
            TroopRosterElement[] partyList = (TroopRosterElement[])mobileParty.MemberRoster.GetType().GetField("data", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mobileParty.MemberRoster);
            var (inf, cav, arch, horseArch) = (0, 0, 0, 0);
            foreach (TroopRosterElement troop in partyList)
            {
                if (troop.Number == 0)
                {
                    break;
                }
                else if (troop.Character.CurrentFormationClass.ToString().ToLower() == "horsearcher")
                {
                    horseArch += troop.Number;
                }
                else if (troop.Character.IsMounted == true)
                {
                    cav += troop.Number;
                }
                else if (troop.Character.IsInfantry == true)
                {
                    inf += troop.Number;
                }
                else if (troop.Character.IsArcher == true)
                {
                    arch += troop.Number;
                }
            }
            return (inf, cav, arch, horseArch);
        }
    }
}
