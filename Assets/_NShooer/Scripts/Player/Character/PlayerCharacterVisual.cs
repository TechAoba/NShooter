using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NShooter 
{
	public class PlayerCharacterVisual : NetworkBehaviour, IPlayerCharacterDefaultMaterialProvider
	{
		public Transform _visualTransform;
		[SerializeField] SkinnedMeshRenderer[] _renderers;

		[SerializeField] Material _materialAlly;
		[SerializeField] Material _materialEnemy;

        public override void OnStartClient()
        {
			foreach (SkinnedMeshRenderer render in _renderers)
			{
				render.material = GetDefaultMaterial();
			}
        }

		public Material GetDefaultMaterial()
		{
			return isLocalPlayer ? _materialAlly : _materialEnemy;
		}
    }
}