using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace NShooter 
{
	public class PlayerSession : NetworkBehaviour
	{
		public PlayerCharacterVisual _playerCharacterVisual;

		[SyncVar(hook = nameof(OnNicknameChanged))]
		public string _nickname;
        [SyncVar]
        public int _kill;
        [SyncVar]
        public int _death;

        [Server]
        public void AddKill() => ++_kill;
        [Server]
        public void AddDeath() => ++_death;

        // ----------------------------
        // 服务器初始化昵称（仅服务器调用）
        // ----------------------------
        [Server]
		public void InitializeNickname(string name)
		{
			_nickname = name;
		}

		// ----------------------------
        // 客户端请求改名（通过 Command）
        // ----------------------------
        [Client]
        public void SetNickname(string newNickname)
        {
            if (!isLocalPlayer)
            {
                Debug.LogWarning("不能修改非本地玩家的昵称！");
                return;
            }

            // 发送改名请求到服务器
            CmdSetNickname(newNickname);
        }

		[Command]
        private void CmdSetNickname(string newNickname)
        {
            // 安全校验：确保是本人操作（Mirror 已保证，但可加强）
            if (connectionToClient == null) return;

            // 输入校验
            if (string.IsNullOrWhiteSpace(newNickname))
            {
                newNickname = "Player";
            }
            else
            {
                // 限制长度（例如最多16字符）
                newNickname = newNickname.Trim().Substring(0, Mathf.Min(newNickname.Trim().Length, 16));
            }

            // 更新昵称 → 自动同步到所有客户端
            _nickname = newNickname;
        }

        // ----------------------------
        // 昵称变更回调（所有客户端触发）
        // ----------------------------
        private void OnNicknameChanged(string oldName, string newName)
        {
			_playerCharacterVisual._textNametag.SetText(newName);
        }


	}
}