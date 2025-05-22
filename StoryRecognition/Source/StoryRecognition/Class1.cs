using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace StoryRecognition
{

    [StaticConstructorOnStartup]
    public static class MyInputMod
    {
        static MyInputMod()
        {
            var harmony = new Harmony("com.mymod.input");
            harmony.Patch(
                original: AccessTools.Method(typeof(UIRoot), "UIRootOnGUI"),
                postfix: new HarmonyMethod(typeof(MyInputMod), nameof(OnGUI_Postfix))
            );
        }

        public static void OnGUI_Postfix()
        {
            Event e = Event.current;
            if (e != null && (e.type == EventType.KeyDown || e.type == EventType.MouseDown))
            {
                Log.Message($"[MyMod] Detected event: {e.ToStringFull()}");
            }
        }
    }
}
