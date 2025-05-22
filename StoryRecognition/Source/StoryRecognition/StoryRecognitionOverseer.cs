using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Verse;
using HugsLib;
using HugsLib.Utils;
using UnityEngine;
using System.IO;
using RimWorld;
using HarmonyLib;
using System.Runtime.InteropServices.ComTypes;
using static RimWorld.ColonistBar;

namespace StoryRecognition
{
    public class StoryRecognitionOverseer : ModBase
    {
        public static string updateLogFilePath;

        string logFilePath;

        string positionFileName;

        string positionFilePath;

        LogGatherer logGatherer;

        string fileName;

        List<Pawn> colonistPawns;


        public override string ModIdentifier => "StoryRecognition";

        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);

            logGatherer = new LogGatherer();

            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            fileName = "all_events_" + date + ".txt";
            logFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "StoryRecognitionLogs", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));


            positionFileName = "all_positions_" + date + ".txt";
            positionFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "StoryRecognitionLogs", positionFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(positionFilePath));

            positionFileName = "update_all_events_" + date + ".txt";
            updateLogFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "StoryRecognitionLogs", positionFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(positionFilePath));

            colonistPawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);

            ///////////////////////////////////// One tick per hour \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(() =>
            {
                //Log.Message("One hour has passed...");

                //logGatherer.WriteJobReports(logFilePath);

            }, 2500, null, true);

        }

        public override void SceneLoaded(Scene scene)
        {
            ///////////////////////////////////// Dirty hacks for deleting static lists. Don't look. \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\ 

            base.SceneLoaded(scene);
            if (!GenScene.InEntryScene)
            {
                return;
            }

        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            /*
            foreach (Pawn p in colonistPawns)
            {
                string info = $"POS_Timestamp_{Find.TickManager.TicksAbs}_Location_{p.Position}_ID_{p.ThingID}";
                File.AppendAllText(positionFilePath, info + "\n");
            }
            */

            if (timerStarted)
            {
                if(Find.TickManager.TicksAbs - lastBattleEntryTimestamp > 5000)
                {
                    timerStarted = false;

                    string battleInfo = $"CL_Timestamp_{battleStartTimestamp}_Location_{battlePosition}_ID_{"B" + battleRef.GetUniqueLoadID().Split('_')[1]}_FullInfo_{battleRef.GetName()}";
                    File.AppendAllText(updateLogFilePath, battleInfo + "\n");
                    Log.Message(battleInfo);

                    lastBattleEntryTimestamp = 0;
                    battleRef = null;
                    battleStartTimestamp = 0;
                    battlePosition = IntVec3.Zero;
                }

            }

        }

        public override void OnGUI() // Trigger your HUGS hotkey logic here
        {
            base.OnGUI();

            Event e = Event.current;

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F10)
            {
                Log.Message("The following events have been uploaded to " + fileName + " (F10).");

                logGatherer.LogAllEventsToFile(logFilePath);

                //logGatherer.LogEverything();
            }

        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();

            //Find.WindowStack.Add(new FakeWindow());

            Log.Message("World loaded...");

            //var obj = UtilityWorldObjectManager.GetUtilityWorldObject<Saves>();
            //_ = Find.World.GetComponent<Saves>();
        }

        static int battleStartTimestamp;
        static IntVec3 battlePosition;
        static Battle battleRef;
        static bool timerStarted;
        static int lastBattleEntryTimestamp;


        [HarmonyPatch(typeof(Battle), nameof(Battle.Add))]
        public static class Battle_Post_Add_Patch
        {
            static void Postfix(LogEntry entry, Battle __instance)
            {
                if (__instance.Entries.Count == 1)
                {
                    timerStarted = true;

                    battleStartTimestamp = entry.Timestamp;
                    battlePosition = entry.GetConcerns().First<Thing>().Position;
                    battleRef = __instance;
                }

                // Everytime we add a new entry, we update this time
                lastBattleEntryTimestamp = entry.Timestamp;

                // Fetch things (pawns) which are involved in the battle log entry
                IEnumerable<Thing> participatingThings = entry.GetConcerns();

                if (participatingThings.Count() != 0)
                {
                    IntVec3 battleLogEntryPosition = participatingThings.First<Thing>().Position;

                    // The first line does not log without a participating pawn
                    string info = $"CL_Timestamp_{entry.Timestamp}_Location_{battleLogEntryPosition}_ID_{entry.LogID}_FullInfo_{entry.ToGameStringFromPOV(participatingThings.First() as Pawn)}";
                    File.AppendAllText(updateLogFilePath, info + "\n");
                    Log.Message(info);
                }
                else
                {
                    Log.Warning("SOMETHING WENT WRONG! ANIMAL SOLO LOG MOST LIKELY");
                }
            }
        }
    }
}
