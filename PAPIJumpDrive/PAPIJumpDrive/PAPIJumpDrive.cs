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
	}

	public class PAPIJumpDrive : PartModule
	{
		private const int LIGHTSPEED = 299792458;
		private const int MINIMUM_ALTITUDE_FROM_BODY = 100000;	// must be at least 100km away
		private const float WINDOW_HEIGHT_PCT = 0.85f;
		private const float WINDOW_WIDTH_PCT = 0.85f;

		private AudioClip fWarpSound;

		private StartState fState = 0;
		private GUIStyle fWindowStyle, fLabelStyle, fButtonStyle, fBoxStyle;
		private bool fHasInitStyles = false;
		private Rect fWindowPosition;
		private bool fShowWindow = false;

		[KSPField(guiActive = true, guiName = "Jump Drive", guiActiveEditor = false)]
		public string fStatus = "inactive";	// current module status
		private bool fTargetIsSet = false;	// do we have a destination?
		private bool fIsJumping = false;	// is the device currently in a jump?
		private JumpCoordinates fJumpParams = new JumpCoordinates();
		[KSPEvent(guiActive = true, active = true, guiName = "Jump!", guiActiveEditor = false, guiActiveUnfocused = true)]
		public void toggleJumping() {
			fIsJumping = !fIsJumping;
			if (fIsJumping) {
				fShowWindow = true;
			}
		}

		private String vectorToString(Vector3d vec) {
			return String.Format ("({0},{1},{2})", vec.x, vec.y, vec.z);
		}

		private void DrawNavWindow(int windowId) {
			try{
				// display navigation to different celestial bodies
				int total = FlightGlobals.Bodies.ToArray().Length;
				double now = Planetarium.GetUniversalTime ();
				foreach (CelestialBody body in FlightGlobals.Bodies) {
					GUILayout.BeginHorizontal ();
					GUILayout.Label (body.name);
					GUILayout.Space (100);
					GUILayout.Label (vectorToString (body.getPositionAtUT (now)));
					if (GUILayout.Button ("Jump High Orbit")) {
						fJumpParams.Target = body;
						fJumpParams.Altitude = Math.Max (5 * MINIMUM_ALTITUDE_FROM_BODY, body.maxAtmosphereAltitude * 5) + (float)body.Radius;
						fTargetIsSet = true;
					}
					if (GUILayout.Button ("Jump Low Orbit")) {
						fJumpParams.Target = body;
						fJumpParams.Altitude = Math.Min (MINIMUM_ALTITUDE_FROM_BODY, body.maxAtmosphereAltitude * 1.5f) + (float)body.Radius;
						fTargetIsSet = true;
					}
					GUILayout.EndHorizontal ();
				}

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
					if (!fHasInitStyles) {
						InitStyles();
					}
					fWarpSound = GetComponent<AudioSource>();
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
				fWindowPosition = GUILayout.Window (10, sr, DrawNavWindow, "Navigation", fWindowStyle);
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
				var emod = part.FindModuleImplementing<ModuleEngines>();

				if (fIsJumping && fTargetIsSet) {
					// we can only jump in space
					if (vessel.altitude < MINIMUM_ALTITUDE_FROM_BODY || vessel.atmDensity > 0.0) {
						fStatus = "Can't jump near planet or in atmosphere!";
						fIsJumping = false;
						fTargetIsSet = false;
						return;
					}
	
					emod.f
					// for now we'll just jump to some random body
					double now = Planetarium.GetUniversalTime();
					Vector3d targetPos = fJumpParams.Target.getTruePositionAtUT(now);
					Vector3d destinationPos = targetPos + new Vector3d(fJumpParams.Altitude, 0, 0);
					Vector3d currentVel = vessel.GetObtVelocity();
					double distToGo = Vector3d.Distance( destinationPos, vessel.GetWorldPos3D() );
					print(String.Format("[JUMP] - We will jump to {0} plus an offset so it will be {1}", vectorToString(targetPos), vectorToString(destinationPos)));
					print(String.Format("[JUMP] - Target is {0}", fJumpParams.Target.GetName()));

					// do the jump!
					print ( String.Format("[JUMP] - From {0} to {1}, distance is {2}",vectorToString(vessel.GetWorldPos3D()), vectorToString(destinationPos), distToGo) );
					Krakensbane kbane = (Krakensbane)FindObjectOfType(typeof(Krakensbane));
					kbane.setOffset(destinationPos);
					audio.PlayOneShot(fWarpSound);

					// now set the velocity to be the same as it was when you jumped
					vessel.SetWorldVelocity(currentVel);

					print ("[JUMP] - Jump completed");
					fStatus = "Jump Succeeded!";
					fIsJumping = false;
					fTargetIsSet = false;
				}
			} catch (Exception e) {
				print(String.Format("[JUMP] Error in OnFixedUpdate - {0},{1}\n\n{2}", e.Message, e.Source, e.StackTrace));
			}
		}
	}
}

