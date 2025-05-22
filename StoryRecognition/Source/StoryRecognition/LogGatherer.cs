using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using LudeonTK;
using UnityEngine;
using System.IO;
using HarmonyLib;
using Verse.AI;

namespace StoryRecognition
{
    public class LogGatherer
    {

        List<ColonistBar.Entry> colonists;
        List<Pawn> colonistPawns;

        public LogGatherer()
        {
            // Get updted list of colonists
            colonists = Find.ColonistBar.Entries;
            colonistPawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);
        }

        public void LogAllEventsToFile(string path)
        {
            WriteBattleLogEntries(path);
            WriteSocialLogEntries(path);
            WriteAllPreviousArchivables(path);
            WriteHealthDetails(path);

            // WriteJobReports is done automatically, 1 in-game hour at a time

            // AI SUMMARY

            SendLetter();
        }

        public void LogEverything()
        {

            // Get updted list of colonists
            colonists = Find.ColonistBar.Entries;
            colonistPawns = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);

            //GetSociaLogEntries();

            GetBattleLogEntries();

            //GetJobReports();

            // Get health conditions of colonists 
            //GetHealthDetails();

            // Gets all previous archivables, including log messages and previous events (e.g. Muffalo revenge)
            //GetAllPreviousArchivables();

            ///////////////////////////////////////////////////
            // NOT SUPER USEFUL ///////////////////////////////
            ///////////////////////////////////////////////////

            // Get info about age, count and interest of things that have happened in the past (e.g. downed, death, wounded)
            //GetTaleManagerTales();

            // Stats about playthrough (after 60 seconds or 6 minutes?)
            //GetStorytellerInfo();

        }

        private void GetSocialLogEntries()
        {
            List<LogEntry> logEntries = Find.PlayLog.AllEntries;

            if (logEntries != null)
            {
                // Log recent social interactions or relationships
                foreach (var logEntry in logEntries)
                {

                    string briefInteractionString = logEntry.ToString();
                    Pawn participantPawn = null;

                    // Fetch one of the colonists who participated in the event log
                    foreach (var colonist in colonistPawns)
                    {
                        // Check if interaction string contains colonist's nickname
                        if (briefInteractionString.Contains(colonist.Name.ToString().Split('\'')[1]))
                        {
                            participantPawn = colonist;
                            break;
                        }
                    }

                    // If we managed to fetch a participating colonist, log relevant information
                    if (participantPawn != null)
                    {
                        // The first line does not log without a participating pawn
                        Log.Message($"Full interaction: {logEntry.ToGameStringFromPOV(participantPawn)}");
                        Log.Message($"Logid {logEntry.LogID}");
                        Log.Message($"What? {briefInteractionString}");
                        Log.Message($"When? age {logEntry.Age}, tick {logEntry.Tick}, timestamp {logEntry.Timestamp}, in-game {logEntry.GetTipString()}");
                    }
                    else
                    {
                        Log.Message("Something went wrong!");
                    }
                }

            }
        }

        private void GetBattleLogEntries()
        {
            List<Battle> battleEntries = Find.BattleLog.Battles;

            if (battleEntries != null)
            {
                // Log recent social interactions or relationships
                foreach (var battleEntry in battleEntries)
                {

                    List<LogEntry> battleLogEntries = battleEntry.Entries;

                    Log.Message($"CL_Timestamp_{battleEntry.CreationTimestamp}_ID_{battleEntry.GetUniqueLoadID()}_BattleName_{battleEntry.GetName()}");

                    foreach (var battleLogEntry in battleLogEntries)
                    {
                        string briefbattleString = battleLogEntry.ToString();
                        Pawn participantPawn = null;

                        // Fetch one of the colonists who participated in the event log
                        foreach (var colonist in colonistPawns)
                        {
                            if (colonist.Name.ToString().Split('\'').Length == 3) // PEOPLE
                            {
                                // Check if interaction string contains colonist's nickname
                                if (briefbattleString.Contains(colonist.Name.ToString().Split('\'')[1]))
                                {
                                    participantPawn = colonist;
                                    break;
                                }
                            }
                            else if (colonist.Name.ToString().Split('\'').Length == 1) //PETS
                            {
                                Log.Message("Pet");

                                // Check if interaction string contains colonist's nickname
                                if (briefbattleString.Contains(colonist.Name.ToString().Split('\'')[0]))
                                {
                                    participantPawn = colonist;
                                    break;
                                }
                            }
                        }

                        // If we managed to fetch a participating colonist, log relevant information
                        if (participantPawn != null)
                        {
                            // The first line does not log without a participating pawn
                            Log.Message($"CL_Timestamp_{battleLogEntry.Timestamp}_ID_{battleLogEntry.LogID}_FullInfo_{battleLogEntry.ToGameStringFromPOV(participantPawn)}");
                            //Log.Message($"Logid {battleLogEntry.LogID}");
                            //Log.Message($"What? {briefbattleString}");
                            //Log.Message($"When? age {battleLogEntry.Age}, tick {battleLogEntry.Tick}, timestamp {battleLogEntry.Timestamp}, in-game {battleLogEntry.GetTipString()}");
                        }
                        else
                        {
                            Log.Message("Something went wrong with the battle logs!");
                        }

                    }
                }

            }
        }

        // Get info about age, count and interest of things that have happened in the past (e.g. downed, death, wounded)
        private void GetTaleManagerTales()
        {

            List<Tale> tales = Find.TaleManager.AllTalesListForReading;

            Log.Message($"Log Tale:");
            Find.TaleManager.LogTales();

            Log.Message($"Log Tale Interest Summary:");
            Find.TaleManager.LogTaleInterestSummary();

            if (tales != null)
            {
                foreach (var tale in tales)
                {
                    Log.Message($"Tale unique ID ({tale.GetUniqueLoadID()})");
                    Log.Message($"Short summary {tale.ShortSummary})");
                    Log.Message($"To string {tale.ToString()})");
                }

            }
        }


        // Stats about playthrough
        private void GetStorytellerInfo()
        {

            Log.Message($"Storyteller summary: ({Find.Storyteller.DebugString()})");
        }

        private void GetHealthDetails()
        {


            // Fetch one of the colonists who participated in the event log
            foreach (var colonist in colonistPawns)
            {
                Log.Message($"Colonist ({colonist.Name})");
                List<Hediff> injuries = colonist.health.hediffSet.hediffs;

                if (injuries != null)
                {
                    // Log recent social interactions or relationships
                    foreach (var injury in injuries)
                    {
                        Log.Message($"Injury main label {injury.Label})");
                        Log.Message($"Injury combat log text {injury.combatLogText})");
                        Log.Message($"Age ticks ({injury.ageTicks})");
                        Log.Message($"Load ID ({injury.loadID})");

                        /*
                        if(injury.sourceBodyPartGroup != null)
                        {
                            Log.Message($"Injury label short ({injury.sourceBodyPartGroup.LabelShort})");
                        }
                        */

                        //Log.Message($"Injury source label ({injury.sourceLabel})");
                        //Log.Message($"Injury source hediff def {injury.sourceHediffDef})");
                    }
                }


                /*
                List<BodyPartRecord> tm = colonist.pawn.health.hediffSet.GetInjuredParts();
                if (tm != null)
                {
                    // Log recent social interactions or relationships
                    foreach (var injury in tm)
                    {
                        Log.Message($"Injury custom label ({injury.customLabel})");
                        Log.Message($"Injury label {injury.Label})");
                        //Log.Message($"Injury to string {injury.ToString()})");
                    }
                }
                */
            }

        }

        // Gets all previous archivables, including log messages and previous events (e.g. Muffalo revenge)
        private void GetAllPreviousArchivables()
        {
            List<IArchivable> archivables = Find.Archive.ArchivablesListForReading;

            if (archivables != null)
            {
                foreach (var archive in archivables)
                {
                    Log.Message($"Archive label ({archive.ArchivedLabel}), tooltip ({archive.ArchivedTooltip}), ticks {archive.CreatedTicksGame}");
                }
            }
        }

        public void WriteJobReports(string outputFile)
        {
            int timestamp = Find.TickManager.TicksAbs;

            foreach (Pawn colonist in colonistPawns)
            {
                //colonist.jobs.debugLog = true;

                string info = $"JOB_Timestamp_{timestamp}_Location_{colonist.Position}_ID_{colonist.jobs.curJob.GetUniqueLoadID()}_FullInfo_{colonist.Name} {colonist.GetJobReport()}";
                File.AppendAllText(outputFile, info + "\n");
                Log.Message(info);
            }
        }

        public void GetMapPawns()
        {
            List<Pawn> pawns = Find.CurrentMap.mapPawns.AllPawns;

            foreach(Pawn p in pawns)
            {
                if(p.Name != null)
                {
                    Log.Message(p.Name.ToString());
                }
            }
        }

        private void WriteBattleLogEntries(string outputFile)
        {

            List<Battle> battleEntries = Find.BattleLog.Battles;

            if (battleEntries != null)
            {
                // Log recent social interactions or relationships
                foreach (Battle battleEntry in battleEntries)
                {

                    List<LogEntry> battleLogEntries = battleEntry.Entries;

                    int timestamp = Find.TickManager.TicksAbs - Find.TickManager.TicksGame + battleEntry.CreationTimestamp;
                    IntVec3 battlePosition = battleEntry.Entries[0].GetConcerns().First<Thing>().Position;

                    string battleInfo = $"CL_Timestamp_{timestamp}_Location_{battlePosition}_ID_{"B" + battleEntry.GetUniqueLoadID().Split('_')[1]}_FullInfo_{battleEntry.GetName()}";
                    File.AppendAllText(outputFile, battleInfo + "\n");
                    Log.Message(battleInfo);


                    foreach (var battleLogEntry in battleLogEntries)
                    {
                        // Fetch things (pawns) which are involved in the battle log entry
                        IEnumerable<Thing> participatingThings = battleLogEntry.GetConcerns();

                        if (participatingThings.Count() != 0)
                        {
                            IntVec3 battleLogEntryPosition = participatingThings.First<Thing>().Position;

                            // The first line does not log without a participating pawn
                            string info = $"CL_Timestamp_{battleLogEntry.Timestamp}_Location_{battleLogEntryPosition}_ID_{battleLogEntry.LogID}_FullInfo_{battleLogEntry.ToGameStringFromPOV(participatingThings.First() as Pawn)}";
                            File.AppendAllText(outputFile, info + "\n");
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

        private void WriteSocialLogEntries(string outputFile)
        {

            List<LogEntry> socialLogEntries = Find.PlayLog.AllEntries;

            if (socialLogEntries != null)
            {
                // Log recent social interactions or relationships
                foreach (LogEntry socialLogEntry in socialLogEntries)
                {
                    string briefInteractionString = socialLogEntry.ToString();

                    // Fetch things (pawns) which are involved in the social log entry
                    IEnumerable<Thing> participatingThings = socialLogEntry.GetConcerns();

                    if (participatingThings.Count() != 0)
                    {
                        IntVec3 location = participatingThings.First<Thing>().Position;

                        // The first line does not log without a participating pawn
                        string info = $"SL_Timestamp_{socialLogEntry.Timestamp}_Location_{location}_ID_{socialLogEntry.LogID}_FullInfo_{socialLogEntry.ToGameStringFromPOV(participatingThings.First() as Pawn)}";
                        File.AppendAllText(outputFile, info + "\n");
                        Log.Message(info);
                    }
                    else
                    {
                        Log.Warning("SOMETHING WENT WRONG! SOCIAL LOG");
                    }
                }

            }
        }

        public void SendLetter()
        {
            Find.LetterStack.ReceiveLetter("Latest Stories",
                "Your latest stories will be written here...",
                LetterDefOf.PositiveEvent);

        }

        private void WriteAllPreviousArchivables(string outputFile)
        {
            List<IArchivable> archivables = Find.Archive.ArchivablesListForReading;

            if (archivables != null)
            {
                foreach (IArchivable archive in archivables)
                {
                    int timestamp = Find.TickManager.TicksAbs - Find.TickManager.TicksGame + archive.CreatedTicksGame;
                    //Log.Message(GenDate.DateFullStringAt(timestamp, Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile)));

                    IntVec3 location = new IntVec3(0, 0, 0);

                    if(archive.LookTargets != null && archive.LookTargets.targets != null
                                             && archive.LookTargets.targets[0].Pawn != null && archive.LookTargets.targets[0].Pawn.Position != null)
                    {
                       location = archive.LookTargets.targets[0].Pawn.Position;
                    }

                    string info = $"ARC_Timestamp_{timestamp}_Location_{location}_ID_{"L" + archive.GetUniqueLoadID().Split('_')[1]}_FullInfo_{archive.ArchivedTooltip}";
                    File.AppendAllText(outputFile, info + "\n");
                    Log.Message(info);
                }
            }
        }

        private void WriteHealthDetails(string outputFile)
        {
            // Fetch one of the colonists who participated in the event log
            foreach (var colonist in colonistPawns)
            {
                List<Hediff> injuries = colonist.health.hediffSet.hediffs;

                if (injuries != null)
                {
                    // Log recent social interactions or relationships
                    foreach (var injury in injuries)
                    {
                        int timestamp = Find.TickManager.TicksAbs - injury.ageTicks;
                        IntVec3 location = new IntVec3(0, 0, 0);// ?????????????????????????????


                        // The first line does not log without a participating pawn
                        string info = $"HP_Timestamp_{timestamp}_Location_{location}_ID_{injury.loadID}_FullInfo_{injury.combatLogText}";
                        File.AppendAllText(outputFile, info + "\n");
                        Log.Message(info);

                        Log.Message($"Injury main label {injury.Label})");
                    }
                }
            }

        }

        private void WriteBattleLogEntriesOLD(string outputFile)
        {

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            List<Battle> battleEntries = Find.BattleLog.Battles;

            if (battleEntries != null)
            {
                // Log recent social interactions or relationships
                foreach (var battleEntry in battleEntries)
                {

                    List<LogEntry> battleLogEntries = battleEntry.Entries;


                    string battleInfo = $"CL_Timestamp_{battleEntry.CreationTimestamp}_ID_{"B" + battleEntry.GetUniqueLoadID().Split('_')[1]}_BattleName_{battleEntry.GetName()}";
                    File.AppendAllText(outputFile, battleInfo + "\n");
                    Log.Message(battleInfo);

                    foreach (var battleLogEntry in battleLogEntries)
                    {
                        string briefbattleString = battleLogEntry.ToString();
                        //Log.Message(briefbattleString);

                        Pawn participantPawn = null;

                        // Fetch one of the colonists who participated in this current event log
                        foreach (Pawn p in colonistPawns)
                        {

                            if (p.Name.ToString().Split('\'').Length == 3) // PEOPLE
                            {
                                // Check if interaction string contains colonist's nickname
                                if (briefbattleString.Contains(p.Name.ToString().Split('\'')[1]))
                                {
                                    participantPawn = p;
                                    break;
                                }
                            }
                            else if (p.Name.ToString().Split('\'').Length == 1) //PETS
                            {
                                //Log.Message("Pet");

                                // Check if interaction string contains colonist's nickname
                                if (briefbattleString.Contains(p.Name.ToString().Split('\'')[0]))
                                {
                                    participantPawn = p;
                                    break;
                                }
                            }

                        }

                        // If we managed to fetch a participating colonist, log relevant information
                        if (participantPawn != null)
                        {
                            // The first line does not log without a participating pawn
                            string info = $"CL_Timestamp_{battleLogEntry.Timestamp}_ID_{battleLogEntry.LogID}_FullInfo_{battleLogEntry.ToGameStringFromPOV(participantPawn)}";
                            File.AppendAllText(outputFile, info + "\n");
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

        
    }
}



// NEEDS TAB
//List<Thought> outThoughts = new List<Thought>();
//pawn.needs?.mood?.thoughts?.GetAllMoodThoughts(outThoughts);
//Log.Message($"{pawn.Name} interacted with1 {socialThought.LabelCap}");
//Log.Message($"{pawn.Name} interacted with2 {socialThought.Description}");
//Log.Message($"{pawn.Name} interacted with3 {socialThought.ToString()}");

// ALSO IN NEEDS TAB?? "Soaking wet" -> "I'm soaking wet"
//List<Thought_Memory> Memories = pawn.needs?.mood?.thoughts?.memories.Memories;

// RELATED TO COMBAT BUT COULDNT GET ANYTHING OUT
//pawn.records;

// CHILDHOOD/ADULTHOOD STORY -> PAWN'S BACKSTORY
//pawn.story;

// RELATIONS
//pawn.relations;
//public static RelationshipRecords RelationshipRecords => Current.Game.relationshipRecords;



// GETS SOCIAL LOGS OF SHOOTER
/*
List<LogEntry> le = Find.PlayLog.AllEntries;
Log.Message($"{shooter.Name}, full interaction: {socialThought.ToGameStringFromPOV(projectile.Launcher)}");
Log.Message($"{shooter.Name} interacted with logid {socialThought.LogID}");
Log.Message($"{shooter.Name} did what? {socialThought.ToString()}");
Log.Message($"{shooter.Name} did this when? age {socialThought.Age}, tick {socialThought.Tick}, timestamp {socialThought.Timestamp}, in-game {socialThought.GetTipString()}");
*/


//!!!!!
// Find.TaleManager.Notify_PawnDiscarded(this, silentlyRemoveReferences);
// Find.QuestManager.Notify_PawnDiscarded(this);

//FIND.
/*
Find.HistoryEventsManager.colonistEvents (private) ->  Find.HistoryEventsManager.GetLastTicksGame()

Find.StoryWatcher.statsRecord ???
*/


// LudeonTK
// 
// Types:
// 
// DebugActionAttribute
// DebugActionButtonResult
// DebugActionNode
// DebugActionType
// DebugActionYielderAttribute
// DebugHistogram
// DebugLogsUtility
// DebugMenuOption
// DebugMenuOptionMode
// DebugOutputAttribute
// DebugTables
// DebugTabMenu
// DebugTabMenu_Actions
// DebugTabMenu_Output
// DebugTabMenu_Settings
// DebugTool
// DebugTools
// DevGUI
// Dialog_Debug

//colonist.pawn.jobs.debugLog = true;

//DebugInputLogger
