using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace EncounterPartyAccess
{
    /// <summary>
    /// Entry point. Applies the Harmony patch that re-enables the Party screen (the P hotkey
    /// and the lower-left campaign nav-bar button) while the player is sitting in a pre-battle
    /// encounter menu, i.e. when <c>MobileParty.MainParty.MapEvent</c> is active.
    /// </summary>
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "com.encounterpartyaccess";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                new Harmony(HarmonyId).PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Debug.Print("[EncounterPartyAccess] Failed to apply Harmony patches: " + ex);
            }
        }
    }
}
