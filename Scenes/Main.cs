using Godot;
using System;

public class Main : Node
{
    [Export]
    public PackedScene MobScene{get; set;}

    private Player player;
    private ColorRect retryRect;
    //Keeps track of the number of `Mob` instances
    private int mobCount = 0;
    private int mobSpeed = 0;
    private Timer mobTimer;
    private ScoreLabel scoreLabel;
    private Label highscoreLabel;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        player = GetTree().Root.GetChild(1).GetChild<Player>(2);
        mobTimer = GetTree().Root.GetChild(1).GetChild<Timer>(7);
        retryRect = GetTree().Root.GetChild(1).GetChild(8).GetChild<ColorRect>(1);
        //The `Hide()` `CanvasItem` instance function hides the `CanvasItem` instance if it ->
        //-> currently visible
        retryRect.Hide();
        scoreLabel = GetTree().Root.GetChild(1).GetChild(8).GetChild<ScoreLabel>(0);
        highscoreLabel = GetTree().Root.GetChild(1).GetChild(8).GetChild<Label>(2);
        Initialize();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }

    public void _on_MobTimer_timeout(){
        //Creating a new `Mob` instance from the `MobScene.tscn` scene that we also "Packed" ->
        //-> above
        //NOTE: The `PackedScene` Class instance function `Instance<>()`, along with its ->
        //-> other functions, triggers the `Node.NotificationInstanced` notification ->
        //-> which I want to keep in mind because it might come in hand later sometime
        Mob mob = MobScene.Instance<Mob>();
        
        //Choose a random location on the `SpawnPath` instance (node)
        //We do this by storing a reference for the `SpawnLocation` instance (node)
        PathFollow mobSpawnLocation = GetNode<PathFollow>("SpawnPath/SpawnLocation");
        //Give the new `mobSpawnLocation` instance of the `PathFollow` Class a random ->
        //-> offset
        //NOTE: The Class function `Randf()` for the `GD` Class (`Godot.GD`) produces a ->
        //-> value between 0 and 1 which is what the `PathFollow` instance (node) ->
        //-> `mobSpawnLocation` expects because `0` is the start of the path and `1` is the ->
        //-> end of the path
        mobSpawnLocation.UnitOffset = GD.Randf();

        //Next we get the `player` instance's current position so that we can pass it as ->
        //-> an argument to our `Mob` instance function we made earlier
        Vector3 playerPosition = player.Translation;

        //Must put this before the `Initialize(Vector3 startPosition, Vector3 playerPosition)` ->
        //-> `Mob` instance function so that the `minSpeed` and `maxSpeed` `Mob` instance ->
        //-> variables are changed in time
        //CHANGE: Made it so only the `maxSpeed` `Mob` instance variable was changed over ->
        //-> time, to add more variety in speeds and make it harder to judge timing of jumps
        if(mobCount >= 4){
            if(mobTimer.WaitTime >= 1){
                mobSpeed += 8;
            }
        }
        mob.maxSpeed = mob.maxSpeed + mobSpeed;
        
        //Using the `Mob` instance function we made earlier to pass the ->
        //-> `mobSpawnLocation.Translation` instance variable as an argument for the ->
        //-> `startPosition` parameter and passing the `playerPosition` Vector3 instance ->
        //-> as an argument for the `playerPosition` parameter
        mob.Initialize(mobSpawnLocation.Translation, playerPosition);

        //BELOW NOTE: The note below is old and is not needed anymore but I kept it to ->
        //-> show my thinking
        //IMPORTANT NOTE: The line of code below had to be moved further up because ->
        //-> the instances were not added to the `SceneTree` before running the ->
        //-> `Initialize` instance function for the `Mob` instance which conflicted with ->
        //-> generating playback speeds for the animation
        //Last we spawn the `mob` instance by adding it to the `Main.tscn` scene with the ->
        //-> `Node` Class instance function (the instance being the instance of the `Main` Class)
        AddChild(mob);
        //Below `if-statement` is for decreasing the time it takes for `Mob` instances to ->
        //-> spawn in the scene as time goes on, increasing the difficulty
        mobCount++;
        if(mobCount >= 5){
            mobCount = 0;
            if(mobTimer.WaitTime >= 1){
                mob.minSpeed = mob.minSpeed + 5;
                mob.maxSpeed = mob.maxSpeed + 5;
                mobTimer.WaitTime = mobTimer.WaitTime - 0.2f;
            }
        }

        //Unlike `Player` instances (like below), we instantiate `Mob` instances from the ->
        //-> code, meaning we can not connect the `Mob` instance signal to the `ScoreLabel` ->
        //-> instance (node) via the editor
        //Instead we have to make the connection from the code everytime we spawn a `Mob` instance
        //We connect the `Mob` instance to the `ScoreLabel` instance (node) to update the ->
        //-> score after a `Mob` instance is "squashed"
        mob.Connect("SquashedEventHandler", GetNode<ScoreLabel>("UserInterface/ScoreLabel"), "_on_Mob_SquashedEventHandler"); 
    }

    public void _on_Player_HitEventHandler(){
        retryRect.Show();
        mobTimer.Stop();
    }

    //If the `ColorRect` instance (node) `retryRect` is visible we need to listen to the ->
    //-> player's input and restart the game if they press enter. To do this we use the ->
    //-> built-in `_UnhandledInput()` callback which is triggered on any input
    //So if the player pressed the predefined `ui_accept` input action and the `retryRect` ->
    //-> `ColorRect` instance (node) is visible, then we reload the current scene
    // public override void _UnhandledInput(InputEvent @event)
    // {
    //     if(@event.IsActionPressed("ui_accept") && retryRect.Visible == true){
    //         //The `SceneTree` instance function `ReloadCurrentScene()` reloads the currently ->
    //         //-> active scene
    //         GetTree().ReloadCurrentScene();
    //     }
    // }
    //IMPORTANT NOTE: `_Input(InputEvent @event)` seemed to be the only one wanting to work ->
    //-> with the left mouse click, `UnhandledInput(InputEvent @event)` did not want ->
    //-> to work with the left mouse click
    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_accept") && retryRect.Visible == true || @event.IsAction("left_click") && retryRect.Visible == true){
            //The `SceneTree` instance function `ReloadCurrentScene()` reloads the currently ->
            //-> active scene
            GetTree().ReloadCurrentScene();
        }
    }

    public void Initialize(){
        UpdateMobCountLabel();
    }
    public void UpdateMobCountLabel(){
        highscoreLabel.Text = "Highscore: " + saveLoadJSON.GetMob();
    }
    public void _on_Main_tree_exiting(){
        GD.Print("Tree Exiting");
        
        if(scoreLabel.score > saveLoadJSON.GetMob()){
            saveLoadJSON.AddMob(scoreLabel.score);
            saveLoadJSON.Save();
        }
    }
}
