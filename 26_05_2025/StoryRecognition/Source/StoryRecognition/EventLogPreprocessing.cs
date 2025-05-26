using HarmonyLib;
using Mono.CompilerServices.SymbolWriter;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace StoryRecognition
{
    static internal class EventLogPreprocessing
    {
        static string logFilePath;
        static string formattedShortOutputFilePath;
        static string formattedCompleteOutputFilePath;

        
        static string reColorPattern = @"<color\=#([0-9A-Fa-f]{8})>"; // Matches _ID_ followed by letters and digits, ending with _


        static string formattedLogPattern = @"(.+)_Timestamp_(\d+)_Location_\((\-?\d+), (\-?\d+), (\-?\d+)\)_ID_([^\s_]+)_Actors_([^_]+)_FullInfo_(.+)";

        static string entityPattern = @"A(?:n)? ([\w\s]+?) \(([^)]+)\)";

        static List<LogEvent> loggedEvents;

        static List<string> tempLines;

        public static List<LogEvent> GetLogEvents(string filePath)
        {
            SetLogFilePath(filePath);
            SetFinalEventsFormat();

            return loggedEvents;
        }

        private static void SetLogFilePath(string filePath)
        {
            logFilePath = filePath;

            formattedCompleteOutputFilePath = logFilePath.Replace(".txt", "_complete.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(formattedCompleteOutputFilePath));

            formattedShortOutputFilePath = logFilePath.Replace(".txt", "_short.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(formattedShortOutputFilePath));
        }

        private static void SetFinalEventsFormat()
        {
            tempLines = new List<string>();
            loggedEvents = new List<LogEvent>();

            FormatLines();
            BuildEvents();
        }

        private static void FormatLines()
        {

            foreach (string line in File.ReadLines(logFilePath))
            {

                if (string.IsNullOrWhiteSpace(line))
                {
                    // Empty line: assume continuation and wait for next part
                    continue;
                }
                else
                {
                    // Continuation of previous non-empty line...
                    if (!line.Contains("_FullInfo_"))
                    {
                        string lastLine = tempLines.Last<string>();
                        tempLines.RemoveLast<string>();
                        tempLines.Add(lastLine + " " + line);
                    }
                    // Normal line
                    else
                    {
                        if (line.Contains("Copied to clipboard.") || line.Contains("Your latest stories will be written here..."))
                        {
                            continue;
                        }

                        //string newLine = Regex.Replace(line, reInitialIdentifiedPattern, "Timestamp_");
                        //newLine = Regex.Replace(newLine, reIDPattern, "_");
                        //newLine = Regex.Replace(newLine, "_FullInfo_", "_");
                        string newLine = Regex.Replace(line, reColorPattern, "");
                        newLine = Regex.Replace(newLine, "</color>", "");

                        tempLines.Add(newLine);

                    }

                }
            }

            // Write the sorted lines to output file
            File.WriteAllLines(formattedCompleteOutputFilePath, tempLines);
        }

        private static void BuildEvents()
        {
            // Reset list
            tempLines.Clear();

            foreach (string line in File.ReadLines(formattedCompleteOutputFilePath))
            {

                LogEvent currentLogEvent = ParseLogLine(line);
                loggedEvents.Add(currentLogEvent);
                tempLines.Add(currentLogEvent.ToString());
            }

            // Write the sorted lines to output file
            File.WriteAllLines(formattedShortOutputFilePath, tempLines);
        }

        public static LogEvent ParseLogLine(string line)
        {
            Match match = Regex.Match(line, formattedLogPattern);

            if (!match.Success)
            {
                return null;

            }

            string logType = match.Groups[1].Value;
            int timestamp = int.Parse(match.Groups[2].Value);
            int x = int.Parse(match.Groups[3].Value);
            int y = int.Parse(match.Groups[4].Value);
            int z = int.Parse(match.Groups[5].Value);
            string id = match.Groups[6].Value;
            string actorsRaw = match.Groups[7].Value;
            string description = match.Groups[8].Value.Trim();


            List<string> actors = new List<string>();

            if (actorsRaw != null)
            {
                actors = actorsRaw.Split(',').Select(a => a.Trim()).ToList();
            }

            return new LogEvent
            {
                LogNameType = logType,
                Timestamp = timestamp,
                Location = (x, y, z),
                ID = id,
                Description = description,
                Actors = actors
            };
        }

    }
}
