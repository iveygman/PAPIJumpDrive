using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class CustomInternalSounds : MonoBehaviour {

	private string customSoundsConfig;

	public bool enableIVA;
	private string backgroundLoop;
	private string switchSound;
	private	string beepSound;
	private string servoSound;
	private float minCooldown;
	private float maxCooldown;

	public FXGroup background;
	public bool backgroundSet;
	public FXGroup switchGroup;
	public bool switchGroupSet;
	public FXGroup beepGroup;
	public bool beepGroupSet;
	public FXGroup servoGroup;
	public bool servoGroupSet;

	private Vessel vessel;
	//private float nextFire;
	private float switchCooldown = 0f;
	private float beepCooldown = 0f;
	private float servoCooldown = 0f;
	//private float currentCooldown;
	private System.Random random = new System.Random ();

	void Awake() {
		//Pull default sound files from the sounds.cfg file if individual ones don't exist
		customSoundsConfig = IOUtils.GetFilePathFor(typeof(CustomInternalSounds), "sounds.cfg");
		ConfigNode node = ConfigNode.Load(customSoundsConfig);
		enableIVA = bool.Parse (node.GetValue ("enableIVA"));
		backgroundLoop = node.GetValue ("backgroundIVA");
		switchSound = node.GetValue ("switchIVA");
		beepSound = node.GetValue ("beepIVA");
		servoSound = node.GetValue ("servoIVA");
		minCooldown = float.Parse (node.GetValue ("minIVACooldown"));
		maxCooldown = float.Parse (node.GetValue ("maxIVACooldown"));
	}

	void Start() {
		if (!enableIVA)
			return; // if IVA sound is disabled, back out
		background = new FXGroup ("IVABackground");
		switchGroup = new FXGroup ("IVASwitch");
		vessel = FlightGlobals.ActiveVessel;

		switchCooldown = Mathf.Lerp (minCooldown, maxCooldown, (float)random.NextDouble ());
		beepCooldown = Mathf.Lerp (minCooldown, maxCooldown, (float)random.NextDouble ());
		servoCooldown = Mathf.Lerp (minCooldown, maxCooldown, (float)random.NextDouble ());

		backgroundSet = CreateInternalGroup (background, vessel, backgroundLoop, true);
		switchGroupSet = CreateInternalGroup (switchGroup, vessel, switchSound, false);
		beepGroupSet = CreateInternalGroup (beepGroup, vessel, beepSound, false);
		servoGroupSet = CreateInternalGroup (servoGroup, vessel, servoSound, false);

	}

	void Update() {
		if (!enableIVA)
			return; // if IVA sound is disabled, back out
		if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA) { // if we're in IVA
			// update the countdowns
			if (switchGroupSet)
				switchCooldown -= Time.deltaTime;
			if (beepGroupSet)
				beepCooldown -= Time.deltaTime;
			if (servoGroupSet)
				servoCooldown -= Time.deltaTime;
			if (!background.audio.isPlaying) { // Start the background audio if needed
				background.audio.Play ();
				Debug.Log ("KSPCustomSounds - IVASound: Background IVA loop launched.");
			}
			if (switchCooldown <= 0f) { // if the countdown hits zero
				Debug.Log ("KSPCustomSounds - IVASound: Random switch sound firing.");
				switchGroup.audio.Play (); // play the switch sound
				switchCooldown = Mathf.Lerp (minCooldown, maxCooldown, (float)random.NextDouble ()); // Randomly get a new countdown
				Debug.Log ("KSPCustomSounds - IVASound: Next cooldown cycle is " + switchCooldown.ToString() + " seconds.");
			}
			if (beepCooldown <= 0f) { // if the countdown hits zero
				Debug.Log ("KSPCustomSounds - IVASound: Random beep sound firing.");
				beepGroup.audio.Play (); // play the switch sound
				beepCooldown = Mathf.Lerp (minCooldown, maxCooldown, (float)random.NextDouble ()); // Randomly get a new countdown
				Debug.Log ("KSPCustomSounds - IVASound: Next cooldown cycle is " + beepCooldown.ToString() + " seconds.");
			}
			if (servoCooldown <= 0f) { // if the countdown hits zero
				Debug.Log ("KSPCustomSounds - IVASound: Random switch sound firing.");
				servoGroup.audio.Play (); // play the switch sound
				servoCooldown = Mathf.Lerp (minCooldown, maxCooldown, (float)random.NextDouble ()); // Randomly get a new countdown
				Debug.Log ("KSPCustomSounds - IVASound: Next cooldown cycle is " + servoCooldown.ToString() + " seconds.");
			}
		} else {
			background.audio.Stop ();
			switchGroup.audio.Stop();
		}
	}

	void Destroy() {
		// null out the FXGroups
		background = null;
		switchGroup = null;
		beepGroup = null;
		servoGroup = null;
	}

	public bool CreateInternalGroup(FXGroup group, Vessel vessel, string clip, bool loop) {
		if (!GameDatabase.Instance.ExistsAudioClip (clip)) { // check to see if clip exists, if not return that it is not set
			Debug.LogError ("KSPCustomSounds - IVASound: Audio file not found: " + clip);
			return false;
		}

		group.audio = vessel.gameObject.AddComponent<AudioSource>();
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

		Debug.Log ("KSPCustomSounds - IVASound: Custom audio group created.");

		return true; // return that group has been set
	}

}
