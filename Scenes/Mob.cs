using Godot;
using System;

public class Mob : KinematicBody
{
    //`Signal` is emmited when a `Player` instance jumps on top of a `Mob` instance
    [Signal]
    public delegate void SquashedEventHandler(float comboCount);

    [Export]
    public int minSpeed {get; set;} = 25;
    [Export]
    public int maxSpeed {get; set;} = 35;
    //private AnimationPlayer animationController;
    private Vector3 velocity;

    // Called when the node enters the scene tree for the first time.
    //IMPORTANT NOTE: Look into pooling for `Mob` instances since I am using C# (which uses ->
    //-> garbage-collection and without using pooling, could lead to crashes)
    public override void _Ready()
    {
        //animationController = GetTree().Root.GetChild(1).GetChild(4).GetChild<AnimationPlayer>(3);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }

    public override void _PhysicsProcess(float delta)
    {
        MoveAndSlide(velocity * delta);
    }
    
    //The `Initialize(Vector 3 startPosition, Vector3 playerPosition)` instance function ->
    //-> below is for turning the `Mob` instance towards the `Player` instance and ->
    //-> randomizing both its angle of motion and velocity. This is all done once on ->
    //-> the `Mob` instances instancing
    public void Initialize(Vector3 startPosition, Vector3 playerPosition){
        //We position the `Mob` instance at `startPosition` and rotate it towards the ->
        //-> `playerPosition` so it looks at the `Player` instance
        LookAtFromPosition(startPosition, playerPosition, Vector3.Up);

        //Rotate the `Mob` instance randomly within a range so that it does not move directly ->
        //-> towards the `Player` instance
        RotateY((float)GD.RandRange(-Mathf.Pi/6.0, Mathf.Pi/6.0));

        //We calculate a `randomSpeed`
        int randomSpeed = (int) GD.RandRange(minSpeed, maxSpeed);

        //We then calculate a forward velocity that represents the `Mob` instances speed
        velocity = Vector3.Forward * randomSpeed;

        //Below line of code makes the `Mob` instances fly:
        //Translation = new Vector3(Translation.x,2.5f,Translation.z);
        //We then rotate the velocity vector based on the `Mob` instances `Y` rotation in ->
        //-> order to move in the direction the `Mob` instance is looking
        //NOTE: In my version of `Godot` `KinematicBody3D` does not have a built in `Velocity` ->
        //-> instance variable so I have to make one as an instance variable in my `Mob` Class ->
        //-> `Mob` instance and then pass that to the `MoveAndSlide()` function as an argument
        velocity = velocity.Rotated(Vector3.Up, Rotation.y);
        // //Below line of code gives every `Mob` instance its own random value for the `PlaybackSpeed` ->
        // //-> instance variable
        GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = randomSpeed/minSpeed;
        // animationController.PlaybackSpeed = randomSpeed/minSpeed;
        // GD.Print(animationController.PlaybackSpeed);
    }

    //Signal function for removing the `Mob` instance once it leaves the screen
    public void _on_VisibilityNotifier_screen_exited(){
        //The `Node` Class function `QueueFree()` queues a `Node` instance for deletion at -> 
        //-> the end of current frame
        //NOTE: When the `Node` instance is deleated so will all of its child `Node` instances
        QueueFree();
    }

    public void Squash(float comboCount){
        EmitSignal("SquashedEventHandler", comboCount);
        QueueFree();
    }
}
