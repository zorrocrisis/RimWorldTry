
// ENABLE LOGS HERE
#define ENABLE_BATTLE_LOGS
#define ENABLE_SOCIAL_LOGS
#define ENABLE_ARCHIVE_LOGS
#define ENABLE_HEALTH_LOGS
//#define ENABLE_JOB_LOGS

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Verse;
using HugsLib;
using HugsLib.Utils;
using UnityEngine;
using System.IO;
using RimWorld;
using HarmonyLib;
using Verse.AI;
using static Verse.DamageWorker;
using RimWorld.Planet;


namespace StoryRecognition
{
    public class MainEventLogGatherer : ModBase
    {
        // Variables to write logs to file
        string fileName;
        static string logFilePath;
        static string logFolder;

        static List<LogEvent> structuredLogs;

        // Log gatherer (old method)
        //LogGatherer logGatherer;

        // List of colonists
        List<Pawn> colonistPawns;

        // Auxiliary variables (health effects log)
        static bool IsMapLoaded = false;

        // Auxiliary variables (combat log)
        static int battleStartTimestamp;
        static IntVec3 battlePosition;
        static Battle battleRef;
        static bool timerStarted;
        static int lastBattleEntryTimestamp;

        HeuristicTester heuristicTester;

        public override string ModIdentifier => "StoryRecognition";

        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);
           
            // Define path for file which stores all events
            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            logFolder = Path.Combine(GenFilePaths.SaveDataFolderPath, $"StoryRecognitionLogs", date);
            Directory.CreateDirectory(logFolder);


            // Log paths
            fileName = "all_events.txt";
            logFilePath = Path.Combine(logFolder, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

            IsMapLoaded = true;


            // OLD METHOD
            #region
            /*
            logGatherer = new LogGatherer();

            fileName = "all_events_" + date + ".txt";
            logFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "StoryRecognitionLogs", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

            colonistPawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);
            */
            #endregion

            // One tick per hour 
            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(() =>
            {
                //Log.Message("One hour has passed...");

                // OLD METHOD
                #region
                //logGatherer.WriteJobReports(logFilePath);
                #endregion

            }, 2500, null, true);

        }

        // // Dirty hacks for deleting static lists. Don't look.
        public override void SceneLoaded(Scene scene)
        {
            base.SceneLoaded(scene);
            if (!GenScene.InEntryScene)
            {
                return;
            }

        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            // Needed to define name for battle after it has finished

#if ENABLE_BATTLE_LOGS
            CheckBattleEnd();
#endif
        }

        // Trigger HUGS hotkey logic here
        public override void OnGUI()
        {
            base.OnGUI();

            // Detect key press to get log gatherer data 
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F10)
            {
                Find.LetterStack.ReceiveLetter("Latest Stories",
                                               "Your latest stories will be written here...",
                                               LetterDefOf.PositiveEvent);

                List<LogEvent> structuredLogs = EventLogPreprocessing.GetLogEvents(logFilePath);

                if(heuristicTester == null)
                {
                    heuristicTester = new HeuristicTester(structuredLogs, logFolder);
                }
                else
                {
                    heuristicTester.SetEventLog(structuredLogs);
                }

                //heuristicTester.TestTemporalDistanceOnly(2);
                //heuristicTester.TestSpatialDistanceOnly(30);
                //heuristicTester.TestSharedActorsOnly(2);

                //heuristicTester.TestSpaceAndTime(5, 5);
                heuristicTester.TestSpaceAndTimeAndMinimumCount(5, 5, 3);

            }

            // OLD METHOD
            #region
            /*
            // Detect key press to get log gatherer data 
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F10)
            {
                Log.Message("The following events have been uploaded to " + fileName + " (F10).");

                logGatherer.LogAllEventsToFile(logFilePath);

                //logGatherer.LogEverything();
            }
            */
            #endregion

        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();

            //Log.Message("World loaded...");
        }

#if ENABLE_BATTLE_LOGS
        /// <summary>
        /// Battle Logs
        /// </summary>
        #region

        // Check if battle has ended to give it a name considering all battle entries
        private void CheckBattleEnd()
        {
            if (timerStarted)
            {
                // Check if battle has ended (5000 ticks after last battle entry -> defined by code)
                if (Find.TickManager.TicksAbs - lastBattleEntryTimestamp > 5000)
                {
                    timerStarted = false;

                    // Give name to battle 
                    string battleInfo = $"CL_Timestamp_{battleStartTimestamp}_Location_{battlePosition}_ID_B{battleRef.GetUniqueLoadID().Split('_')[1]}_FullInfo_{battleRef.GetName()}";
                    File.AppendAllText(logFilePath, battleInfo + "\n");
                    Log.Message(battleInfo);

                    // Reset auxiliary variables
                    lastBattleEntryTimestamp = 0;
                    battleRef = null;
                    battleStartTimestamp = 0;
                    battlePosition = IntVec3.Zero;
                }

            }
        }

        // Register battle entries
        [HarmonyPatch(typeof(Battle), nameof(Battle.Add))]
        public static class Battle_Post_Add_Patch
        {
            static void Postfix(LogEntry entry, Battle __instance)
            {
                if (__instance.Entries.Count == 1)
                {
                    timerStarted = true;

                    battleStartTimestamp = entry.Timestamp - 1; // timestamp is first entry less one tick
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
                    string info = $"CL_Timestamp_{entry.Timestamp}_Location_{battleLogEntryPosition}_ID_b{entry.LogID}_Actors_{participatingThings.Join()}_FullInfo_{entry.ToGameStringFromPOV(participatingThings.First() as Pawn)}";
                    File.AppendAllText(logFilePath, info + "\n");
                    Log.Message(info);
                }
                else
                {
                    Log.Warning("SOMETHING WENT WRONG! ANIMAL SOLO LOG MOST LIKELY");
                }
            }
        }

        #endregion
#endif

#if ENABLE_SOCIAL_LOGS
        /// <summary>
        /// Social Logs
        /// </summary>
        #region

        // Register social logs
        [HarmonyPatch(typeof(PlayLog), nameof(PlayLog.Add))]
        public static class PlayLog_Post_Add_Patch
        {
            static void Postfix(LogEntry entry, PlayLog __instance)
            {
                if (entry != null)
                {
                    string briefInteractionString = entry.ToString();

                    // Fetch things (pawns) which are involved in the social log entry
                    IEnumerable<Thing> participatingThings = entry.GetConcerns();

                    if (participatingThings.Count() != 0)
                    {
                        IntVec3 location = participatingThings.First<Thing>().Position;

                        string info = $"SL_Timestamp_{entry.Timestamp}_Location_{location}_ID_S{entry.LogID}_Actors_{participatingThings.Join()}_FullInfo_{entry.ToGameStringFromPOV(participatingThings.First() as Pawn)}";
                        File.AppendAllText(logFilePath, info + "\n");
                        Log.Message(info);
                    }
                    else
                    {
                        Log.Warning("SOMETHING WENT WRONG! SOCIAL LOG");
                    }
                }
            }
        }
        #endregion
#endif

#if ENABLE_ARCHIVE_LOGS
        /// <summary>
        /// Archives/Letter Logs
        /// </summary>
        #region

        // Register archives/letters (events)
        [HarmonyPatch(typeof(Archive), nameof(Archive.Add))]
        public static class Archive_Post_Add_Patch
        {
            static void Postfix(IArchivable archivable, Archive __instance)
            {
                if (archivable != null)
                {

                    int timestamp = Find.TickManager.TicksAbs - Find.TickManager.TicksGame + archivable.CreatedTicksGame;
                    IntVec3 location = new IntVec3(0, 0, 0);
                    List<Thing> participatingThings = new List<Thing>();

                    if (archivable.LookTargets != null && archivable.LookTargets.PrimaryTarget.Cell != null)
                    {
                        location = archivable.LookTargets.PrimaryTarget.Cell;

                        // Try to get participating things
                        participatingThings = archivable.LookTargets.targets
                                                            .Select(item => item.Thing)
                                                            .Where(item => item != null)
                                                            .ToList();
                    }

                    string info = $"ARC_Timestamp_{timestamp}_Location_{location}_ID_L{archivable.GetUniqueLoadID().Split('_')[1]}_Actors_{participatingThings.Join()}_FullInfo_{archivable.ArchivedTooltip}";
                    File.AppendAllText(logFilePath, info + "\n");
                    Log.Message(info);
                }
            }
        }
        #endregion
#endif

#if ENABLE_HEALTH_LOGS
        /// <summary>
        /// Health Logs
        /// </summary>
        #region


        // Register health logs (adding health effects)
        [HarmonyPatch(typeof(HediffSet), nameof(HediffSet.AddDirect))]
        public static class HediffSet_Post_AddHediff_Patch
        {
            static void Postfix(HediffSet __instance)
            {

                if (IsMapLoaded == false)
                {
                    return;
                }

                Pawn affectedPawn = __instance.pawn;
                Hediff lastHediff = __instance.hediffs.Last<Hediff>();

                if (lastHediff != null && affectedPawn != null)
                {
                    int timestamp = Find.TickManager.TicksAbs - lastHediff.ageTicks;
                    IntVec3 location = affectedPawn.Position;
                    string pawnName = "NO_NAME";
                    string actorName = "NO_NAME";

                    // If it's a character
                    if (affectedPawn.Name != null)
                    {
                        pawnName = affectedPawn.Name.ToStringShort;
                        actorName = pawnName;
                    }
                    else
                    {
                        actorName = affectedPawn.ThingID.Replace("_", "");
                        pawnName = $"A(n) {affectedPawn.Label}";
                    }

                    string info = "";

                    // God knows why but downed/dead pawns don't have parts...
                    if (lastHediff.Part == null)
                    {
                        info = $"HP_Timestamp_{timestamp}_Location_{location}_ID_H{lastHediff.loadID}_Actors_{actorName}_FullInfo_{pawnName} was afflicted with (a) {lastHediff.Label}";
                    }
                    else
                    {
                        info = $"HP_Timestamp_{timestamp}_Location_{location}_ID_H{lastHediff.loadID}_Actors_{actorName}_FullInfo_{pawnName} was afflicted with (a) {lastHediff.Label} ({lastHediff.Part.Label})";
                    }

                    File.AppendAllText(logFilePath, info + "\n");
                    Log.Message(info);

                }

            }

        }

        // Register health logs (removing health effects)
        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff))]
        public static class Pawn_HealthTracker_Post_RemoveHediff_Patch
        {
            static void Postfix(Hediff hediff)
            {
                if (IsMapLoaded == false)
                {
                    return;
                }

                Pawn affectedPawn = hediff.pawn;

                if (hediff != null && affectedPawn != null)
                {
                    int timestamp = Find.TickManager.TicksAbs;
                    IntVec3 location = affectedPawn.Position;
                    string pawnName = "NO_NAME";
                    string actorName = "NO_NAME";

                    // If it's a character
                    if (affectedPawn.Name != null)
                    {
                        pawnName = affectedPawn.Name.ToStringShort;
                        actorName = pawnName;
                    }
                    else
                    {
                        actorName = affectedPawn.ThingID.Replace("_", "");
                        pawnName = $"A(n) {affectedPawn.Label}";
                    }


                    // The first line does not log without a participating pawn
                    string info = $"HP_Timestamp_{timestamp}_Location_{location}_ID_H{hediff.loadID}_Actors_{actorName}_FullInfo_{pawnName} healed from (a) {hediff.Label} ({hediff.Part.Label})";
                    File.AppendAllText(logFilePath, info + "\n");
                    Log.Message(info);

                }

            }

        }
        #endregion
#endif

#if ENABLE_JOB_LOGS
        /// <summary>
        /// Job Logs
        /// </summary>
        #region

        // Register job logs
        [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.EndCurrentJob))]
        public static class Pawn_JobTracker_Post_EndCurrentJob_Patch
        {
            static void Prefix(Pawn_JobTracker __instance)
            {
                int timestamp = Find.TickManager.TicksAbs;

                Pawn affectedPawn = __instance.curDriver.pawn;
                string pawnName = "NO_NAME";
                string actorName = "NO_NAME";

                // If it's a character
                if (affectedPawn.Name != null)
                {
                    pawnName = affectedPawn.Name.ToStringShort;
                    actorName = pawnName;
                }
                else
                {
                    actorName = affectedPawn.ThingID.Replace("_", "");
                    pawnName = $"A(n) {affectedPawn.Label}";           
                }

                string info = $"JOB_Timestamp_{timestamp}_Location_{affectedPawn.Position}_ID_J{__instance.curJob.GetUniqueLoadID().Split('_')[1]}_Actors_{actorName}_FullInfo_{pawnName} {__instance.curJob.GetReport(affectedPawn)}";
                File.AppendAllText(logFilePath, info + "\n");
                Log.Message(info);

            }

        }
        #endregion
#endif
    }
}
