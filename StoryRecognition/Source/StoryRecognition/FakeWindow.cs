using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace StoryRecognition
{
    public class FakeWindow: Window
    {
        int index = 0;

        LogGatherer socialLogExample = new LogGatherer();

        public override Vector2 InitialSize => new Vector2(0.0f, 0.0f);
        protected override float Margin => 0f;

        public FakeWindow()
        {

            doCloseButton = false;
            doCloseX = false;
            soundAppear = null;
            soundClose = null;
            closeOnClickedOutside = false;
            closeOnAccept = false;
            closeOnCancel = false;
            focusWhenOpened = false;
            preventCameraMotion = false;

        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if(index == 0)
            {
                Log.Message("Fake window opened...");

                DebugViewSettings.logInput = true;
                index = 1;
            }

            LogMessage lastLogMessage = Log.Messages.Last<LogMessage>();

            if (lastLogMessage != null)
            {

                if (lastLogMessage.text.Contains("type=KeyDown") && lastLogMessage.text.Contains("keyCode=F10"))
                {
                    Log.Message($"Pressed F10!");
                    socialLogExample.LogEverything();

                }
            }

        }

        public override void DoWindowContents(Rect inRect)
        {
            
        }
    }
}
