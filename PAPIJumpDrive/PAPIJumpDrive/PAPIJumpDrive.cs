using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace PAPIJumpDrive
{
	// some constants
	public class PAPIJumpDriveConstants {
		public int LIGHTSPEED = 299792458;
	}

	// from https://github.com/BobPalmer/WarpDrive/blob/master/Source/WarpEngine/WarpEngine/ShipInfo.cs
	public class ShipInfo
	{
		public Part ShipPart { get; set; }
		public float BreakingForce { get; set; }
		public float BreakingTorque { get; set; }
		public float CrashTolerance { get; set; }
		public RigidbodyConstraints Constraints { get; set; }
	}

	public class PAPIJumpModule : PartModule
	{
		private StartState fState;

		[KSPField(guiActive = true, guiName = "Jump Drive", guiActiveEditor = false)]
		public string fStatus = "inactive";	// current module status
		public bool fIsJumping = false;	// is the device currently in a jump?
		[KSPEvent(guiActive = true, active = true, guiName = "Jump!", guiActiveEditor = false, guiActiveUnfocused = false)]
		public void toggleJumping() {
			fIsJumping = !fIsJumping;
		}

		private String vectorToString(Vector3d vec) {
			return String.Format ("({0},{1},{2})", vec.x, vec.y, vec.z);
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

				if (fIsJumping) {
					List<CelestialBody> bodies = FlightGlobals.Bodies;
					var r = new System.Random();
					CelestialBody someBody = bodies[r.Next(0, bodies.ToArray().Length)];
					double now = Planetarium.GetUniversalTime();
					Vector3d targetPos = someBody.getTruePositionAtUT(now);
					double offset = someBody.sphereOfInfluence * 0.25;
					Vector3d destinationPos = targetPos + new Vector3d(offset,0,0);
					double distToGo = Vector3d.Distance( destinationPos, vessel.transform.position );
					print(String.Format("[JUMP] - We will jump to {0} plus an offset so it will be {1}", vectorToString(targetPos), vectorToString(destinationPos)));
					print(String.Format("[JUMP] - Target is {0}", someBody.GetName()));

					// do the jump!
					print ( String.Format("[JUMP] - From {0} to {1}, distance is {2}",vectorToString(vessel.GetWorldPos3D()), vectorToString(destinationPos), distToGo) );
					Krakensbane kbane = (Krakensbane)FindObjectOfType(typeof(Krakensbane));
					kbane.setOffset(destinationPos);
					print ("[JUMP] - Jump completed");
					fIsJumping = false;
				}
			} catch (Exception e) {
				print(String.Format("[JUMP] Error in OnFixedUpdate - {0},{1}\n\n{2}", e.Message, e.Source, e.StackTrace));
			}
		}
	}
}

