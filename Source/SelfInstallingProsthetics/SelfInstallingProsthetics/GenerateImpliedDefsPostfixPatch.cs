using System.Collections.Generic;
using System.Linq;
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
            foreach (ThingDef sip in ImpliedSIPs(hotReload))
                DefGenerator.AddImpliedDef(sip);
        }

        public static IEnumerable<ThingDef> ImpliedSIPs(bool hotReload = false)
        {
            ModContentPack thisMod = LoadedModManager.RunningModsListForReading.Where(arg => arg.PackageId == "Alite.SelfInstallingProsthetics").First();
            foreach (HediffDef hediff in DefDatabase<HediffDef>.AllDefs.ToList())
            {
                // Only include added parts
                if (!typeof(Hediff_AddedPart).IsAssignableFrom(hediff.hediffClass))
                    continue;

                // Make sure it is an implant that comes from an item
                if (!hediff.countsAsAddedPartOrImplant || hediff.spawnThingOnRemoved == null)
                    continue;

                // Make sure it comes from an item that has sufficient techlevel
                if ((int)hediff.spawnThingOnRemoved.techLevel < 6)
                    continue;

                // Make sure someone didn't manually exclude it
                if (SIPDefOf.SIPExceptions.Excluded(hediff))
                    continue;

                ThingDef normalThing = hediff.spawnThingOnRemoved;

                // If the base thing doesn't accept comps for some reason, we'll just run away
                if (!typeof(ThingWithComps).IsAssignableFrom(normalThing.thingClass))
                    continue;

                // If it already has a usable, we don't want to mess with that
                if (!normalThing.comps.NullOrEmpty())
                {
                    bool flag = false;
                    foreach (var comp in normalThing.comps)
                        if (comp is CompProperties_Usable)
                        {
                            flag = true;
                            break;
                        }
                    if (flag)
                        continue;
                }

                string defName = "SIP_" + normalThing.defName;
                ThingDef thing = (hotReload ? (DefDatabase<ThingDef>.GetNamedSilentFail(defName) ?? new ThingDef()) : new ThingDef());
                thing.defName = defName;

                // Copy stuff from the original item
                thing.label = "SelfInstallingImplant".Translate(normalThing.label);
                thing.description = normalThing.description + "\n\n" + "SelfInstallingProsthetic_DescriptionAddition".Translate();
                thing.drawerType = normalThing.drawerType;
                thing.resourceReadoutPriority = normalThing.resourceReadoutPriority;
                thing.thingClass = normalThing.thingClass;
                thing.thingCategories = normalThing.thingCategories;
                thing.graphicData = normalThing.graphicData;
                thing.useHitPoints = normalThing.useHitPoints;
                thing.statBases = normalThing.statBases;
                thing.BaseMarketValue = normalThing.BaseMarketValue * 1.5f;
                thing.rotatable = normalThing.rotatable;
                thing.stackLimit = normalThing.stackLimit;
                thing.comps = normalThing.comps;
                thing.thingSetMakerTags = normalThing.thingSetMakerTags;

                // Add comps
                if (thing.comps == null)
                    thing.comps = new List<CompProperties>();
                thing.comps.Add(new CompProperties_Usable
                {
                    useJob = SIPDefOf.UseItem,
                    useLabel = "SIPUse".Translate(thing.label),
                    showUseGizmo = true
                });
                thing.comps.Add(new CompProperties_UseEffectInstallProsthetic
                {
                    hediff = hediff,
                    bodyPart = hediff.defaultInstallPart
                });

                // Adds the more static information
                thing.modContentPack = thisMod;
                thing.pathCost = DefGenerator.StandardItemPathCost;
                thing.drawGUIOverlay = true;
                thing.category = ThingCategory.Item;
                thing.selectable = true;
                thing.alwaysHaulable = true;
                thing.healthAffectsPrice = false;


                // Add links
                thing.descriptionHyperlinks = new List<DefHyperlink>
                {
                    new DefHyperlink(hediff),
                    new DefHyperlink(normalThing)
                };
                if (!normalThing.descriptionHyperlinks.NullOrEmpty())
                    foreach (DefHyperlink link in normalThing.descriptionHyperlinks)
                        if (!thing.descriptionHyperlinks.Contains(link))
                            thing.descriptionHyperlinks.Add(link);

                yield return thing;
            }
        }
    }

}
