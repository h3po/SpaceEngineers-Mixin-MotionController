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
        public class PistonMotionController
        {
            IMyPistonBase myPiston;
            float speed = 0.5f;
            float setPosition = -1;

            IEnumerator<bool> myStateMachine;
            UpdateFrequency requiredFrequency;
            Program parentProgram;

            public PistonMotionController(IMyPistonBase piston, Program program)
            {
                myPiston = piston;
                parentProgram = program;
                myStateMachine = StateMachine();
            }

            public UpdateFrequency SetTarget(float position)
            {
                if ((position <= myPiston.MinLimit) || (position >= myPiston.MaxLimit))
                {
                    parentProgram.Echo("Piston target out of limits");
                    requiredFrequency = UpdateFrequency.None;
                }
                    
                setPosition = position;
                parentProgram.Echo($"Piston target set to {position:0.#}");
                requiredFrequency = UpdateFrequency.Once;

                return requiredFrequency;
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

            public UpdateFrequency Update(UpdateType updateType)
            {
                if ((setPosition >= 0) && (UpdateTypeMatchesFrequency(updateType, requiredFrequency)))
                {
                    myStateMachine.MoveNext();
                }

                return requiredFrequency;
            }

            public IEnumerator<bool> StateMachine()
            {
                while (true) {
                    //always positive
                    float distanceFromTargetPosition = Math.Max(myPiston.CurrentPosition, setPosition) - Math.Min(myPiston.CurrentPosition, setPosition);
                    float signedSpeed = speed * Math.Sign(setPosition - myPiston.CurrentPosition);
                    float timeToTargetPosition = distanceFromTargetPosition / speed;

                    parentProgram.Echo($"Piston has {distanceFromTargetPosition:0.#} m to go, {timeToTargetPosition:0.#}s at {speed:0.#}m/s");

                    //less than 1 tick remaining, call it done
                    if (timeToTargetPosition < (1f/60)) {
                        myPiston.Velocity = 0;
                        parentProgram.Echo("Piston done");
                        requiredFrequency = UpdateFrequency.None;
                        yield return false;
                    }
                    else
                    {
                        myPiston.Velocity = signedSpeed;

                        //less than 10 ticks remaining
                        if (timeToTargetPosition < (1f / 6))
                        {
                            parentProgram.Echo("Switching to Update1");
                            requiredFrequency = UpdateFrequency.Update1;
                        }
                        //less than 100 ticks remaining
                        else if (timeToTargetPosition < (1f / 0.6))
                        {
                            parentProgram.Echo("Switching to Update10");
                            requiredFrequency = UpdateFrequency.Update10;
                        }
                        //more than 99 ticks remaining
                        else
                        {
                            parentProgram.Echo("Switching to Update100");
                            requiredFrequency = UpdateFrequency.Update10;
                        }

                        yield return true;
                    }
                }
            }
        }
    }
}
