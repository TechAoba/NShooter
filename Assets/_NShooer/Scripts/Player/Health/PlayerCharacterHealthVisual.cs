using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class PlayerCharacterHealthVisual : NetworkBehaviour
	{
		[SerializeField] private PlayerCharacterHealth _health;
		[SerializeField] private GameObject _vfxBloodPrefab;

		[SerializeField] private SkinnedMeshRenderer[] _renderers;
		[SerializeField] private Material _defaultMaterial;
		[SerializeField] private Material _onHitMaterial;

		[SerializeField] private float _onHitMaterialDuration = 0.2f;

        public override void OnStartClient()
        {
            _health.OnHealthDecrease += OnHealthChanged;
        }
        public override void OnStopClient()
        {
            _health.OnHealthDecrease -= OnHealthChanged;
        }

		private void ApplyMaterial(Material material)
		{
			foreach (SkinnedMeshRenderer renderer in _renderers)
			{
				renderer.material = material;
			}
		}

		// 玩家收到伤害时：血液vfx生成、
		private void OnHealthChanged()
		{
			Instantiate(_vfxBloodPrefab, transform.position, Quaternion.identity);
			StartCoroutine(ChangeMaterial2Hit());
		}

		IEnumerator ChangeMaterial2Hit()
		{
			ApplyMaterial(_onHitMaterial);
			yield return new WaitForSeconds(_onHitMaterialDuration);
			ApplyMaterial(_defaultMaterial);
		}
	}
}