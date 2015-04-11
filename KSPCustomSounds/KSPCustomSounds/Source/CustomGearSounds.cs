using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

public class ModuleLandingGearSounds : PartModule {

	[KSPField]
	public string gearDeploySound;
	[KSPField]
	public string gearRetractSound;

	public FXGroup gearDeploy;
	public bool gearDeploySet;
	public FXGroup gearRetract;
	public bool gearRetractSet;
	//private bool gearToggled;
	private ModuleLandingGear.GearStates stateCheck;

	private string customSoundsConfig;

	private ModuleLandingGear gear;

	public override void OnAwake ()
	{
		//Pull default sound files from the sounds.cfg file if individual ones don't exist
		customSoundsConfig = IOUtils.GetFilePathFor(typeof(ModuleLandingGearSounds), "sounds.cfg");
		ConfigNode node = ConfigNode.Load(customSoundsConfig);
		if (gearDeploySound == null) {
			gearDeploySound = node.GetValue ("gearSoundDefault");
		}
		if (gearRetractSound == null) {
			gearRetractSound = node.GetValue ("gearSoundDefault");
		}

		base.OnAwake ();
	}

	public override void OnStart(StartState state){
		if (state == StartState.Editor || state == StartState.None) return;

		//Set FXGroups for each sound (deploy and retract)
		gearDeploySet = CreateGearGroup (gearDeploy, gearDeploySound, false);
		gearRetractSet = CreateGearGroup (gearRetract, gearRetractSound, false);

		//Get gear module to compare later
		gear = part.GetComponent<ModuleLandingGear> ();
		stateCheck = gear.gearState;

		base.OnStart (state);
	}

	public override void OnUpdate(){
		// if our sound is set
		if (gear.gearState != stateCheck) { // if the state has changed this update
			if (gear.gearState == ModuleLandingGear.GearStates.DEPLOYING && gearDeploySet) { // if the state is deploying and our sound is set
				Debug.Log ("KSPCustomSounds - LandingGear: gear deploying, fire sound. " + gear.gearState.ToString ());
				gearDeploy.audio.Play (); // Play deploying sound
			} else if (gear.gearState == ModuleLandingGear.GearStates.RETRACTING && gearRetractSet) {
				Debug.Log ("KSPCustomSounds - LandingGear: gear retracting, fire sound. " + gear.gearState.ToString ());
				gearRetract.audio.Play (); // Play retracting sound
			}
			stateCheck = gear.gearState; // match states for next update
		}


		base.OnUpdate ();
	}

	public bool CreateGearGroup(FXGroup group, string clip, bool loop) {
		if (!GameDatabase.Instance.ExistsAudioClip (clip)) { // check to see if clip exists, if not return that it is not set
			Debug.LogError ("KSPCustomSounds - LandingGear: Audio file not found: " + clip);
			return false;
		}

		group.audio = gameObject.AddComponent<AudioSource>();
		group.audio.clip = GameDatabase.Instance.GetAudioClip(clip);
		group.audio.Stop();
		group.audio.volume = GameSettings.SHIP_VOLUME * 0.7f;
		group.audio.pitch = 1f;
		group.audio.rolloffMode = AudioRolloffMode.Linear;
		group.audio.maxDistance = 10000f; // distance about the same as stock
		group.audio.loop = loop;
		group.audio.playOnAwake = false;
		group.audio.dopplerLevel = 0f; // if set to 1, doppler effect happens on camera movements. funny noises.
		group.audio.panLevel = 1f;

		Debug.Log ("KSPCustomSounds - LandingGear: Custom audio group created.");

		return true; // return that group has been set
	}

}