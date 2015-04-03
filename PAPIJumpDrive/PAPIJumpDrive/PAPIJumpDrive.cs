using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace PAPIJumpDrive
{
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

	}
}

