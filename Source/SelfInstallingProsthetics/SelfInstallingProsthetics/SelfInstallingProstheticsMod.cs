using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace SelfInstallingProsthetics
{
    public class SelfInstallingProstheticsMod : Mod
    {
        public SelfInstallingProstheticsMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony("Rimworld.Alite.SelfInstallingProsthetics");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static Harmony harmony;
    }
}
