using System.Collections.Generic;
using Verse;

namespace SelfInstallingProsthetics
{
    public class SIPExceptionsDef : Def
    {
        private List<HediffDef> exclusions;

        private List<HediffDef> psychic;

        private List<HediffDef> leveling;

        private List<string> excludedRecipeWorkers;

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
                return false;
            if (excludedRecipeWorkers.NullOrEmpty())
                return false;
            return excludedRecipeWorkers.Contains(recipe.workerClass.Namespace + "." + recipe.workerClass.Name);
        }
    }
}
