using Mirror;
using Mirror.Examples.AssignAuthority;
using Mirror.Examples.CouchCoop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class MyNetworkManager : NetworkManager
	{
        [Header("Custom")]
        [SerializeField] private GameObject _playerCharacterPrefab; // 玩家对象
        public GUIGame guiGame;
        [SerializeField] private Vector3 _spawnPoint; // 出生点

        // 在客户端执行：当客户端成功连接到服务器时触发（客户端初始化、UI更新等）
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            guiGame.OnEnterGame();
        }

        // 在客户端执行：当有玩家断开连接（清理本地状态、返回主菜单、实现断开连接等）
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            guiGame.OnQuitGame();
        }

        // 在服务端执行：当有新玩家连接并请求生成玩家时调用
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // 1. 实例化 Player（作为玩家对象）
            GameObject player = Instantiate(_playerCharacterPrefab);
            if (player.TryGetComponent(out PlayerSession playerSession))
            {
                playerSession.InitializeNickname($"Player_{conn.connectionId}");
            }

            // 2. 传递出生点
            if (_spawnPoint != null)
            {
                player.transform.position = _spawnPoint;
            }

            // 3. 告诉 Mirror：这是该连接的“玩家对象”
            NetworkServer.AddPlayerForConnection(conn, player);

            // 4. 往PlayerManager注册玩家
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RegisterPlayerCharacter(conn, player);
            }
        }

        // 在服务端执行：当有玩家断开连接（清理玩家数据、通知其他玩家、保存进度等）
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RemovePlayerCharacter(conn);
            }
        }
	}
}