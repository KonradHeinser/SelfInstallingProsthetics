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
                bool levels = SIPDefOf.SIPExceptions.Leveling(Props.hediff);
                if (parts.Count == 1)
                {
                    BodyPartRecord part = parts.FirstOrDefault();
                    Hediff hediff = usedBy.health.hediffSet.GetFirstHediffOfDef(Props.hediff);

                    if (hediff == null)
                        DoPartInstall(usedBy, part);
                    else if (hediff.Severity < hediff.def.maxSeverity && levels)
                        LevelUp(hediff);
                }
                else
                {
                    // If this pawn happens to not have any hediffs, just do a generic install
                    if (usedBy.health.hediffSet.hediffs.NullOrEmpty())
                    {
                        DoPartInstall(usedBy, parts.FirstOrDefault(), true);
                        return;
                    }
                    // Otherwise, start by regaining any lost parts if possible
                    foreach (BodyPartRecord part in parts)
                        if (usedBy.health.hediffSet.PartIsMissing(part))
                        {
                            DoPartInstall(usedBy, part);
                            return;
                        }

                    // Try to level up an existing hediff if possible
                    Hediff hediff = usedBy.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                    if (hediff != null && levels)
                        if (hediff.Severity < hediff.def.maxSeverity)
                        {
                            LevelUp(hediff);
                            return;
                        }
                        else
                            foreach (Hediff h in usedBy.health.hediffSet.hediffs.Where(arg => arg.def == Props.hediff))
                            {
                                if (h.Severity < h.def.maxSeverity)
                                {
                                    LevelUp(h);
                                    return;
                                }
                            }

                    BodyPartRecord bestPart = null;
                    int lowestTech = (int)Props.hediff.spawnThingOnRemoved.techLevel;

                    // Start checking for existing implants to avoid
                    List<BodyPartRecord> tmpRecords = new List<BodyPartRecord>(parts);
                    foreach (Hediff h in usedBy.health.hediffSet.hediffs)
                        if (tmpRecords.Contains(h.Part) && h is Hediff_AddedPart)
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
                        DoPartInstall(usedBy, tmpRecords.FirstOrDefault());
                        return;
                    }

                    // Otherwise go with the part with a lower tech level
                    if (bestPart != null)
                    {
                        DoPartInstall(usedBy, bestPart);
                        return;
                    }

                    // Go with the first overall part since there isn't really anything else to try now
                    // If this is reached, something might have went horribly wrong
                    DoPartInstall(usedBy, parts.FirstOrDefault());
                    Log.Warning("Using panic part");
                }
            }
        }

        private void LevelUp(Hediff hediff)
        {
            if (hediff is Hediff_Level leveling)
                leveling.ChangeLevel(1);
            else
                hediff.Severity += 1;
        }

        private void DoPartInstall(Pawn usedBy, BodyPartRecord part, bool skipCurrent = false)
        {
            if (!skipCurrent)
            {
                var currentPartHediffs = usedBy.health.hediffSet.hediffs.Where(arg => arg.Part == part && arg is Hediff_AddedPart && arg.def.spawnThingOnRemoved != null);
                if (!currentPartHediffs.EnumerableNullOrEmpty())
                    foreach (var h in currentPartHediffs)
                        GenSpawn.Spawn(h.def.spawnThingOnRemoved, usedBy.PositionHeld, usedBy.MapHeld);
            }

            Hediff newHediff = HediffMaker.MakeHediff(Props.hediff, usedBy, part);
            usedBy.health.RestorePart(part, newHediff);
            usedBy.health.AddHediff(newHediff);
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {

            if (p.HasExtraHomeFaction() || !p.IsFreeColonist)
                return "InstallImplantNotAllowedForNonColonists".Translate();

            List<BodyPartRecord> parts = p.RaceProps.body.GetPartsWithDef(Props.bodyPart);
            bool levels = SIPDefOf.SIPExceptions.Leveling(Props.hediff);
            // Make sure they are a race that has a part that actually works
            if (parts.NullOrEmpty())
                return "InstallImplantNoBodyPart".Translate();

            if (SIPDefOf.SIPExceptions.Psychic(Props.hediff) && p.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)
                return "InstallImplantPsychicallyDeaf".Translate();

            Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(Props.hediff);

            if (parts.Count == 1)
            {
                BodyPartRecord part = parts.FirstOrDefault();
                if (hediff != null)
                {
                    if (!levels)
                        return "InstallImplantAlreadyInstalled".Translate();
                    else if (hediff.Severity == hediff.def.maxSeverity)
                        return "InstallImplantAlreadyMaxLevel".Translate();
                }
                else
                {
                    int techLevel = (int)Props.hediff.spawnThingOnRemoved.techLevel;
                    foreach (Hediff h in p.health.hediffSet.hediffs)
                    {
                        if (h.Part == part && h is Hediff_AddedPart)
                        {
                            int level = h.def.spawnThingOnRemoved != null ? (int)h.def.spawnThingOnRemoved.techLevel : 0;
                            if (level >= techLevel)
                                return "SIPImplantInTheWay".Translate(h.Label);
                        }
                    }
                }
            }
            else
            {
                if (hediff != null)
                {
                    if (!levels || hediff.Severity == hediff.def.maxSeverity)
                    {
                        List<BodyPartRecord> tmpRecords = new List<BodyPartRecord>(parts);
                        foreach (Hediff h in p.health.hediffSet.hediffs.Where(arg => arg.def == Props.hediff))
                        {
                            if (tmpRecords.Contains(h.Part))
                            {
                                if (levels && h.Severity < h.def.maxSeverity)
                                    return true;
                                tmpRecords.Remove(h.Part);

                                // If none of the parts allow for more installations, that's a no
                                if (tmpRecords.NullOrEmpty())
                                    if (levels)
                                        return "InstallImplantAlreadyMaxLevel".Translate();
                                    else
                                        return "InstallImplantAlreadyInstalled".Translate();
                            }
                        }
                    }
                    else
                        return true;
                }
                if (CheckObstacles(parts, p))
                    return "SIPInTheWay".Translate();
            }

            return base.CanBeUsedBy(p);
        }

        private bool CheckObstacles(List<BodyPartRecord> parts, Pawn p)
        {
            List<BodyPartRecord> tmpRecords = new List<BodyPartRecord>(parts);
            int techLevel = (int)Props.hediff.spawnThingOnRemoved.techLevel;
            foreach (Hediff h in p.health.hediffSet.hediffs)
                if (tmpRecords.Contains(h.Part) && h is Hediff_AddedPart)
                {
                    int level = h.def.spawnThingOnRemoved != null ? (int)h.def.spawnThingOnRemoved.techLevel : 99;
                    if (level >= techLevel || h.def == Props.hediff)
                        tmpRecords.Remove(h.Part);
                    if (tmpRecords.NullOrEmpty())
                        break;
                }

            return tmpRecords.NullOrEmpty();
        }
    }
}
