using System.Reflection;
using System.Runtime.CompilerServices;
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
        private const string Version = "1.0.0";
        public static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(Name);

        private Harmony _harmony;
    
        private void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Guid);
        }

        [HarmonyPatch(typeof(ZNetView), "Destroy")]
        class ZNetView_Destroy_Patch
        {
            static void Prefix(ZNetView __instance)
            {
                if (!__instance) return;
                var tombstone = __instance.GetComponent<TombStone>();
                if (!tombstone) return;
                Minimap.instance.RemovePin(tombstone.transform.position, 1.5f);
            }
        }
        
        
        [HarmonyPatch(typeof(Player), "OnDeath")]
        class Player_OnDeath_Patch
        {
            private static bool noTombstoneCreated = false;
            
            static void Prefix(Player __instance)
            {
                noTombstoneCreated = __instance.m_inventory.NrOfItems() == 0;
            }
            static void Postfix(Player __instance)
            {
                var pins = Minimap.instance.m_pins;
                pins.Reverse();
                
                foreach (var pin in pins)
                {
                    if (pin.m_type != Minimap.PinType.Death) continue;
                    if (!noTombstoneCreated) continue;
                    
                    Minimap.instance.RemovePin(pin);
                    noTombstoneCreated = false;
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