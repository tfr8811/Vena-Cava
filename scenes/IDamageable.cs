using Godot;
using System;
/// <summary>
/// Roman Noodles
/// 2/6/2024
/// interface for damageable gameobjects
/// </summary>
public interface IDamageable
{
    public void TakeDamage(int damage);
}