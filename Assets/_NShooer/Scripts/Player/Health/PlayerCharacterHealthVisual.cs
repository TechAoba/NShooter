using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class PlayerCharacterHealthVisual : NetworkBehaviour
	{
		[SerializeField] PlayerCharacterHealth _health;
		[SerializeField] GameObject _vfxBloodPrefab;

		[SerializeField] SkinnedMeshRenderer[] _renderers;
		Material _defaultMaterial;
		[SerializeField] Material _onHitMaterial;

		[SerializeField] private float _onHitMaterialDuration = 0.2f;

        public override void OnStartClient()
        {
			IPlayerCharacterDefaultMaterialProvider materialProvider = GetComponent<IPlayerCharacterDefaultMaterialProvider>();
			_defaultMaterial = materialProvider.GetDefaultMaterial();
			
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