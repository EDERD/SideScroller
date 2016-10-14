using UnityEngine;
using System.Collections;
[RequireComponent(typeof(PlayerController))]

public class PlayerInput : MonoBehaviour {

    PlayerController player;

	void Start () {

        player = GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);
        if(Input.GetKeyDown(KeyCode.Space))
        {
            player.JumpInputDown();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            player.JumpInputUp();
        }

    }
}
