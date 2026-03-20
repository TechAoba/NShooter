using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NShooter 
{
	public class PlayerCharacter : NetworkBehaviour
	{
        public override void OnStartClient()
        {
            if (isLocalPlayer)
			{
				LocalPlayerManager.Instance.InvokeOnCharacterSpawned(this);
			}
        }
	}
}