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
        public class RotorMotionController : MotionController
        {
            public float setSpeed = MathHelper.RPMToRadiansPerSecond * 1f;  //[rad/s]
            public float setTarget = float.NaN;                             //rad
            public bool truncatePosition = false;
            IMyMotorStator myRotor;

            public RotorMotionController(IMyMotorStator rotor, Program program) : base(program)
            {
                myRotor = rotor;
            }

            public override UpdateFrequency SetTarget(float targetDegrees)
            {
                return SetTargetRadians(MathHelper.ToRadians(targetDegrees));
            }

            public UpdateFrequency SetTargetRadians(float targetRadians)
            {
                if ((targetRadians <= myRotor.LowerLimitRad) || (targetRadians >= myRotor.UpperLimitRad))
                {
                    if (!truncatePosition)
                    {
                        Echo("Rotor target position out of limits");
                        requiredFrequency = UpdateFrequency.None;
                        return requiredFrequency;
                    }
                    else
                    {
                        Echo("Truncating target position to rotor limits");
                        targetRadians = (targetRadians < myRotor.LowerLimitRad) ? myRotor.LowerLimitRad : (targetRadians > myRotor.UpperLimitRad) ? myRotor.UpperLimitRad : targetRadians;
                    }
                }
                else if ((truncatePosition) && (Math.Abs(targetRadians) > MathHelper.TwoPi))
                {
                    Echo("Truncating target position to within 360°");
                    MathHelper.LimitRadians(ref targetRadians);
                }

                setTarget = targetRadians;
                Echo($"Rotor target position set to {MathHelper.ToDegrees(targetRadians):0.#}°");
                requiredFrequency = UpdateFrequency.Once;

                return requiredFrequency;
            }

            public override UpdateFrequency Update(UpdateType updateType)
            {
                if ((setTarget != float.NaN) && (UpdateTypeMatchesFrequency(updateType, requiredFrequency)))
                {
                    float radiansFromTargetPosition = Math.Max(myRotor.Angle, setTarget) - Math.Min(myRotor.Angle, setTarget);
                    float moveDirection = Math.Sign(setTarget - myRotor.Angle);
                    float signedSpeed = setSpeed * moveDirection;
                    float timeToTargetPosition = radiansFromTargetPosition / setSpeed;

                    Echo($"Rotor has {MathHelper.ToDegrees(radiansFromTargetPosition):0.#}° to go, {timeToTargetPosition:0.#}s at {setSpeed * MathHelper.RadiansPerSecondToRPM:0.#}rpm");

                    //less than 1 tick remaining, call it done
                    if (timeToTargetPosition < (1f / 60))
                    {
                        myRotor.TargetVelocityRad = 0;
                        myRotor.RotorLock = true;
                        Echo("Rotor done");
                        requiredFrequency = UpdateFrequency.None;
                    }
                    else
                    {
                        myRotor.RotorLock = false;
                        myRotor.TargetVelocityRad = signedSpeed;

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
