using System.Collections.Generic;
using Verse;

namespace SelfInstallingProsthetics
{
    public class SIPExceptionsDef : Def
    {
        public List<HediffDef> exclusions;

        public List<string> excludedPrefixes;

        public List<HediffDef> psychic;

        public List<HediffDef> leveling;

        public List<string> excludedRecipeWorkers;

        public bool Excluded(HediffDef hediff)
        {
            if (exclusions.NullOrEmpty() && excludedPrefixes.NullOrEmpty())
                return false;
            if (!excludedPrefixes.NullOrEmpty())
                foreach (string prefix in excludedPrefixes)
                    if (hediff.defName.StartsWith(prefix))
                        return true;

            return exclusions.Contains(hediff);
        }

        public bool Psychic(HediffDef hediff)
        {
            return !psychic.NullOrEmpty() && psychic.Contains(hediff);
        }

        public bool Leveling(HediffDef hediff)
        {
            // If the modder set a max severity, assume it's levelable
            if (hediff.maxSeverity < float.MaxValue)
                return true;

            // If there aren't exclusions, assume it's not levelable
            if (leveling.NullOrEmpty())
                return false;

            // Otherwise, check the list
            return leveling.Contains(hediff);
        }

        public bool InvalidRecipe(RecipeDef recipe)
        {
            if (recipe.appliedOnFixedBodyParts.NullOrEmpty())
                return true;
            return !excludedRecipeWorkers.NullOrEmpty() && excludedRecipeWorkers.Contains(recipe.workerClass.Namespace + "." + recipe.workerClass.Name);
        }
    }
}
