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
            public float setSpeed = 0.5f;   //[m/s]
            public bool truncatePosition = true;

            IMyPistonBase myPiston;
            float setPosition = -1;
            UpdateFrequency requiredFrequency;
            Program parentProgram;
            Action<string> Echo = text => {};

            public PistonMotionController(IMyPistonBase piston, Program program)
            {
                myPiston = piston;
                parentProgram = program;
            }

            public void EnableDebug()
            {
                Echo = parentProgram.Echo;
            }

            public UpdateFrequency SetTarget(float position)
            {
                if ((position <= myPiston.MinLimit) || (position >= myPiston.MaxLimit))
                {
                    if (!truncatePosition)
                    {
                        Echo("Piston target position out of limits");
                        requiredFrequency = UpdateFrequency.None;
                        return requiredFrequency;
                    }
                    else
                    {
                        Echo("Truncating target position");
                        position = (position < myPiston.MinLimit) ? myPiston.MinLimit : (position > myPiston.MaxLimit) ? myPiston.MaxLimit : position;
                    }
                }
                    
                setPosition = position;
                Echo($"Piston target position set to {position:0.#}");
                requiredFrequency = UpdateFrequency.Once;

                return requiredFrequency;
            }

            bool UpdateTypeMatchesFrequency(UpdateType type, UpdateFrequency frequency)
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
                    //always positive
                    float distanceFromTargetPosition = Math.Max(myPiston.CurrentPosition, setPosition) - Math.Min(myPiston.CurrentPosition, setPosition);
                    float moveDirection = Math.Sign(setPosition - myPiston.CurrentPosition);
                    float signedSpeed = setSpeed * moveDirection;
                    float timeToTargetPosition = distanceFromTargetPosition / setSpeed;
                    
                    Echo($"Piston has {distanceFromTargetPosition:0.#} m to go, {timeToTargetPosition:0.#}s at {setSpeed:0.#}m/s");

                    //less than 1 tick remaining, call it done
                    if (timeToTargetPosition < (1f / 60))
                    {
                        myPiston.Velocity = 0;
                        Echo("Piston done");
                        requiredFrequency = UpdateFrequency.None;
                    }
                    else
                    {
                        myPiston.Velocity = signedSpeed;

                        //less than 10 ticks remaining
                        if (timeToTargetPosition < (1f / 6))
                        {
                            Echo("Switching to Update1");
                            requiredFrequency = UpdateFrequency.Update1;
                        }
                        //less than 100 ticks remaining
                        else if (timeToTargetPosition < (1f / 0.6))
                        {
                            Echo("Switching to Update10");
                            requiredFrequency = UpdateFrequency.Update10;
                        }
                        //more than 99 ticks remaining
                        else
                        {
                            Echo("Switching to Update100");
                            requiredFrequency = UpdateFrequency.Update100;
                        }
                    }
                }

                return requiredFrequency;
            }
        }
    }
}
