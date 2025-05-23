﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;


namespace StoryRecognition
{
    public interface IEvent : IExposable
    {
        bool GetIsAnniversary();

        bool TryStartEvent();
        bool TryStartEvent(Map map);
        bool IsStillEvent();
        void EndEvent();
        string ShowInLog();
        Date Date();
    }
}
