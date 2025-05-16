using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Storyteller
{
    public class SocialLogExample
    {

        public void LogSocialInteractions(Pawn hitpawn, Projectile_TimeBullet projectile)
        {
            // Ensure the pawn has a social tracker
            if (hitpawn != null)
            {
                //var socialTracker = pawn.needs?.mood?.thoughts?.GetSocialThoughts();

                List<ColonistBar.Entry> colonists = Find.ColonistBar.Entries;

                //GetPlayLogEntries(colonists);

                GetBattleLogEntries(colonists);


                
            }
        }

        private void GetPlayLogEntries(List<ColonistBar.Entry> colonists)
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
                    foreach (var colonist in colonists)
                    {
                        // Check if interaction string contains colonist's nickname
                        if (briefInteractionString.Contains(colonist.pawn.Name.ToString().Split('\'')[1]))
                        {
                            participantPawn = colonist.pawn;
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

        private void GetBattleLogEntries(List<ColonistBar.Entry> colonists)
        {
            List<Battle> battleEntries = Find.BattleLog.Battles;

            if (battleEntries != null)
            {
                // Log recent social interactions or relationships
                foreach (var battleEntry in battleEntries)
                {

                    List<LogEntry> battleLogEntries = battleEntry.Entries;

                    Log.Message($"Battle name: {battleEntry.GetName()}, with unique ID ({battleEntry.GetUniqueLoadID()})");

                    foreach (var battleLogEntry in battleLogEntries)
                    {
                        string briefbattleString = battleLogEntry.ToString();
                        Pawn participantPawn = null;

                        // Fetch one of the colonists who participated in the event log
                        foreach (var colonist in colonists)
                        {
                            // Check if interaction string contains colonist's nickname
                            if (briefbattleString.Contains(colonist.pawn.Name.ToString().Split('\'')[1]))
                            {
                                participantPawn = colonist.pawn;
                                break;
                            }
                        }

                        // If we managed to fetch a participating colonist, log relevant information
                        if (participantPawn != null)
                        {
                            // The first line does not log without a participating pawn
                            Log.Message($"Full interaction: {battleLogEntry.ToGameStringFromPOV(participantPawn)}");
                            Log.Message($"Logid {battleLogEntry.LogID}");
                            Log.Message($"What? {briefbattleString}");
                            Log.Message($"When? age {battleLogEntry.Age}, tick {battleLogEntry.Tick}, timestamp {battleLogEntry.Timestamp}, in-game {battleLogEntry.GetTipString()}");
                        }
                        else
                        {
                            Log.Message("Something went wrong with the battle logs!");
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


// GETS SOCIAL LOGS OF SHOOTER
/*
List<LogEntry> le = Find.PlayLog.AllEntries;
Log.Message($"{shooter.Name}, full interaction: {socialThought.ToGameStringFromPOV(projectile.Launcher)}");
Log.Message($"{shooter.Name} interacted with logid {socialThought.LogID}");
Log.Message($"{shooter.Name} did what? {socialThought.ToString()}");
Log.Message($"{shooter.Name} did this when? age {socialThought.Age}, tick {socialThought.Tick}, timestamp {socialThought.Timestamp}, in-game {socialThought.GetTipString()}");
*/


//!!!!!
// Find.BattleLog.Notify_PawnDiscarded(this, silentlyRemoveReferences);
// Find.TaleManager.Notify_PawnDiscarded(this, silentlyRemoveReferences);
// Find.QuestManager.Notify_PawnDiscarded(this);

//FIND.
/*
Find.HistoryEventsManager.colonistEvents (private) ->  Find.HistoryEventsManager.GetLastTicksGame()

Find.StoryWatcher.statsRecord ???
Find.TaleManager.AllTalesListForReading ???

public static PlaySettings PlaySettings => Current.Game.playSettings;

public static Storyteller Storyteller => Current.Game?.storyteller;
public static RelationshipRecords RelationshipRecords => Current.Game.relationshipRecords;
*/
