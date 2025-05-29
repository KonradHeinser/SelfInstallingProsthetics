using RimWorld;
using Verse;

namespace SelfInstallingProsthetics
{
    public class CompProperties_UseEffectInstallProsthetic : CompProperties_UseEffect
    {
        public HediffDef hediff;

        public BodyPartDef bodyPart;

        public CompProperties_UseEffectInstallProsthetic()
        {
            compClass = typeof(CompUseEffect_InstallProsthetic);
        }
    }
}
