using Mirror;
using StinkySteak.SimulationTimer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class AutoDestroyGO : MonoBehaviour
	{
		[SerializeField] private float _lifttime = 2f;
		private SimulationTimer _timerLifetime;

        private void Start()
        {
            _timerLifetime = SimulationTimer.CreateFromSeconds(_lifttime);
        }

        private void Update()
        {
            if (_timerLifetime.IsExpiredOrNotRunning())
			{
				Destroy(gameObject);
			}
        }
    }
}