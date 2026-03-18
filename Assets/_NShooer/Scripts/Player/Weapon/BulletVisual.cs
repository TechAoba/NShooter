using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace NShooter 
{
	public class BulletVisual : MonoBehaviour
	{
		[SerializeField] float _moveSpeed = 50f;

		public float FlyTo(Vector3 offset)
		{
			float duration = offset.magnitude / _moveSpeed;
			StartCoroutine(MoveRoutine(offset, duration));
			return duration;
		}

		IEnumerator MoveRoutine(Vector3 offset, float duration)
		{
			Vector3 start = transform.position;
			Vector3 end = start + offset;
			float elapsed = 0;
			while (elapsed < duration)
			{
				transform.position = Vector3.Lerp(start, end, elapsed / duration);
				elapsed += Time.deltaTime;
				yield return null;
			}
			transform.position = end;
		}
	}
}