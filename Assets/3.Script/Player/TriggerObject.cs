using System;
using UnityEngine;
using static TriggerObject;

public class TriggerObject : MonoBehaviour
{
    public enum Role 
    { 
        ChargeDetect, 
        HitBox, 
    }

    [SerializeField] private Role role;

    private PlayerCtrl player;

    private void Awake()
    {
        player = GetComponentInParent<PlayerCtrl>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Target"))
        {
            string tag = collision.gameObject.tag;
            player.OnPlayerTriggerEnter(role, collision, tag);
        }
        else if (collision.CompareTag("Anchor"))
        {
            string tag = collision.gameObject.tag;
            player.OnPlayerTriggerEnter(role, collision, tag);
        }
        else if (collision.CompareTag("ReturnTitle"))
        {
            string tag = collision.gameObject.tag;
            player.OnPlayerTriggerEnter(role, collision, tag);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ReturnTitle"))
        {
            string tag = collision.gameObject.tag;
            player.OnPlayerTriggerExit(role, collision, tag);
        }
    }
}
