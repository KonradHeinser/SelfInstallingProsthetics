using HarmonyLib;
using RimWorld;
using Verse;

namespace SelfInstallingProsthetics
{
    [HarmonyPatch(typeof(DefGenerator))]
    [HarmonyPatch("GenerateImpliedDefs_PreResolve")]
    public static class GenerateImpliedDefsPostfixPatch
    {
        [HarmonyPrefix]
        public static void GenerateImpliedDefsPostfix(bool hotReload = false)
        {
            foreach (ThingDef sip in ThingDefGenerator_SelfInstallingProsthetic.ImpliedThingDefs(hotReload))
                DefGenerator.AddImpliedDef(sip);
        }
    }

}
