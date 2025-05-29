using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SelfInstallingProsthetics
{
    public class SelfInstallingProstheticsMod : Mod
    {
        internal static SIP_Settings settings;

        public SelfInstallingProstheticsMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony("Rimworld.Alite.SelfInstallingProsthetics");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = GetSettings<SIP_Settings>();
        }

        public override string SettingsCategory()
        {
            return "SIPMainLabel".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoWindowContents(inRect);
        }

        public static Harmony harmony;
    }

    public class SIP_Settings : ModSettings
    {
        public static TechLevel techLevel = TechLevel.Ultra;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref techLevel, "techLevel", TechLevel.Ultra);
        }

        private static List<FloatMenuOption> techLevelOptions;

        public List<FloatMenuOption> TechLevelOptions
        {
            get
            {
                if (techLevelOptions == null)
                {
                    techLevelOptions = new List<FloatMenuOption>();
                    foreach (TechLevel level in Enum.GetValues(typeof(TechLevel)))
                    {
                        if (level == TechLevel.Undefined)
                            continue;
                        techLevelOptions.Add(new FloatMenuOption(TechLevelUtility.ToStringHuman(level), delegate
                        {
                            techLevel = level;
                        }));
                    }
                }
                return techLevelOptions;
            }
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard optionsMenu = new Listing_Standard();

            var scrollContainer = new Rect(inRect);
            scrollContainer.height -= optionsMenu.CurHeight;
            Widgets.DrawBoxSolid(scrollContainer, Color.grey);
            var innerContainer = scrollContainer.ContractedBy(1);
            Widgets.DrawBoxSolid(scrollContainer, new ColorInt(37, 37, 37).ToColor);
            var frameRect = innerContainer.ContractedBy(5);
            frameRect.y += 15;
            frameRect.height -= 15;
            var contentRect = frameRect.ContractedBy(5);
            contentRect.x = -5;
            contentRect.y = 0;

            optionsMenu.Begin(contentRect.AtZero());

            if (optionsMenu.ButtonTextLabeledPct("SIPSetMinTechLevel".Translate(), TechLevelUtility.ToStringHuman(techLevel), 0.65f, tooltip: "SIPTechLevel".Translate()))
                Find.WindowStack.Add(new FloatMenu(TechLevelOptions));
            optionsMenu.End();
            Write();
        }
    }
}
