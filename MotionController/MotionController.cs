using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public abstract class MotionController
        {
            private Program parentProgram;
            public Action<string> Echo = text => { };
            public UpdateFrequency requiredFrequency = UpdateFrequency.None;

            public MotionController(Program program)
            {
                parentProgram = program;
            }

            public void EnableDebug()
            {
                Echo = parentProgram.Echo;
            }

            public bool UpdateTypeMatchesFrequency(UpdateType type, UpdateFrequency frequency)
            {
                if (((frequency == UpdateFrequency.Once) && ((type & UpdateType.Once) != 0)) ||
                    ((frequency == UpdateFrequency.Update1) && ((type & UpdateType.Update1) != 0)) ||
                    ((frequency == UpdateFrequency.Update10) && ((type & UpdateType.Update10) != 0)) ||
                    ((frequency == UpdateFrequency.Update100) && ((type & UpdateType.Update100) != 0)))
                {
                    return true;
                }

                return false;
            }

            public float UpdateFrequencyToInterval(UpdateFrequency frequency)
            {
                switch (frequency)
                {
                    case UpdateFrequency.Once:
                    case UpdateFrequency.Update1: return 1f / 60;
                    case UpdateFrequency.Update10: return 1f / 6;
                    case UpdateFrequency.Update100: return 1f / 0.6f;
                }
                return 0;
            }

            public abstract UpdateFrequency SetTarget(float target);
            public abstract UpdateFrequency Update(UpdateType updateType);
        }
    }
}
