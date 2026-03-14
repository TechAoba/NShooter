using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NShooter 
{
	public class PlayerMovement : NetworkBehaviour
	{
		[SerializeField] private Rigidbody _rb;
        [SerializeField] private float _speed = 6f;

        Camera camera;
        private void Start()
        {
            camera = Camera.main;
        }

        private void HandleMovement()
		{
			if (!isLocalPlayer) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 forward = camera.transform.forward * v;
            Vector3 right = camera.transform.right * h;

            Vector3 direction = (new Vector3(forward.x + right.x, 0f, forward.z + right.z)).normalized;
            transform.Translate(direction * Time.deltaTime * _speed);
		}

        private void Update()
        {
			HandleMovement();
        }
    }
}