using System;
using System.Reflection;
using HarmonyLib;
using SandBox.View.Map.Navigation;
using SandBox.View.Map.Navigation.NavigationElements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EncounterPartyAccess.Patches
{
    /// <summary>
    /// Re-enables Party screen access while in a pre-battle encounter menu.
    ///
    /// Both the P hotkey (<see cref="MapScreen"/> game key 43) and the lower-left nav-bar button
    /// read <c>PartyNavigationElement.GetPermission().IsAuthorized</c>. Stock logic returns
    /// unauthorized whenever <c>MobileParty.MainParty.MapEvent != null</c>, which is exactly the
    /// pre-battle / encounter situation. This postfix lifts ONLY that one gate: it re-checks every
    /// other blocker the original method applies (nav bar globally enabled, not already on the
    /// party screen, not on a raft / prisoner, no mission forbidding party access) and authorizes
    /// only when the active map event was the sole reason it was blocked.
    ///
    /// Patching <c>GetPermission</c> covers both entry points at once, since both funnel through it.
    /// The stock <c>PartyNavigationElement</c> is also the one Naval DLC uses (its
    /// <c>NavalMapNavigationHandler</c> derives from <c>MapNavigationHandler</c> and keeps the base
    /// elements), so this works with or without the DLC.
    /// </summary>
    [HarmonyPatch(typeof(PartyNavigationElement), "GetPermission")]
    public static class PartyNavigationPermissionPatch
    {
        // _handler is a protected field on MapNavigationElementBase; needed to re-run
        // MapNavigationHelper.IsNavigationBarEnabled with the live handler.
        private static readonly FieldInfo HandlerField =
            AccessTools.Field(typeof(MapNavigationElementBase), "_handler");

        [HarmonyPostfix]
        private static void Postfix(PartyNavigationElement __instance, ref NavigationPermissionItem __result)
        {
            try
            {
                // Only intervene when access is currently blocked.
                if (__result.IsAuthorized)
                {
                    return;
                }

                MobileParty main = MobileParty.MainParty;

                // We only lift the "active map event" gate. If there is no map event, the block
                // came from a different (legitimate) reason we must respect.
                if (main == null || main.MapEvent == null)
                {
                    return;
                }

                // Re-evaluate every OTHER blocker from PartyNavigationElement.GetPermission so we
                // keep all the legitimate ones intact and only remove the map-event restriction.
                MapNavigationHandler handler = HandlerField?.GetValue(__instance) as MapNavigationHandler;
                if (handler == null || !MapNavigationHelper.IsNavigationBarEnabled(handler))
                {
                    return;
                }
                if (__instance.IsActive)
                {
                    return; // already on the party screen
                }
                if (main.IsInRaftState || Hero.MainHero.HeroState == Hero.CharacterStates.Prisoner)
                {
                    return;
                }
                Mission mission = Mission.Current;
                if (mission != null && !mission.IsPartyWindowAccessAllowed)
                {
                    return;
                }

                // The active map event was the sole remaining blocker -> allow the Party screen.
                __result = new NavigationPermissionItem(true, null);
            }
            catch (Exception ex)
            {
                Debug.Print("[EncounterPartyAccess] GetPermission postfix failed: " + ex);
            }
        }
    }
}
