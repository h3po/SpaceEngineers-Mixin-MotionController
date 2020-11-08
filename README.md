# SpaceEngineers-Mixin-MotionController
This is WIP. I'd like to add smooth acceleration control at some point; right now this simply sets Rotor/Piston velocity in the correct direction to reach a set target position and stop there by setting velocity to 0 without touching the Block's Limit settings.
The Update() function dynamically determines which update frequency is needed and uses the slowest possible one.

## Minimal example
```C#
partial class Program : MyGridProgram
{
	MotionController motion;
	UpdateFrequency motionUpdateFrequency;

	public Program()
	{
		//IMyPistonBase piston = (IMyPistonBase) GridTerminalSystem.GetBlockWithName("Piston");
		//motion = new PistonMotionController(piston, this);

		IMyMotorStator rotor = (IMyMotorStator) GridTerminalSystem.GetBlockWithName("Rotor");
		motion = new RotorMotionController(rotor, this);

		//make the motion controller echo debug stuff
		motion.EnableDebug();
	}

	public void Main(string argument, UpdateType updateType)
	{
		const UpdateType tickUpdateTypes = (UpdateType.Once | UpdateType.Update1 | UpdateType.Update10 | UpdateType.Update100);
		const UpdateType externalUpdateTypes = (UpdateType.IGC | UpdateType.Mod | UpdateType.Script | UpdateType.Terminal | UpdateType.Trigger);
		
		argument = argument.ToLower();

		//update the motion controller
		if ((updateType & tickUpdateTypes) != 0)
		{
			motionUpdateFrequency = motion.Update(updateType);
		}

		//set a target for the motion controller
		if ((updateType & externalUpdateTypes) != 0)
		{
			float targetPos;
			if (float.TryParse(argument, out targetPos))
				motionUpdateFrequency = motion.SetTarget(targetPos);
		}

		//set the update frequency to the value requested by the motion controller
		Runtime.UpdateFrequency = motionUpdateFrequency;
	}
}
```
