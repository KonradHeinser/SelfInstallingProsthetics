using System.Collections.Generic;
using Verse;

namespace SelfInstallingProsthetics
{
    public class SIPExceptionsDef : Def
    {
        public List<HediffDef> exclusions;

        public List<HediffDef> psychic;

        public List<HediffDef> leveling;

        public bool Excluded(HediffDef hediff)
        {
            if (exclusions.NullOrEmpty())
                return false;
            return exclusions.Contains(hediff);
        }

        public bool Psychic(HediffDef hediff)
        {
            if (psychic.NullOrEmpty())
                return false;
            return psychic.Contains(hediff);
        }

        public bool Leveling(HediffDef hediff)
        {
            if (leveling.NullOrEmpty())
                return false;
            return leveling.Contains(hediff);
        }
    }
}
