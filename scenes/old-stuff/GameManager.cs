using Godot;
using System;

/// <summary>
/// Roman Noodles
/// 5/03/2024
/// Manages the games UI overlay, will manage important variables and saving
/// </summary>
public partial class GameManager : Node3D
{

    //reference to the player
    [Export]
    private Player player;

    [Export]
    private Label copCounter;

    private int copsDefeated = 0;

    public override void _Ready()
    {
        // setup the copCounter
        copCounter.Text = copsDefeated.ToString();
    }

    /// <summary>
    /// Roman Noodles
    /// 5/03/2024
    /// Event handler for when the player defeats an enemy
    /// </summary>
    public void _on_player_enemy_defeated()
    {
        copsDefeated += 1;
        copCounter.Text = copsDefeated.ToString();
    }

    ///// <summary>
    ///// Roman Noodles
    ///// 5/19/2024
    ///// keeps track of when enemy hit, alerts nearby enemies
    ///// </summary>
    //public void enemyHit(Enemy enemyRef)
    //{
    //    // alert the cops who could see it
    //    var allEnemies = GetTree().GetNodesInGroup("Enemies");
    //    foreach (var enemy in allEnemies)
    //    {
    //        if (enemy is Enemy)
    //        {
    //            Enemy enemy1 = (Enemy) enemy;
    //            // check can see target must take a relative position vector
    //            if (!enemy1.HasSeenPlayer /*&& enemy1.CheckCanSeeTarget(enemy1.GlobalPosition - enemyRef.GlobalPosition, enemy1)*/)
    //            {
    //                enemy1.HasSeenPlayer = true;
    //                //enemy1.Callout();
    //                //GD.Print("it worked");
    //            }
    //        }
    //    }
    //}
}
