using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace PAPIJumpDrive
{

	public class JumpCoordinates
	{
		public CelestialBody Target { get; set; }
		public float Altitude { get; set; }
		public double JumpInitiateTime { get; set; } // when the jump was initiated
		public Vector3d JumpCoord {get; set;}
	}

	public class PAPIJumpDrive : PartModule
	{
		[KSPField]
		public string fJumpSoundFile = "PiconAdvanced/Sounds/sound_bamf";
		[KSPField]
		public string fSpinupSoundFile = "PiconAdvanced/Sounds/sound_jump_spinup";
		public FXGroup fJumpSpinupSoundGroup = null;
		public FXGroup fJumpSoundGroup = null;

		private const int LIGHTSPEED = 299792458;
		private const int MINIMUM_ALTITUDE_FROM_BODY = 100000;	// must be at least 100km away
		private const float WINDOW_HEIGHT_PCT = 0.35f;
		private const float WINDOW_WIDTH_PCT = 0.35f;
		private const double JUMP_SPINUP_TIME_SECONDS = 60.0f;

		private StartState fState = 0;
		private GUIStyle fWindowStyle, fLabelStyle, fButtonStyle, fBoxStyle;
		private bool fHasInitStyles = false;
//		private Rect fWindowPosition;

		[KSPField(guiActive = true, guiName = "Jump Drive", guiActiveEditor = false)]
		// jump drive states
		private bool fShowWindow = false; 	// show jump menu?
		public string fStatus = "inactive";	// current module status
		private bool fTargetIsSet = false;	// do we have a destination?
		private ScreenMessage fJumpDelayMessage = null;
		private bool fIsJumping = false;	// is the device currently in a jump?
		private string fPersistentXCoord = "0.0";
		private string fPersistentYCoord = "0.0";
		private string fPersistentZCoord = "0.0";
//		private string fPersistentXCoord = "0.0";
		private JumpCoordinates fJumpParams = new JumpCoordinates();
		[KSPEvent(guiActive = true, active = true, guiName = "Jump!", guiActiveEditor = false, guiActiveUnfocused = true)]
		public void toggleJumping() {
			fIsJumping = !fIsJumping;
			if (fIsJumping) {
				fShowWindow = true;
			}
		}

		private Vector3d validateCoords() {
			try {
				return new Vector3d( Convert.ToDouble(fPersistentXCoord), Convert.ToDouble(fPersistentYCoord), Convert.ToDouble(fPersistentZCoord) );
			} catch (Exception e) {
				print (String.Format ("[JUMP] - Couldn't parse coordinates: {0},{1}", e.Message, e.StackTrace));
				return new Vector3d(0.0f, 0.0f, 0.0f);
			}
		}

		private String vectorToString(Vector3d vec) {
			return String.Format ("({0},{1},{2})", vec.x, vec.y, vec.z);
		}

		private void DrawNavWindow(int windowId) {
			try{
				// display navigation to different celestial bodies
//				int total = FlightGlobals.Bodies.ToArray().Length;
//				double now = Planetarium.GetUniversalTime ();
//				foreach (CelestialBody body in FlightGlobals.Bodies) {
//					GUILayout.BeginHorizontal ();
//					GUILayout.Label (body.name);
//					GUILayout.Space (100);
//					GUILayout.Label (vectorToString (body.getPositionAtUT (now)));
//					if (GUILayout.Button ("Jump High Orbit")) {
//						fJumpParams.Target = body;
//						fJumpParams.Altitude = Math.Max (5 * MINIMUM_ALTITUDE_FROM_BODY, body.maxAtmosphereAltitude * 5) + (float)body.Radius;
//						fJumpParams.JumpInitiateTime = Planetarium.GetUniversalTime();
//						fTargetIsSet = true;
//						fJumpSpinupSoundGroup.audio.Play();
//					}
//					if (GUILayout.Button ("Jump Low Orbit")) {
//						fJumpParams.Target = body;
//						fJumpParams.Altitude = Math.Min (MINIMUM_ALTITUDE_FROM_BODY, body.maxAtmosphereAltitude * 1.5f) + (float)body.Radius;
//						fJumpParams.JumpInitiateTime = Planetarium.GetUniversalTime();
//						fTargetIsSet = true;
//						fJumpSpinupSoundGroup.audio.Play();
//					}
//					GUILayout.EndHorizontal ();
//				}
				GUILayout.BeginHorizontal();
				GUILayout.Label("Position X:");
				fPersistentXCoord = GUILayout.TextField(fPersistentXCoord, 25);
				GUILayout.Label("Position Y:");
				fPersistentYCoord = GUILayout.TextField(fPersistentYCoord, 25);
				GUILayout.Label("Position Z:");
				fPersistentZCoord = GUILayout.TextField(fPersistentZCoord, 25);
				if (GUILayout.Button("Jump Here")) {
					Vector3d vec = validateCoords();
					if (vec != new Vector3d(0.0f, 0.0f, 0.0f)) {
						fJumpParams.JumpInitiateTime = Planetarium.GetUniversalTime();
						fTargetIsSet = true;
						fJumpParams.JumpCoord = vec;
						fJumpParams.Altitude = 0.0f;
					}
				}
				GUILayout.EndHorizontal();
				GUI.DragWindow ();
			} catch (Exception e) {
				print(String.Format("[JUMP] Error in OnFixedUpdate - {0},{1}\n\n{2}", e.Message, e.Source, e.StackTrace));
			}
		}

		private void InitStyles() {
			fWindowStyle = new GUIStyle (HighLogic.Skin.window);
			fWindowStyle.stretchWidth = true;

			fLabelStyle = new GUIStyle (HighLogic.Skin.label);
			fLabelStyle.stretchWidth = true;

			fButtonStyle = new GUIStyle (HighLogic.Skin.button);
			fButtonStyle.fixedWidth = 50.0f;

			fBoxStyle = new GUIStyle (HighLogic.Skin.box);
			fBoxStyle.stretchWidth = true;
		}
		public override void OnStart(StartState st) {
			try {
				if (st != StartState.Editor) {
					if (!GameDatabase.Instance.ExistsAudioClip(fJumpSoundFile)) {
						print(String.Format("[JUMP] Audio file not found: {0}", fJumpSoundFile));
					} else {
						fJumpSoundGroup.audio = gameObject.AddComponent<AudioSource>();
						fJumpSoundGroup.audio.clip = GameDatabase.Instance.GetAudioClip(fJumpSoundFile);
						fJumpSoundGroup.audio.Stop();
						fJumpSoundGroup.audio.loop = false;
					}

					if (!GameDatabase.Instance.ExistsAudioClip(fSpinupSoundFile)) {
						print(String.Format("[JUMP] Audio file not found: {0}", fSpinupSoundFile));
					} else {
						fJumpSpinupSoundGroup.audio = gameObject.AddComponent<AudioSource>();
						fJumpSpinupSoundGroup.audio.clip = GameDatabase.Instance.GetAudioClip(fSpinupSoundFile);
						fJumpSpinupSoundGroup.audio.Stop();
						fJumpSpinupSoundGroup.audio.loop = false;
					}

					if (!fHasInitStyles) {
						InitStyles();
					}
					// Add events to stop the sound when paused.
					// Make sure to remove these if the part is destroyed to prevent a memory leak.
//					GameEvents.onGamePause.Add(new EventVoid.OnEvent(this.OnPause));
//					GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(this.OnUnPause));
				}
			} catch (Exception e) {
				print(String.Format("[JUMP] Error in OnStart - {0}", e.Message));
			}
		}

		private void OnGUI() {
			// make a screen at the center
			if (fIsJumping && fShowWindow) {
				float windowHeight = WINDOW_HEIGHT_PCT * Screen.height;
				float windowWidth = WINDOW_WIDTH_PCT * Screen.width;
				Rect sr = new Rect (Screen.width / 2 - windowWidth / 2, Screen.height / 2 - windowHeight / 2, windowWidth, windowHeight);
				GUILayout.Window (10, sr, DrawNavWindow, "Navigation", fWindowStyle);
			}
		}

		public override void OnLoad(ConfigNode node) {
			try {
				if (fState == StartState.Editor) {
					return;
				}
				base.OnLoad(node);
			} catch (Exception e) {
				print(String.Format("[JUMP] Error in Onload - {0}", e.Message));
			}
		}

		public void FixedUpdate() {
			try {
				if (vessel == null || fState == StartState.Editor) return;
//				var emod = part.FindModuleImplementing<ModuleEngines>();

				if (fIsJumping && fTargetIsSet) {
					// we can only jump in space
					if (vessel.altitude < MINIMUM_ALTITUDE_FROM_BODY || vessel.atmDensity > 0.0) {
						fStatus = "Can't jump near planet or in atmosphere!";
						fIsJumping = false;
						fTargetIsSet = false;
						return;
					}

					double now = Planetarium.GetUniversalTime();
					double elapsedSinceInitiate = Math.Abs( now - fJumpParams.JumpInitiateTime );
					fShowWindow = false;
					if (elapsedSinceInitiate < JUMP_SPINUP_TIME_SECONDS ) {
						if (fJumpDelayMessage != null) {
							ScreenMessages.RemoveMessage(fJumpDelayMessage);
						}
						fJumpDelayMessage = ScreenMessages.PostScreenMessage( String.Format("Spinning up jump drive... {0}s", (int)(JUMP_SPINUP_TIME_SECONDS - elapsedSinceInitiate)) );
						print(String.Format("[JUMP] - Need {0} more seconds before we can jump", JUMP_SPINUP_TIME_SECONDS - elapsedSinceInitiate));
						return;	// still spinning up
					}

					Vector3d targetPos = fJumpParams.JumpCoord;// fJumpParams.Target.getTruePositionAtUT(now);
					Vector3d destinationPos = targetPos + new Vector3d(fJumpParams.Altitude, 0, 0);
					Vector3d currentVel = vessel.GetObtVelocity();
					double distToGo = Vector3d.Distance( destinationPos, vessel.GetWorldPos3D() );

					// do the jump!
					print ( String.Format("[JUMP] - From {0} to {1}, distance is {2}",vectorToString(vessel.GetWorldPos3D()), vectorToString(destinationPos), distToGo) );
					Krakensbane kbane = (Krakensbane)FindObjectOfType(typeof(Krakensbane));
					kbane.setOffset(destinationPos);
					if (!fJumpSoundGroup.audio.isPlaying) {
						fJumpSoundGroup.audio.Play();
					}
					// now set the velocity to be the same as it was when you jumped
					vessel.SetWorldVelocity(currentVel);

					print ("[JUMP] - Jump completed");
					fStatus = "Jump Succeeded!";
					fIsJumping = false;
					fTargetIsSet = false;
					fJumpParams = new JumpCoordinates();
					fJumpDelayMessage = null;
				}
			} catch (Exception e) {
				print(String.Format("[JUMP] Error in OnFixedUpdate - {0},{1}\n\n{2}", e.Message, e.Source, e.StackTrace));
			}
		}
	}
}

