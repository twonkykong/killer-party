using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWalk : MonoBehaviour
{
    Rigidbody rb;
    RectTransform rect;
    public Transform joystick, cam;
    public float speed;

    Animator anim;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rect = joystick.GetComponent<RectTransform>();
        anim = GetComponent<Animator>();
    }

    private void Update() 
    { 
        Vector3 pos = Vector3.right * (joystick.localPosition.x / rect.sizeDelta.x * speed) + Vector3.forward * (joystick.localPosition.y / rect.sizeDelta.x * speed);
        Vector3 newPos = new Vector3(pos.x, rb.velocity.y, pos.z);
        rb.velocity = newPos;

        if (joystick.localPosition != Vector3.zero)
        {
            anim.SetBool("walk", true);
            anim.SetFloat("walkSpeed", Vector3.Distance(joystick.localPosition, Vector3.zero) / rect.sizeDelta.x);
        }
        else anim.SetBool("walk", false);

        transform.LookAt(transform.position + (Vector3.forward * joystick.localPosition.y / rect.sizeDelta.y) + (Vector3.right * joystick.localPosition.x / rect.sizeDelta.x));
        cam.position = Vector3.Slerp(cam.position, transform.position + new Vector3(0, 10f, -10f), 0.25f);
    }
}