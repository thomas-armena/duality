using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{

    public CharacterController controller;
    public Transform groundCheck;
    public float speed = 14f;
    public float gravity = -9.81f;
    public float groundDistance = 0.4f;
    public float jumpHeight = 3;
    public LayerMask groundMask;
    Vector3 velocity;

    public GameObject camera;

    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        camera.SetActive(IsOwner);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            walk();
            jump();
            fall();
            if (NetworkManager.Singleton.IsServer)
            {
                networkPosition.Value = transform.position;
            }
            else
            {
                submitPositionServerRpc(transform.position);
            }
        } 
        else
        {
            transform.position = networkPosition.Value;
        }


    }

    [ServerRpc]
    void submitPositionServerRpc(Vector3 position)
    {
        networkPosition.Value = position;
    }

    private void walk()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);
    }

    private void jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void fall()
    {

        if (isGrounded() && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    private bool isGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }
}
