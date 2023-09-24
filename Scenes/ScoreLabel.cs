using Godot;
using System;

public class ScoreLabel : Label
{
    public int score {get; set;} = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Text = "Score: " + score;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    //"`Signal`" that is connected to each specific `Mob` instance when it is made which ->
    //-> changes the score when the `Mob` instance is "squashed"
    public void _on_Mob_SquashedEventHandler(float comboCount){
        GD.Print("I FIRED: " + comboCount);
        score += 10 * (int) comboCount;
        //Using "string interpolation` with the `$` and `{}`
        Text = $"Score: {score}";
        //Text = "Score: " + score;
    }
}
