using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace NShooter 
{
	public class GUIGame : MonoBehaviour
	{
		[SerializeField] private Button _buttonRespawn;

		private void Start()
		{
			// 初始隐藏按钮
			_buttonRespawn?.gameObject.SetActive(false);
			_buttonRespawn?.onClick.AddListener(OnButtonRespawn);
		}

		// 当客户端成功连接服务器，显示重生按钮
		public void OnClientConnected()
		{
			_buttonRespawn?.gameObject.SetActive(true);
		}
		// 玩家被 销毁 时，显示重生按钮
		public void OnClientDestroyed()
		{
			_buttonRespawn?.gameObject.SetActive(true);
		}

		private void OnButtonRespawn()
		{
			// 放置重复点击
			_buttonRespawn?.gameObject.SetActive(false);
			
			if (!NetworkClient.active)
			{
				Debug.Log("Not connected to server");
			}

			// Mirror 创建玩家
			NetworkClient.AddPlayer();
		}
	}
}