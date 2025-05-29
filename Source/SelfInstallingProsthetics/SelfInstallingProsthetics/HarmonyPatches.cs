using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace SelfInstallingProsthetics
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        public HarmonyPatches()
        {
            Harmony harmony = new Harmony("Rimworld.Alite.SelfInstallingProsthetics");

            harmony.Patch(AccessTools.Method(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve)),
                postfix: new HarmonyMethod(patchType, nameof(GenerateImpliedDefsPostfix)));
        }

        public static void GenerateImpliedDefsPostfix(bool hotReload)
        {
            foreach (ThingDef sip in ThingDefGenerator_SelfInstallingProsthetic.ImpliedThingDefs(hotReload))
                DefGenerator.AddImpliedDef(sip);
        }
    }
}
