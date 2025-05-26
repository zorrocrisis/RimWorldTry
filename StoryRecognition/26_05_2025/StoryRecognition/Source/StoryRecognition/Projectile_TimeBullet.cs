using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StoryRecognition
{
    public class Projectile_TimeBullet : Bullet
    {
        #region Properties
        //
        public ThingDef_TimeBullet Def
        {
            get
            {
                return this.def as ThingDef_TimeBullet;
            }
        }
        #endregion Properties

        #region Overrides

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing);

            /*
             * Null checking is very important in RimWorld.
             * 99% of errors reported are from NullReferenceExceptions (NREs).
             * Make sure your code checks if things actually exist, before they
             * try to use the code that belongs to said things.
             */
            if (Def != null && hitThing != null && hitThing is Pawn hitPawn) //Fancy way to declare a variable inside an if statement. - Thanks Erdelf.
            {
                var rand = Rand.Value; // This is a random percentage between 0% and 100%
                if (rand <= Def.AddHediffChance) // If the percentage falls under the chance, success!
                {
                    /*
                     * Messages.Message flashes a message on the top of the screen.
                     * You may be familiar with this one when a colonist dies, because
                     * it makes a negative sound and mentioneds "So and so has died of _____".
                     *
                     * Here, we're using the "Translate" function. More on that later in
                     * the localization section.
                     */

                    /*
                    Messages.Message("Time checked successfully: ".Translate(new object[] {
                        Utils.CurrentDate(), Utils.CurrentDate()
                    }), MessageTypeDefOf.NegativeHealthEvent);
                    */
                    string text = "Time checked successfully: " + Utils.CurrentDate() + " " + Utils.CurrentHour() + " " + Utils.CurrentGameStartTick();
                    Messages.Message(new Message(text, MessageTypeDefOf.NegativeHealthEvent));


                    // ELIMINATE, WIP
                    //GetEventLog();

                    LogGatherer socialLogExample = new LogGatherer();
                    socialLogExample.LogEverything();

                }
                else //failure!
                {
                    /*
                     * Motes handle all the smaller visual effects in RimWorld.
                     * Dust plumes, symbol bubbles, and text messages floating next to characters.
                     * This mote makes a small text message next to the character.
                     */
                    MoteMaker.ThrowText(hitThing.PositionHeld.ToVector3(), hitThing.MapHeld, "T_TimeBullet_FailureMote".Translate(Def.AddHediffChance), 12f);

                }
            }

        }
        #endregion Overrides


        public void GetEventLog()
        {
            // Get the type of the Log class from the RimWorld assembly.
            Type logType = typeof(Log);

            // Retrieve the private field or method that stores the event log.
            FieldInfo eventLogField = logType.GetField("entries", BindingFlags.NonPublic | BindingFlags.Static);

            if (eventLogField != null)
            {
                var logEntries = eventLogField.GetValue(null); // This will give you the actual list of log entries.
                Log.Message("Accessed Log Entries!");

                // If it's a list or collection, you can iterate over it and print out the log contents.
                if (logEntries is System.Collections.IEnumerable entries)
                {
                    foreach (var entry in entries)
                    {
                        Log.Message(entry.ToString());
                    }
                }
            }
            else
            {
                Log.Error("Could not access the event log.");
            }
        }

    }

}
