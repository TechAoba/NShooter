using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace NShooter 
{
	public class VFXBulletVisual : MonoBehaviour
	{
		[SerializeField] private GameObject _vfxBulletImpactPrefab;
		[SerializeField] float _moveSpeed = 50f;

		public void FlyTo(Vector3 offset)
		{
			float duration = offset.magnitude / _moveSpeed;
			StartCoroutine(MoveRoutine(offset, duration));
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
			DestroyThis();
		}

		private void DestroyThis()
		{
			// 碰撞粒子特效
			Instantiate(_vfxBulletImpactPrefab, transform.position, Quaternion.identity);
			
			Destroy(gameObject);
		}
	}
}