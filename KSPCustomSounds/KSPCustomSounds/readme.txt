KSPCustomSounds plugin by Raptor831

This is a plugin designed to let you choose custom sounds for certain elements in-game. First, there is a module that can define custom engine sounds for new engine parts. It can also replace stock parts with the ModuleManager mod and an appropriate .cfg file.

Second is a module that adds a sound to landing gear when they are toggled. With the included ModuleManager and MM Extension, any part using the ModuleLandingGear module will have this automatically added. The file can be defined per part or as a global default in the sound.cfg file included.

Third, an option for IVA pod sounds is included. The sounds are defined in the sound.cfg file, and will play whenever you are in IVA mode. One background sound will loop (like a fan or background hum) and up to three defined sounds will play at random intervals (i.e. switches, motors, beeps, computer noises, etc.).

This mod does not provide any custom audio at this time, although it may in the future.

ENGINES

KSPCustomSounds creates a new module, CustomEngineSounds, that can be added to an engine part. This module has options for each of the four sounds normally attached to an engine.

running -> for the normal engine sound while running
engage -> sound when turned on
disengage -> sound when turned off
flameout -> sound when a flameout occurs

So, for example, in your part.cfg you would have:

MODULE
{
	name = CustomEngineSounds
	running = KSPCustomSounds/Sounds/sound_file1
	engage = KSPCustomSounds/Sounds/sound_file2
	disengage = KSPCustomSounds/Sounds/sound_file3
	flameout = KSPCustomSounds/Sounds/sound_file4
}

This way, all custom engine sounds can be placed in a single folder and loaded once into memory. Also, it makes replacing sounds much quicker.

To replace a stock sound with ModuleManager, say for the LV-909, you would need to have a .cfg file with the following:

@PART[liquidEngine3]
{
	!sound_vent_medium = DELETE
	!sound_rocket_hard = DELETE
	!sound_vent_soft = DELETE
	!sound_explosion_low = DELETE
	
	MODULE
	{
		name = CustomEngineSounds
		running = KSPCustomSounds/Sounds/sound_file1
		engage = KSPCustomSounds/Sounds/sound_file2
		disengage = KSPCustomSounds/Sounds/sound_file3
		flameout = KSPCustomSounds/Sounds/sound_file4
	}
}

This will probably be different for every single part, so pay attention to the items you delete.

You also don’t have to switch them all. You can use both stock sounds and custom sounds on the same part.  You just need to be sure that you delete the comparable stock sound if you use a custom sound, otherwise both sounds will run at the same time.

LANDING GEAR

The included ModuleManager/MMExtension file will add the appropriate module automatically. To change the sound referenced, go to <KSP>/GameData/KSPCustomSounds/PluginData/KSPCustomSound/sound.cfg and replace the sound currently there for “gearSoundDefault”.

You can also add a custom sounds per part if you like. Both the deploy and the retract actions have specific sounds, so you can reference them from anywhere inside /GameData/.

Example:

MODULE
{
	name = ModuleLandingGearSounds
	gearDeploy = KSPCustomSounds/Sound/sound_file1
	gearRetract = KSPCustomSounds/Sound/sound_file2
}

INTERNAL IVA SOUND

This option will be enabled automatically. Once you go into IVA view, the pod noises will commence. You can define these files in the <KSP>/GameData/KSPCustomSounds/PluginData/KSPCustomSound/sound.cfg file. The options are explained as follows:

enableIVA = turn the sounds on and off. true is on, false is off.
backgroundIVA = looping background sound that will constantly play while in IVA.
switchIVA = first random sound, intended to be the sound of switches.
beepIVA = second random sound, intended to be any beeping or computer noises.
servoIVA = third random sound, intended to be any mechanical items moving that could be heard (i.e. solar panels, intermittent fans, snack drawers).
minIVACooldown = minimum time between two sound FX of the same type (i.e. two switch sounds).
maxIVACooldown = maximum time between two sound FX of the same type.


KSPCustomSounds 

License:
CC BY-NC-SA 3.0
http://creativecommons.org/licenses/by-nc-sa/3.0/deed.en_US
Short version: You can use this code for any non-commercial project as long as you give me credit for this plugin.
Also, a short note saying you are using it wouldn’t hurt, but is not required.

Special thanks to Snjo and pizzaoverhead. Was able to learn a lot by their code so I had a good starting point. Would have been lost otherwise.

Installation
To install, place the KSPCustomSounds folder in your <KSP>/GameData/ folder.

If you wish to use the engine sounds on stock parts, or wish to use the landing gear module, add the ModuleManager.dll and the MMSarbianExt.dll into <KSP>/GameData/. If you have later versions of these, do not overwrite them.

Release Notes

v0.2
ADDED: Module for landing gear sounds.
ADDED: IVA background sounds.
ADDED: sound.cfg file for control of the landing gear and IVA modules.

v0.1
Initial dev release. Basic features completed, works with KSP 0.22. Probably works down to KSP 0.20, but untested.