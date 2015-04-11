using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

public class CustomEngineSounds : PartModule
{
	[KSPField]
	public string running;
	[KSPField]
	public string engage;
	[KSPField]
	public string disengage;
	[KSPField]
	public string flameout;
	[KSPField]
	public bool runningConstantly = false;


	public FXGroup runningGroup = null;
	public bool runningSet;
	//private AudioLowPassFilter runningLowPass;
	public FXGroup engageGroup = null;
	public bool engageSet;
	public FXGroup disengageGroup = null;
	public bool disengageSet;
	public FXGroup flameoutGroup = null;
	public bool flameoutSet;

	private bool prevEngaged;
	private bool flameoutOccurred;

	private ModuleEngines engine;
	private float engineMaxThrust;

	public override void OnStart(StartState state)
	{
		if (state == StartState.Editor || state == StartState.None) return;

		runningSet = CreateGroup (runningGroup, running, true);
		engageSet = CreateGroup (engageGroup, engage, false);
		disengageSet = CreateGroup (disengageGroup, disengage, false);
		flameoutSet = CreateGroup (flameoutGroup, flameout, false);

		engine = part.GetComponent<ModuleEngines>();

		if (engine != null) {
			engineMaxThrust = engine.maxThrust;
			prevEngaged = engine.getIgnitionState; // set engine toggle
			flameoutOccurred = false;
		}
		// Add events to stop the sound when paused.
		// Make sure to remove these if the part is destroyed to prevent a memory leak.
		GameEvents.onGamePause.Add(new EventVoid.OnEvent(this.OnPause));
		GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(this.OnUnPause));

		base.OnStart(state);
	}

	void OnPause()
	{
		runningGroup.audio.mute = true;
	}

	void OnUnPause()
	{
		runningGroup.audio.mute = false;
	}

	void OnDestroy()
	{
		// Remove the events when the part is destroyed to prevent a memory leak.
		GameEvents.onGamePause.Remove(new EventVoid.OnEvent(OnPause));
		GameEvents.onGameUnpause.Remove(new EventVoid.OnEvent(OnUnPause));
	}

	public override void OnUpdate()
	{
		base.OnUpdate();

		if (engine != null) {
			float thrustRatio = (engine.finalThrust / engineMaxThrust); // Essentially, the current throttle based on thrust

			// Engage/Disengage Sounds
			if (engageSet || disengageSet) { // if engageGroup or disengageGroup is set, play sounds
				if (engine.getIgnitionState != prevEngaged) { // if the engine was toggled on or off
					if (engageSet && engine.getIgnitionState) // if toggled to on play the engage sound if set
						engageGroup.audio.Play ();
					if (disengageSet && !engine.getIgnitionState) // if toggled to off, play the disengage sound if set
						disengageGroup.audio.Play ();
					prevEngaged = engine.getIgnitionState; // reset prevEngaged to match the ignition state
				}
			}

			// Runnning Sounds (KSP power sound)
			if (runningSet) { // if runningGroup is set, check to see if sound gets played. If not, do nothing.
				if (engine.getIgnitionState && !engine.getFlameoutState && thrustRatio > 0f) { // If the engine is actually producing thrust, i.e. started, not flamed out, and not at zero throttle
					// Set the volume and pitch of the clip based on power	
					runningGroup.audio.volume = GameSettings.SHIP_VOLUME * Mathf.Lerp (0.33f, 1f, thrustRatio);
					runningGroup.audio.pitch = Mathf.Lerp (0.2f, 1f, thrustRatio);
					if (!runningGroup.audio.isPlaying)
						runningGroup.audio.Play (); // Play the clip if it isn't already
				} else { // Engine is not actively running (i.e. not engaged, flameout, zero throttle)
					if (runningGroup.audio.isPlaying && !runningConstantly) // if engine sound is playing and we are not a constantly running engine
						runningGroup.audio.Stop (); // Turn off the clip if needed
				}
			}

			// Flameout Sounds
			if (flameoutSet) { // if flamout sound is set
				if (engine.getFlameoutState && !flameoutOccurred) { // if the engine flames out and it was previously running
					flameoutGroup.audio.Play (); // play the flamout sound
					flameoutOccurred = true; // mark as flameout occurred
				}
				if (!engine.getFlameoutState && flameoutOccurred) // if the engine is not in flameout and one has occurred before
					flameoutOccurred = false; // reset the flameout occurrance
			}
		}
	}

	public bool CreateGroup(FXGroup group, string clip, bool loop) {
		if (!GameDatabase.Instance.ExistsAudioClip (clip)) { // check to see if clip exists, if not return that it is not set
			Debug.LogError ("KSPCustomSounds - Engines: Audio file not found: " + clip);
			return false;
		}

		group.audio = gameObject.AddComponent<AudioSource>();
		group.audio.clip = GameDatabase.Instance.GetAudioClip(clip);
		group.audio.Stop();
		group.audio.volume = GameSettings.SHIP_VOLUME;
		group.audio.pitch = 1f;
		group.audio.rolloffMode = AudioRolloffMode.Linear;
		group.audio.maxDistance = 10000f; // distance about the same as stock
		group.audio.loop = loop;
		group.audio.playOnAwake = false;
		group.audio.dopplerLevel = 0f; // if set to 1, doppler effect happens on camera movements. funny noises.
		group.audio.panLevel = 1f;

		Debug.Log ("KSPCustomSounds - Engines: Custom audio group created.");

		/*if (filter) {
			filter = group.audio.GetComponent<AudioLowPassFilter> ();
			Debug.Log ("KSPCustomSounds - Engines: Filter added to group.");
		}*/

		return true; // return that group has been set
	}

}
