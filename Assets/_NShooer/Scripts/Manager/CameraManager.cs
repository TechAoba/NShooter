using Cinemachine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class CameraManager : MonoBehaviour
	{
		[SerializeField] CinemachineVirtualCamera _virtualCamera;

        void Awake()
        {
            LocalPlayerManager.Instance.OnCharacterSpawned += OnCharacterSpawned;
        }

		void OnCharacterSpawned(PlayerCharacter playerCharacter)
		{
			_virtualCamera.Follow = playerCharacter.transform;
		}
    }
}