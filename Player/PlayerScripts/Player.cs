using Godot;
using System;

public class Player : KinematicBody
{   
    //`Signal` to emitt when a `Player` instance is hit by a `Mob` instance on flat ground
    [Signal]
    public delegate void HitEventHandler();

    //Movement speed `Speed` instance variable in meters per second
    [Export]
    public int speed {get; set;} = 14;
    //The downward acceleration when in the air `fallAcceleration` instance variable, in meters per second squared
    [Export]
    public int fallAcceleration {get; set;} = 75;
    //The verticle impulse (force at a moment in time) applied to the `Player` instance ->
    //-> when jumping, in meters per second
    [Export]
    public int jumpImpulse {get; set;} = 20;
    //The verticle impulse (force at a moment in time) applied to the `Player` instance ->
    //-> after bouncing on a `Mob` instance, in meters per second
    [Export]
    public int bounceImpulse {get; set;} = 18;
    //Velocity we'll use to move the `Player` instance
    //NOTE: here the 3D vector `targetVelocity` is a property (instance variable) because ->
    //-> we want to update and reuse its value across frames
    private Vector3 targetVelocity = Vector3.Zero;
    private Spatial pivotNode;
    private AnimationPlayer animationController;
    private float comboCount = 0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        pivotNode = GetTree().Root.GetChild(1).GetChild(2).GetChild<Spatial>(0);
        animationController = GetTree().Root.GetChild(1).GetChild(2).GetChild<AnimationPlayer>(2);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }

    //The instance function (virtual function) `_PhysicsProcess(float delta)` is just like the ->
    //-> instance function (virtual function) `_Process(float delta)` but it is made ->
    //-> specifically for physics-related code like moving a kinematic or rigid body
    public override void _PhysicsProcess(float delta)
    {
        //Local variable `direction` to store the input direction
        Vector3 direction = Vector3.Zero;

        //We check for each move input and update the direction accordingly
        //NOTE: We are working with the 3D vector's `x` and `z` axes because in 3D the ->
        //-> "XZ plane" is the "ground plane"
        if(Input.IsActionPressed("move_right")){
            direction.x += 1.0f;
        }
        if(Input.IsActionPressed("move_left")){
            direction.x -= 1.0f;
        }
        if(Input.IsActionPressed("move_back")){
            direction.z += 1.0f;
        }
        if(Input.IsActionPressed("move_forward")){
            direction.z -= 1.0f;
        }
        //GD.Print(direction);
        //GD.Print(direction.Length());

        //Below is an `if-statement` to normalize the `direction` `Vector3` instance ->
        //-> to a unit vector magnitude of `1` (this is because if the player holds ->
        //-> the "left" and "forward" movement keys simultaneously, then the magnitude ->
        //-> of the resulting vector would be `1.4` which means moving diagonally is faster ->
        //-> than regular movement without normalizing the `direction` `Vector3` instance ->
        //-> vector first)
        //SPECIAL NOTE: Thanks Faye for the help!
        //NOTE: We only normalize the `direction` `Vector3` instance if it has a length ->
        //-> greater than zero, a length of zero means the player is not moving and ->
        //-> you cannot normalize `0` (divide by `0`)
        if(direction != Vector3.Zero){
            direction = direction.Normalized();

            //Now we call the `Pivot` instance (node) of the `Spatial` Class and use its ->
            //-> instance function `LookAt()` which rotates the instance (node) so that ->
            //-> the local "forward axis" (-Z) points toward the `direction` instance ->
            //-> of the `Vector3` Class and takes the local up axis (+Y) and points it as ->
            //-> close to the up vector (`Vector3.Up`) while staying perpendicular to the ->
            //-> local forward axis (-Z)
            //GetNode<Spatial>("Pivot").LookAt(direction, Vector3.Up);
            //GD.Print("THIS" + " " + direction);
            pivotNode.LookAt(Translation + direction, Vector3.Up);
            //Below line of code will make the `float` animation play faster if the ->
            //-> `Player` instance is moving
            animationController.PlaybackSpeed = 4;
        }
        else{
            //Below line of code will make the `float` animation play slower if the `Player` ->
            //-> instance is not moving
            animationController.PlaybackSpeed = 1;
        }

        //Now that we have the 3D model pointing in the correct direction, we now to need ->
        //-> calculate and update the "ground velocity"
        //Ground Velocity:
        targetVelocity.x = direction.x * speed;
        targetVelocity.z = direction.z * speed;

        //Resets the `Player` instance variable `comboCount` if the player touches the ground

        //Code below is for "jumping"
        //NOTE: The `IsOnFloor()` function is an instance function from the `KinematicBody` ->
        //-> Class and returns true if the the body collidied with the `floor` (`ground`) ->
        //-> instance this frame
        //IMPORTANT NOTE: How does the `IsOnFloor()` instance function know what is the "floor" ->
        //-> Well in reality it does not really mark anything as the floor, instead it ->
        //-> uses the `Vector3.Up` direction you provide as the, well, "up" direction and then ->
        //-> the `IsOnFloor()` instance function just tells you if your `KinematicBody` instance ->
        //-> has been touched anywhere on the bottom of its `CollisionShape` instance variable ->
        //-> So in reality the engine just predicts that anything blocking the `KinematicBody` ->
        //-> instance from below is the "floor"
        //NOTE: This is why I had to add the `Vector3.Up` argument to the ->
        //-> `MoveAndSlide(targetVelocity, Vector3.Up)` `KinematicBody` instance function ->
        //-> below because the engine did not know which way was up without it and kept ->
        //-> running the "falling" `if-statement` because it thought it was not touching ->
        //-> the floor (ground)
        if(IsOnFloor() && Input.IsActionPressed("jump")){
            //IMPORTANT NOTE: Notice how the `y` axis is positive in the upward direction where ->
            //-> in 2D the `y` axis is positive in the downward direction
            animationController.Stop();
            targetVelocity.y = jumpImpulse;
            //GD.Print("Jumping");
        }

        //Godot makes the `body` move multiple times in a row sometimes, to smooth out the ->
        //-> character's motion, this means we have to loop over all collisions that may ->
        //-> have happened in these moves
        //That is why below we have to check if the `Player` instance landed on a `Mob` ->
        //-> instance in every iteration of the loop and if the `Player` instance did, we ->
        //-> `QueueFree()` the `Mob` instance and bounce the `Player` instance with the ->
        //-> `bounceImpulse` instance variable
        //NOTE: The `for-loop` is set up so that if no collisions occured on a given frame ->
        //-> the `for-loop` wont run at all
        //NOTE: The `KinematicBody` instance function `GetSlideCount()` returns the number of ->
        //-> times the `body` collided and changed direction during the last call to ->
        //-> `KinematicBody.MoveAndSlide()` or `KinematicBody.MoveAndSlideWithSnap()`
        //Iterate through all collisions that occured this frame
        for(int i = 0; i < GetSlideCount(); i++){
            //We get one of the collisions with the `Player` instance
            KinematicCollision collision = GetSlideCollision(i);
            //GD.Print(collision.Collider.GetType());

            //If the `collision` `KinematicCollision` instance is with a `Mob` instance ->
            //-> and manipulating it using the `mob` group
            if(collision.Collider is Mob mob){
                //We check to make sure that the `Player` instance is hitting the `Mob` ->
                //-> instance from above and not the sides (or even below which should ->
                //-> not be possible)
                //NOTE: If the resulting "dot product" was less than zero it would mean ->
                //-> the collision happened below the `Mob` instance and if the resulting ->
                //-> "dot product" was exactly zero it would mean that the collision happened ->
                //-> directly to the side of the `Mob` instance
                //NOTE: The "collision normal" is just the direction vector between two objects
                //BETTER EXPLANATION OF COLLISION NORMAL: You can think of the collision normal ->
                //-> as the direction that your collision object will move after the collision ->
                //-> meaning if you have a `Player` instance collide with a `Mob` instance ->
                //-> the resulting collision normal is a vector in the direction pointing ->
                //-> away from the `Mob` instance and into the `Player` instance because ->
                //-> the normal (is always perpendicular to the surface) is the force ->
                //-> acting on the `Player` instance by the `Mob` instance after the collision ->
                //-> So if the `Player` instance collides with the `Mob` instance from below ->
                //-> it will produce a collision normal (normal) pointing downwards but if the ->
                //-> `Player` instance collides with the `Mob` instance from above it will ->
                //-> produce a collision normal (normal) pointing upwards
                if(Vector3.Up.Dot(collision.Normal) > 0.5f){
                    //If so we run the `Squash()` `Mob` instance function and bounce the ->
                    //-> `Player` instance
                    GD.Print(collision.Normal);
                    //GD.Print(mob.GetInstanceId());
                    comboCount += 1.0f;
                    mob.Squash(comboCount);
                    targetVelocity.y = bounceImpulse;
                }
                else{
                    die();
                }
            }
            else{
                comboCount = 0.0f;
            }
        }

        //Vertical Velocity:
        if(!IsOnFloor()){//If in the air, fall towards the floor like gravity (adding gravity)
            targetVelocity.y -= fallAcceleration * delta;
            //Below line of code make the `Player` instance arc when jumping by manipulating ->
            //-> the `playerModel` instance (node)
            //IMPORTANT NOTE: How do we do this while an animation is playing?
            //The `pivoteNode` instance (node) allows us to layer transforms on top of the ->
            //-> animation since the `AnimationPlayer` instance (node) is affecting an instance ->
            //-> (node) that is a child of it (that being the `playerModel` instance (node))
            pivotNode.Rotation = new Vector3(Mathf.Pi/6.0f * targetVelocity.y / jumpImpulse, pivotNode.Rotation.y, pivotNode.Rotation.z);
            //GD.Print("Falling");
        }
        else{
            //Below line of code starts the `AnimationPlayer` instance's (node) animation ->
            //-> again when it touches the ground, this stops the awkward mix of the ->
            //-> `float` animation playing while the `Player` instance is in the air still
            animationController.Play();
        }
        //Since this is a `KinematicBody` Class instance (node) it comes with instance functions ->
        //-> to move the `KinematicBody` instance (in this case `Player`) along a vector
        //NOTE: If the `KinematicBody` collides with another "body", it will ->
        //-> slide along the other "body" instead of stopping immediately. If the other ->
        //-> "body" is a `KinematicBody` or `RigidBody`, it will also be affected by the ->
        //-> motion of the other body
        MoveAndSlide(targetVelocity, Vector3.Up);
    }

    public void die(){
        EmitSignal("HitEventHandler");
        QueueFree();
    }
}
