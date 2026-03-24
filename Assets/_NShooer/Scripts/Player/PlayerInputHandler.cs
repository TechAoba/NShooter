using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class PlayerInputHandler : NetworkBehaviour
	{
		[Header("References")]
		[SerializeField] private PlayerMovement _movement;
		[SerializeField] private PlayerCharacterWeapon _weapon;

        void Update()
        {
			// 只有本地玩家处理输入
            if (!isLocalPlayer) return;

			// === 移动输入 ===
			float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
			_movement?.SetInput(new Vector2(moveX, moveY), Input.mousePosition);

			// === 射击输入 ===
			if (Input.GetMouseButton(0))
			{
				_weapon?.TryShoot();
			}

			// === 换弹输入 ===
			if (Input.GetKeyDown(KeyCode.R))
			{
				_weapon?.TryReload();
			}
        }
    }
}