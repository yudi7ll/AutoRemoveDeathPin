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