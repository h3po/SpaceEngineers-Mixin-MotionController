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
        public class PistonMotionController : MotionController
        {
            public float setSpeed = 0.5f;   //[m/s]
            public float setTarget = -1;
            public bool truncatePosition = true;
            public IMyPistonBase piston;

            public PistonMotionController(IMyPistonBase piston, Program program) : base(program)
            {
                this.piston = piston;
            }

            public override UpdateFrequency SetTarget(float target)
            {
                if ((target <= piston.MinLimit) || (target >= piston.MaxLimit))
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
                        target = (target < piston.MinLimit) ? piston.MinLimit : (target > piston.MaxLimit) ? piston.MaxLimit : target;
                    }
                }
                    
                setTarget = target;
                Echo($"Piston target position set to {target:0.#}");
                requiredFrequency = UpdateFrequency.Once;

                return requiredFrequency;
            }

            public override UpdateFrequency Update(UpdateType updateType)
            {
                if ((setTarget >= 0) && (UpdateTypeMatchesFrequency(updateType, requiredFrequency)))
                {
                    //always positive
                    float distanceFromTargetPosition = Math.Max(piston.CurrentPosition, setTarget) - Math.Min(piston.CurrentPosition, setTarget);
                    float moveDirection = Math.Sign(setTarget - piston.CurrentPosition);
                    float signedSpeed = setSpeed * moveDirection;
                    float timeToTargetPosition = distanceFromTargetPosition / setSpeed;
                    
                    Echo($"Piston has {distanceFromTargetPosition:0.#} m to go, {timeToTargetPosition:0.#}s at {setSpeed:0.#}m/s");

                    //less than 1 tick remaining, call it done
                    if (timeToTargetPosition < (1f / 60))
                    {
                        piston.Velocity = 0;
                        Echo("Piston done");
                        requiredFrequency = UpdateFrequency.None;
                    }
                    else
                    {
                        piston.Velocity = signedSpeed;

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
