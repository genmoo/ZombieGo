using System;
using Fusion;
using UnityEngine;

public class DashGhost : NetworkBehaviour
{
    [SerializeField] private Sprite[] sprites; // 0: side, 1: up, 2: down

    private SpriteRenderer sr;

    [Networked]
    public int SpriteType { get; set; } // 0, 1, 2a
    [Networked]
    public bool IsFlipX { get; set; } // true or false

    public override void Spawned()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplySprite();
        
        Invoke(nameof(DespawnSelf), 0.2f); // 0.2초 후 자동 삭제
    }

    public void Update()
    {
        print(SpriteType);
    }

    private void ApplySprite()
    {
        if (sr == null || sprites.Length == 0) return;

        sr.sprite = sprites[SpriteType];
        sr.flipX = IsFlipX;
        sr.sortingOrder = 0;
        sr.color = new Color(1f, 1f, 1f, 0.5f);
    }

    private void DespawnSelf()
    {
        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}