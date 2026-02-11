using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace AutoRemoveDeathPin
{
    [BepInPlugin(Guid, Name, Version)]
    public class AutoRemoveDeathPin : BaseUnityPlugin
    {
        private const string Guid = "yudi7ll.autoremovedeathpin";
        private const string Name = "AutoRemoveDeathPin";
        private const string Version = "1.0.0";

        private Harmony _harmony;
    
        private void Awake()
        {
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Guid);
        }

        [HarmonyPatch(typeof(TombStone), "OnTakeAllSuccess")]
        class Tombstone_OnTakeAllSuccess_Patch
        {
            static void Postfix(TombStone __instance)
            {
                var deathPosition = __instance.transform.position;
                const float radius = 0.1f;
                Minimap.instance.RemovePin(deathPosition, radius);
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}