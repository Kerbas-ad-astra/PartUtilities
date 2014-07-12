﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSIPartUtilities
{
	public class JSIAnimationTracker: PartModule
	{
		[KSPField]
		public string animationName = string.Empty;

		[KSPField]
		public float maxTrigger = 1.1f;
		[KSPField]
		public float minTrigger = 0.8f;

		[KSPField]
		public string componentToggles = string.Empty;
        
		[KSPField]
		public string moduleToggles = string.Empty;

		private List<Actuator> actuators = new List<Actuator> ();
		private Animation trackedAnimation;
		private bool actuatorState;

		public override void OnStart (PartModule.StartState state)
		{
			trackedAnimation = part.FindModelAnimators (animationName) [0];
			if (trackedAnimation == null) {
				JUtil.LogErrorMessage (this, "Could not find animation named '{0}' to track.", animationName);
				Destroy (this);
			}
			// Bloody Squad and their ConfigNodes that never work properly!
			try {
				foreach (string actuatorConfig in componentToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.PartComponent, part));
				}
				foreach (string actuatorConfig in moduleToggles.Split(new [] {'|'},StringSplitOptions.RemoveEmptyEntries)) {
					actuators.Add (new Actuator (actuatorConfig.Trim (), ActuatorType.PartModule, part));
				}
			} catch {
				JUtil.LogErrorMessage (this, "Please check your configuration.");
				Destroy (this);
			}
			actuatorState = GetAnimationState ();
			LoopThroughActuators (actuatorState);
		}

		private void LoopThroughActuators (bool state)
		{
			actuatorState = state;
			foreach (Actuator thatActuator in actuators) {
				thatActuator.SetState (part, state);
			}
		}

		private bool GetAnimationState ()
		{
			return trackedAnimation [animationName].normalizedTime >= minTrigger && trackedAnimation [animationName].normalizedTime <= maxTrigger;
		}

		public override void OnUpdate ()
		{
			if (trackedAnimation != null) {
				bool newstate = GetAnimationState ();
				if (newstate != actuatorState) {
					LoopThroughActuators (newstate);
				}

			}
		}
	}
}

