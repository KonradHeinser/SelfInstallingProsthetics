using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace SelfInstallingProsthetics
{
    public class CompUseEffect_InstallProsthetic : CompUseEffect
    {
        public CompProperties_UseEffectInstallProsthetic Props => (CompProperties_UseEffectInstallProsthetic)props;

        public override void DoEffect(Pawn usedBy)
        {
            List<BodyPartRecord> parts = usedBy.RaceProps.body.GetPartsWithDef(Props.bodyPart);

            if (!parts.NullOrEmpty())
            {
                if (parts.Count == 1)
                {
                    BodyPartRecord part = parts.First();
                    Hediff hediff = usedBy.health.hediffSet.GetFirstHediffOfDef(Props.hediff);

                    if (hediff == null)
                        DoPartInstall(usedBy, part);
                    else if (hediff.Severity < hediff.def.maxSeverity &&
                        (SIPDefOf.SIPExceptions.Leveling(hediff.def) || hediff.def.maxSeverity != float.MaxValue))
                        if (hediff is Hediff_Level leveling)
                            leveling.ChangeLevel(1);
                        else
                            hediff.Severity += 1;
                }
                else
                {
                    // If this pawn happens to not have any hediffs, just do a generic install
                    if (usedBy.health.hediffSet.hediffs.NullOrEmpty())
                    {
                        DoPartInstall(usedBy, parts.First(), true);
                        return;
                    }
                    // Otherwise, start by regaining any lost parts
                    foreach (BodyPartRecord part in parts)
                        if (usedBy.health.hediffSet.PartIsMissing(part))
                        {
                            DoPartInstall(usedBy, part);
                            return;
                        }

                    // Try to level up an existing hediff if possible
                    Hediff hediff = usedBy.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                    if (hediff != null && hediff.Severity < hediff.def.maxSeverity && (SIPDefOf.SIPExceptions.Leveling(hediff.def) || hediff.def.maxSeverity != float.MaxValue))
                    {
                        if (hediff is Hediff_Level leveling)
                            leveling.ChangeLevel(1);
                        else
                            hediff.Severity += 1;
                        return;
                    }

                    BodyPartRecord bestPart = null;
                    int lowestTech = (int)Props.hediff.spawnThingOnRemoved.techLevel;

                    // Start checking for existing implants to avoid
                    List<BodyPartRecord> tmpRecords = new List<BodyPartRecord>(parts);
                    foreach (Hediff h in usedBy.health.hediffSet.hediffs)
                        if (tmpRecords.Contains(h.Part) && h is Hediff_Implant)
                        {
                            tmpRecords.Remove(h.Part);
                            if (h.def != Props.hediff)
                            {
                                int level = h.def.spawnThingOnRemoved != null ? (int)h.def.spawnThingOnRemoved.techLevel : 99;
                                if (level < lowestTech) // Ignores any implant with the same level as this one
                                {
                                    bestPart = h.Part;
                                    lowestTech = level;
                                }
                            }
                            if (tmpRecords.NullOrEmpty())
                                break;
                        }

                    // If there's a part with no implants at all, go with that one
                    if (!tmpRecords.NullOrEmpty())
                    {
                        DoPartInstall(usedBy, tmpRecords.First());
                        return;
                    }

                    // Otherwise go with the part with a lower tech level
                    if (bestPart != null) 
                        DoPartInstall(usedBy, bestPart);

                    // Go with the first overall part since there isn't really anything else to try now
                    // If this is reached, something might have went horribly wrong
                    DoPartInstall(usedBy, parts.First());
                }
            }
        }

        private void DoPartInstall(Pawn usedBy, BodyPartRecord part, bool skipCurrent = false)
        {
            if (!skipCurrent)
            {
                ThingDef currentPart = usedBy.health.hediffSet.hediffs.Where(arg => arg.Part == part && arg is Hediff_Implant added).First().def.spawnThingOnRemoved;
                if (currentPart != null)
                    GenSpawn.Spawn(currentPart, usedBy.PositionHeld, usedBy.MapHeld);
            }

            Hediff newHediff = HediffMaker.MakeHediff(Props.hediff, usedBy, part);
            usedBy.health.RestorePart(part, newHediff);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!base.CanBeUsedBy(p))
                return false;
            return true;
        }
    }
}
