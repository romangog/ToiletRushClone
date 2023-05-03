using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHitbox : MonoBehaviour
{
    public event Action<Collider2D> CharacterHitEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CharacterHitEvent?.Invoke(collision);
    }
}
