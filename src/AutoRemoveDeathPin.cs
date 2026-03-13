using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AutoRemoveDeathPin
{
    [BepInPlugin(Guid, Name, Version)]
    public class AutoRemoveDeathPin : BaseUnityPlugin
    {
        private const string Guid = "yudi7ll.autoremovedeathpin";
        private const string Name = "AutoRemoveDeathPin";
        private const string Version = "1.1.2";
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(Name);

        private Harmony _harmony;
    
        private void Awake()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                logger.LogInfo("Dedicated server detected. Mod disabled.");
                return;
            }

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Guid);
            // logger.LogInfo("Mod loaded.");
        }

        [HarmonyPatch(typeof(TombStone), "GiveBoost")]
        class Tombstone_GiveBoost_Patch
        {
            static void Postfix(TombStone __instance)
            {
                // logger.LogInfo($"Removing death pin for tombstone at {__instance.transform.position}");
                Minimap.instance.RemovePin(__instance.transform.position, 1.5f);
            }
        }
        
        
        [HarmonyPatch(typeof(Player), "OnDeath")]
        class Player_OnDeath_Patch
        {
            static void Prefix(Player __instance, ref bool __state)
            {
                var noTombstoneCreated = __instance.m_inventory.NrOfItems() == 0;
                if (noTombstoneCreated)
                {
                    __state = true;
                }
            }
            static void Postfix(Player __instance, ref bool __state)
            {
                if (!__state) return;
                var pins = Minimap.instance.m_pins;
                
                for (var i = pins.Count - 1; i >= 0; i--)
                {
                    var pin = pins[i];
                    if (pin.m_type != Minimap.PinType.Death) continue;
                    
                    // logger.LogInfo("Removing death pin because no tombstone was created.");
                    Minimap.instance.RemovePin(pin);
                    __state = false;
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
