using Godot;
using System;
using System.Linq;
using System.Xml.Xsl;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.Arm;

public partial class PlayerCharacter : CharacterBody3D
{
    [Export]
    public float WalkSpeed { get; set; } = 1f;
    [Export]
    public float RunSpeed = 5f;
    [Export]
    public float HighSpeed = 8f;
    [Export]
    public float TopSpeed = 10f;
    [Export]
    public float TopSpeedTimer = 0f;
    [Export]
    public float TopSpeedThresholdTime = 5f;
    [Export]
    public float JumpTime { get; set; } = 0.35f;
    [Export]
    public float JumpVelocity = 8f;
    [Export]
    public float RunAccelarationTime = 0.3f;
    [Export]
    public float TopAccelarationTime = 0.4f;
    [Export]
    public float ToRunDecelartionTime = 0.2f;
    [Export]
    public float RotationSpeed { get; set; } = 12;
    [Export]
    public float MouseSensitivity { get; set; } = 0.003f;
    [Export]
    public float CoyoteTime = 0.1f;
    [Export]
    public float JumpTimeThreshold = 1.5f;
    [Export]
    public float DissolveTime = 0.8f;

    [Export]
    public double InvulnerabilityTimer = 1;

    [Export]
    public int ComboCounter { get; set; } = 1;
    [Export]
    public int MaxHealth { get; set; } = 3;
    [Export]
    public int CurrentHealth { get; set; } = 3;
    [Export]
    public int Lifes { get; set; } = 3;
    [Export]
    public int JoystickSensitivity { get; set; } = 5;

    [Export]
    public Hud Hud;


    public TransitionScreen TransitionScreen;

    public Vector3 SpawnPoint;
    public Vector3 SpawnRoration;
    public Vector3 PlayerVelocity;
    public Vector3 Gravity;
    public Vector3 Direction;
    public Vector3 PlatformVelocity;

    public Vector2 InputDir;

    public Node3D CameraController;

    public bool CanInput = true;
    public bool HasDoubleJump = true;
    public bool IsHurt = false;
    public bool IsInvulnerable = false;
    public bool HasWon = false;
    public bool ShallBounce = false;

    public float MovementSpeed;
    public float TimeSinceLeftFloor = 0f;
    public float WallTime = 0.5f;
    public float TimeOnWall = 0f;
    public float JumpWeight;

    //Min and max vertical rotation
    const float minPitch = -45f;
    const float maxPitch = 75f;

    //Min and max Yaw rotation
    const float minYaw = -75f;
    const float maxYaw = 75f;

    public float DissolveValue;

    public float StopRate;
    public float RunAcellerationRate;
    public float TopAcellerationRate;
    public float ToRunDecelerationRate;

    public int HitStagger { get; set; } = 8;

    public AnimationTree AnimationTree { get; set; }

    public AnimationNodeStateMachinePlayback AnimationNodeState { get; set; }

    public Global Global;

    public SoundManager SoundManager;

    public RayCast3D WallDetector;

    [Signal]
    public delegate void PlayerHitEventHandler();

    double currentInvulnerabilityTime;

    float blinkInterval;

    bool isDead = false;
    bool isControllerInput;
    bool isBlinking = false;
    bool hasTopSpeed = false;

    RayCast3D groundDetector;

    MeshInstance3D meshDonnaHead;

    Color originColour;
    Color damageColour;

    Transform3D xTransform;

    Node trails;

    Node3D sounds;
    Node3D rigDonna;
    Node3D cameraTarget;

    Node3D[] thrusters;
    Node3D[] footLightTrails;

    Area3D[] hitboxes;
    Area3D[] hurtboxes;

    MeshInstance3D[] donnaMeshs;

    GpuParticles3D grassTrailL;
    GpuParticles3D grassTrailR;

    RandomNumberGenerator rng = new RandomNumberGenerator();

    AudioStreamPlayer3D soundJump;
    AudioStreamPlayer3D soundDrop;
    AudioStreamPlayer3D soundSkate;
    AudioStreamPlayer3D soundGetHit;
    AudioStreamPlayer3D soundTopSpeed;
    AudioStreamPlayer3D soundDissolve;
    AudioStreamPlayer3D soundDoubleJump;
    AudioStreamPlayer3D soundLandOnGrass;
    AudioStreamPlayer3D soundWalkOnGrass;

    public override void _Ready()
    {
        //Get nodes
        Global = GetNode<Global>("/root/Global");
        SoundManager = GetNode<SoundManager>("/root/SoundManager");
        TransitionScreen = GetNode<TransitionScreen>("/root/TransitionScreen");

        CameraController = GetNode<Node3D>("Camera_Controller");
        WallDetector = GetNode<RayCast3D>("WallDetector");

        cameraTarget = CameraController.GetNode<Node3D>("Camera_Target");
        groundDetector = GetNode<RayCast3D>("GroundDetector");
        rigDonna = GetNode<Node3D>("rig_Donna");
        meshDonnaHead = rigDonna.GetNode<Skeleton3D>("Skeleton3D").GetNode<MeshInstance3D>("Donna_Head");

        sounds = GetNode<Node3D>("Sounds");
        soundDrop = sounds.GetNode<AudioStreamPlayer3D>("Drop");
        soundJump = sounds.GetNode<AudioStreamPlayer3D>("Jump");
        soundSkate = sounds.GetNode<AudioStreamPlayer3D>("Skate");
        soundGetHit = sounds.GetNode<AudioStreamPlayer3D>("GetHit");
        soundDissolve = sounds.GetNode<AudioStreamPlayer3D>("Dissolve");
        soundTopSpeed = sounds.GetNode<AudioStreamPlayer3D>("TopSpeed");
        soundDoubleJump = sounds.GetNode<AudioStreamPlayer3D>("DoubleJump");
        soundLandOnGrass = sounds.GetNode<AudioStreamPlayer3D>("LandOnGrass");
        soundWalkOnGrass = sounds.GetNode<AudioStreamPlayer3D>("WalkOnGrass");

        trails = GetNode<Node3D>("Trails");
        grassTrailL = trails.GetNode<Node3D>("GrassTrailL").GetNode<GpuParticles3D>("GrassExplosion");
        grassTrailR = trails.GetNode<Node3D>("GrassTrailR").GetNode<GpuParticles3D>("GrassExplosion");

        thrusters = GetTree().GetNodesInGroup("Thrusters").Cast<Node3D>().ToArray();
        hitboxes = GetTree().GetNodesInGroup("HitBoxesPlayer").Cast<Area3D>().ToArray();
        hurtboxes = GetTree().GetNodesInGroup("HurtBoxesPlayer").Cast<Area3D>().ToArray();
        footLightTrails = GetTree().GetNodesInGroup("FootLightTrails").Cast<Node3D>().ToArray();
        donnaMeshs = GetTree().GetNodesInGroup("DonnaMesh").Cast<MeshInstance3D>().ToArray();

        //AnimationTree = GetNode<AnimationTree>("AnimationTree");
        //AnimationNodeState = (AnimationNodeStateMachinePlayback) AnimationTree.Get("parameters/playback");

        //Set initial values
        blinkInterval = 0.15f;
        originColour = (Color)meshDonnaHead.GetActiveMaterial(0).NextPass.Get("albedo_color");
        damageColour = new Color(1f, 0f, 0f);

        Hud.UpdateHealthbar(CurrentHealth);
        Hud.UpdateLifes(Lifes);
        SpawnPoint = Position;
        SpawnRoration = Rotation;
        Input.MouseMode = Input.MouseModeEnum.Captured;

        //Call important methods
        ResetCamera();

        ControlHurtBoxes(true);
        ControlTrusters(false);
        ControlHitBoxes(false);
        ControlSaltoCollision(false);

        RunAcellerationRate = RunSpeed / RunAccelarationTime;
        TopAcellerationRate = (HighSpeed - RunSpeed) / TopAccelarationTime;
        ToRunDecelerationRate = (HighSpeed - RunSpeed) / ToRunDecelartionTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        //Control camera position manually
        Vector3 characterPosition = Position;
        Vector3 characterRotation = Rotation;

        //Get camera controller position
        Vector3 cameraPosition = CameraController.Position;
        Vector3 cameraRotation = CameraController.Rotation;

        if (IsInvulnerable)
        {
            currentInvulnerabilityTime += delta;

            if (!isBlinking && IsHurt)
            {
                blinking(originColour, damageColour, blinkInterval);
            }

            if (currentInvulnerabilityTime >= InvulnerabilityTimer)
            {
                ResetInvulnerability();
            }
        }

        var factor = CalculateSpeedBoost();
        DetermineTopSpeedConditions(factor, (float) delta);

        HitWall();

        if (MovementSpeed > RunSpeed)
        {
            ControlLightTrail(false);
        }
        else
        {
            ControlLightTrail(true);
        }

        alignWithFloor(groundDetector.GetCollisionNormal());

        InputDir = GetInputDirection();

        CalculatePlatformVelocity();

        Velocity = PlayerVelocity;

        controlGamepadCamera(delta);

        AlignCamera(cameraPosition, characterPosition, delta);

        MoveAndSlide();
    }

    //public override void _Process(double delta)
    //{
    //    controlGamepadCamera(delta);
    //}

    public override void _Input(InputEvent @event)
    {
        isControllerInput = @event is InputEventJoypadButton || @event is InputEventJoypadMotion;

        if (@event is InputEventMouseMotion motion)
        {
            CameraController.RotateY(-motion.Relative.X * MouseSensitivity);

            //if (inputDir == Vector2.Zero)
            //{
            //	//Control min and max Yaw rotation
            //	float yaw = cameraController.RotationDegrees.Y;
            //	yaw -= motion.Relative.X * MouseSensitivity * Mathf.RadToDeg(1f);
            //	yaw = Mathf.Clamp(yaw, MinYaw, MaxYaw);
            //	cameraController.RotationDegrees = new Vector3(cameraController.RotationDegrees.X, yaw, cameraController.RotationDegrees.Z);
            //}

            //Control min and max vertical rotation
            float pitch = cameraTarget.RotationDegrees.X;
            pitch -= motion.Relative.Y * MouseSensitivity * Mathf.RadToDeg(1f);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraTarget.RotationDegrees = new Vector3(pitch, cameraTarget.RotationDegrees.Y, cameraTarget.RotationDegrees.Z);
        }
    }

    private void controlGamepadCamera(double delta)
    {
        Vector2 joyStickInput = Input.GetVector("look_left", "look_right", "look_down", "look_up");

        if (joyStickInput.Length() > 0.1f)
        {
            CameraController.RotateY(-joyStickInput.X * JoystickSensitivity * (float)delta);

            float pitch = cameraTarget.RotationDegrees.X;
            pitch -= joyStickInput.Y * JoystickSensitivity * Mathf.RadToDeg(1f) * (float)delta;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraTarget.RotationDegrees = new Vector3(pitch, cameraTarget.RotationDegrees.Y, cameraTarget.RotationDegrees.Z);
        }
    }

    private async void blinking(Color originColour, Color blinkColor, float blinkInterval)
    {
        isBlinking = true;

        while (IsInvulnerable)
        {
            meshDonnaHead.GetActiveMaterial(0).NextPass.Set("albedo_color", blinkColor);
            await ToSignal(GetTree().CreateTimer(blinkInterval), SceneTreeTimer.SignalName.Timeout);

            meshDonnaHead.GetActiveMaterial(0).NextPass.Set("albedo_color", originColour);
            await ToSignal(GetTree().CreateTimer(blinkInterval), SceneTreeTimer.SignalName.Timeout);
        }

        isBlinking = false;
        meshDonnaHead.GetActiveMaterial(0).NextPass.Set("albedo_color", originColour);
    }

    public void AlignCamera(Vector3 cameraPosition, Vector3 characterPosition, double delta)
    {
        //Get Yaw camera degree
        Vector3 camForward = -CameraController.GlobalTransform.Basis.Z;
        camForward.Y = 0;
        camForward = camForward.Normalized();

        //Get Yaw camera degree
        Vector3 camYaw = -CameraController.GlobalTransform.Basis.X;
        camYaw.Y = 0;
        camYaw = camYaw.Normalized();

        Direction = (camYaw * InputDir.X + camForward * InputDir.Y).Normalized();

        //if (direction != Vector3.Zero)
        //{
        //    if (InputDir.Y < 0)
        //    {
        //        //Rotate player to camera angle
        //        float targetYaw = Mathf.Atan2(direction.X, direction.Z);
        //        float currentYaw = Rotation.Y;
        //        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 10f * (float)delta);
        //        Rotation = new Vector3(Rotation.X, newYaw, Rotation.Z);
        //    }

        //    //Align camera slowly with palyer movement
        //    if (InputDir.Y > 0)
        //    {
        //        float targetYaw = RotationDegrees.Y;
        //        float currentYaw = CameraController.RotationDegrees.Y;
        //        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 5f * (float)delta);

        //        CameraController.RotationDegrees = new Vector3(CameraController.RotationDegrees.X, newYaw, CameraController.RotationDegrees.Z);
        //    }
        //}

        CameraController.Position = cameraPosition.Lerp(characterPosition, 3f * (float)delta);
    }

    //Align character to floor
    private void alignWithFloor(Vector3 floorNormal)
    {
        if (IsOnFloor())
        {
            calculateAligninWithFloor(groundDetector.GetCollisionNormal());
            GlobalTransform = GlobalTransform.InterpolateWith(xTransform, 0.3f);
        }
        else
        {
            calculateAligninWithFloor(Vector3.Up);
            GlobalTransform = GlobalTransform.InterpolateWith(xTransform, 0.3f);
        }
    }

    private void calculateAligninWithFloor(Vector3 floorNormal)
    {
        xTransform = GlobalTransform;
        xTransform.Basis.Y = floorNormal;
        xTransform.Basis.X = -xTransform.Basis.Z.Cross(floorNormal);
        xTransform.Basis = xTransform.Basis.Orthonormalized();
    }

    public void CalculateMaximumJumpHeigh(float weight)
    {
        CalculateMaximumJumpHeigh(weight, JumpVelocity);
    }

    public void CalculateMaximumJumpHeigh(float weight, float jumpheight)
    {
        var newJumpHeight = Mathf.Lerp(PlayerVelocity.Y, jumpheight, weight);
        PlayerVelocity = new Vector3(PlayerVelocity.X, (float)newJumpHeight, PlayerVelocity.Z);
    }

    public void CalculateMovementSpeed(double delta)
    {
        if (IsOnFloor())
        {
            calculateGroundMovementSpeed(delta);
        }
        else
        {
            calculateJumpMovementSpeed(delta);
        }
    }

    private void calculateGroundMovementSpeed(double delta)
    {
        if (Input.IsActionPressed("Accelerate") && !WallDetector.IsColliding())
        {
            if (TopSpeedTimer > TopSpeedThresholdTime)
            {
                MovementSpeed = Mathf.MoveToward(MovementSpeed, TopSpeed, TopAcellerationRate * (float)delta);
            }
            else
            {
                MovementSpeed = Mathf.MoveToward(MovementSpeed, HighSpeed, TopAcellerationRate * (float)delta);
            }
        }
        else
        {
            MovementSpeed = Mathf.MoveToward(MovementSpeed, RunSpeed, RunAcellerationRate * (float)delta);
        }
    }

    private void calculateJumpMovementSpeed(double delta)
    {
        if (Input.IsActionPressed("Accelerate") && !WallDetector.IsColliding() && TimeSinceLeftFloor <= JumpTimeThreshold && MovementSpeed >= HighSpeed && TimeSinceLeftFloor > CoyoteTime)
        {
            if (MovementSpeed > HighSpeed)
            {
                MovementSpeed = Mathf.MoveToward(MovementSpeed, HighSpeed, ToRunDecelerationRate * (float)delta);
            }
        }
        else
        {
            var timeFactor = Mathf.Clamp((TimeSinceLeftFloor / 0.8f), 0.5f, 1.5f);
            MovementSpeed = Mathf.MoveToward(MovementSpeed, RunSpeed, ToRunDecelerationRate * timeFactor * (float)delta);
        }
    }

    public void CalculatePlatformVelocity()
    {
        var collider = GetLastSlideCollision()?.GetCollider() as MovingPlatform;
        if (collider != null)
        {
            PlatformVelocity = collider.Velocity;
        }
    }

    public void CalculateVelocity(Vector2 inputDir, Vector3 direction, double delta)
    {
        if (inputDir != Vector2.Zero)
        {
            CalculateMovementSpeed(delta);

            if (direction != Vector3.Zero)
            {
                PlayerVelocity.X = direction.X * MovementSpeed;
                PlayerVelocity.Z = direction.Z * MovementSpeed;

                //Control with keyboard
                if (inputDir.Y < 0)
                {
                    //Rotate player to camera angle
                    float targetYaw = Mathf.Atan2(direction.X, direction.Z);
                    float currentYaw = Rotation.Y;
                    float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 10f * (float)delta);
                    Rotation = new Vector3(Rotation.X, newYaw, Rotation.Z);
                }

                //Align camera slowly with palyer movement
                if (inputDir.Y > 0)
                {
                    float targetYaw = RotationDegrees.Y;
                    float currentYaw = CameraController.RotationDegrees.Y;
                    float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, 5f * (float)delta);

                    CameraController.RotationDegrees = new Vector3(CameraController.RotationDegrees.X, newYaw, CameraController.RotationDegrees.Z);
                }
            }
        }
        else
        {
            //Reduce movement speed when not pressing a button
            var stopRate =
                (MovementSpeed > 6f)
                ? HighSpeed / TopAccelarationTime
                : HighSpeed / RunAccelarationTime;

            MovementSpeed = Mathf.MoveToward(MovementSpeed, 0f, stopRate * (float)delta);

            PlayerVelocity.X = Mathf.MoveToward(Velocity.X, 0, stopRate * (float)delta);
            PlayerVelocity.Z = Mathf.MoveToward(Velocity.Z, 0, stopRate * (float)delta);
        }
    }

    public void ControlLightTrail(bool isHidden)
    {

        foreach (Node3D trail in footLightTrails)
        {
            if (isHidden)
            {
                trail.Hide();
            }
            else
            {
                trail.Show();
            }
        }
    }

    public void FireGrassTrail(string side)
    {
        GpuParticles3D trail = side == "l" ? grassTrailL : grassTrailR;

        trail.Restart();
        trail.Amount = (int)MovementSpeed * 2;
        trail.Emitting = true;
    }

    public void ControlTrusters(bool onOff)
    {
        for (int i = 0; i < thrusters.Length; i++)
        {
            var thruster = thrusters[i].GetNode<GpuParticles3D>("Fire");
            if (thruster != null)
            {
                thruster.Emitting = onOff;
            }
        }
    }

    public void ControlHurtbox(Area3D hurtbox, bool onOff)
    {
        var collisionBox = hurtbox.GetChild<CollisionShape3D>(0);

        hurtbox.Monitorable = onOff;
        collisionBox.Disabled = !onOff;

        hurtbox.SetCollisionLayerValue(6, onOff);
        hurtbox.SetCollisionMaskValue(5, onOff);
        hurtbox.SetCollisionMaskValue(9, onOff);
    }

    public void ControlHurtBoxes(bool onOff)
    {
        foreach (Area3D hurtbox in hurtboxes)
        {
            ControlHurtbox(hurtbox, onOff);
        }
    }

    public void ControlHitbox(Area3D hitbox, bool onOff)
    {
        var collisionBox = hitbox.GetChild<CollisionShape3D>(0);

        hitbox.Monitorable = onOff;
        collisionBox.Disabled = !onOff;

        hitbox.SetCollisionLayerValue(7, onOff);
        hitbox.SetCollisionMaskValue(8, onOff);
    }

    public void ControlHitBoxes(bool onOff)
    {
        foreach (Area3D hitbox in hitboxes)
        {
            ControlHitbox(hitbox, onOff);
        }
    }

    public void ControlFeetCollision(bool onOffHurt, bool onOffHit)
    {
        var feetHitboxes = hitboxes.Where(hitbox => hitbox.Name.ToString().ToLower().Contains("foot"));
        var feetHurtboxes = hurtboxes.Where(hurtBox => hurtBox.Name.ToString().ToLower().Contains("foot"));

        foreach (Area3D footHurtbox in feetHurtboxes)
        {
            ControlHurtbox(footHurtbox, onOffHurt);
        }

        foreach (Area3D footHitbox in feetHitboxes)
        {
            ControlHitbox(footHitbox, onOffHit);
        }
    }

    public void ControlSaltoCollision(bool onOffSalto)
    {
        var saltoHitboxes = hitboxes.Where(hitbox => hitbox.Name.ToString().ToLower().Contains("salto"));

        foreach (Area3D saltoHitbox in saltoHitboxes)
        {
            ControlHitbox(saltoHitbox, onOffSalto);
        }
    }

    public void FireExplosion(float scaling = 0.2f)
    {
        if(InputDir == Vector2.Zero)
        {
            Global.CreateGrassExplosion(Position, scaling);
        }
        else
        {
            var moveDir = (-GlobalTransform.Basis.X * InputDir.X + -GlobalTransform.Basis.Z * InputDir.Y).Normalized();
            var cloudPosition = Position + moveDir * 0.5f;

            Global.CreateGrassExplosion(cloudPosition, scaling);
        }
    }

    public async Task GameOver(string nextScene)
    {
        var sceneTree = GetTree();

        var timer = sceneTree.CreateTimer(0.1);
        await ToSignal(timer, Timer.SignalName.Timeout);

        if (sceneTree != null)
        {
            LoadScene(sceneTree, nextScene);
        }
    }

    public Vector2 GetInputDirection()
    {
        if (!CanInput)
        {
            return new Vector2();
        }

        var input = Input.GetVector("Left", "Right", "Forward", "Backward");
        return input;
    }

    public bool HasMovementInput()
    {
        return InputDir.X != 0 || InputDir.Y != 0;
    }

    public void LoadScene(SceneTree sceneTree, string sceneName)
    {
        foreach (Node3D trail in footLightTrails)
        {
            if (IsInstanceValid(trail))
            {
                trail.QueueFree();
            }
        }

        if (sceneTree != null)
        {
            sceneTree.CallDeferred("change_scene_to_file", sceneName);
        }
    }

    public bool IsAtTop(float delta)
    {
        return (PlayerVelocity.Y <= (Gravity * delta).Y);
    }

    public bool ShallRespawn()
    {
        return CurrentHealth <= 0 && Lifes > 0;
    }

    public bool HasLost()
    {
        return Lifes <= 0;
    }

    public void OnHurtboxEntered(Area3D hitbox)
    {
        if(isDead)
        {
            return;
        }
        
        if (hitbox.Name == "FallZone")
        {
            isDead = true;
            CallDeferred(nameof(handleFallZone));
        }
        else if(!IsInvulnerable)
        {
            Global.SetDeathType(Deathypes.Slimes);
            IsHurt = true;
            IsInvulnerable = true;
        }
    }

    private void handleFallZone()
    {
        Global.StopTimer();
        soundDrop.Play();
        ReduceLife();
        Hud.UpdateLifes(Lifes);

        if (Lifes > 0)
        {
            CurrentHealth = 0;
            Hud.UpdateHealthbar(CurrentHealth);
        }

        Global.SetDeathType(Deathypes.Drop);
    }
    
    public void PlaySoundDoubleJump()
    {
        soundDoubleJump.PitchScale = 1.5f * rng.RandfRange(0.9f, 1.1f);
        soundDoubleJump.Play();
    }

    public void PlaySoundDrop()
    {
        soundDrop.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundDrop.Play();
    }

    public void PlaySoundJump()
    {
        soundJump.PitchScale = 1.5f * rng.RandfRange(0.9f, 1.1f);
        soundJump.Play();
    }

    public void PlaySoundSkate()
    {
        soundSkate.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundSkate.Play();
    }

    public void PlaySoundOnHit()
    {
        soundGetHit.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundGetHit.Play();
    }

    public void PlaySoundGameOver()
    {
        if (SoundManager != null)
        {
            SoundManager.PlaySoundLose();
        }
    }

    public void PlaySoundLandOnGrass()
    {
        soundLandOnGrass.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundLandOnGrass.Play();
    }

    public void PlaySoundWalkOnGrass()
    {
        soundWalkOnGrass.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundWalkOnGrass.Play();
    }

    public void PlaySoundDissolve()
    {
        soundDissolve.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundDissolve.Play();
    }

    public void PlaySoundTopSpeed()
    {
        soundTopSpeed.PitchScale = rng.RandfRange(0.9f, 1.1f);
        soundTopSpeed.Play();
    }

    public void AddHealth()
    {
        CurrentHealth += 1;
        Hud.UpdateHealthbar(CurrentHealth);
    }

    public void ReduceHealth()
    {
        CurrentHealth -= 1;
        Hud.UpdateHealthbar(CurrentHealth);
    }

    public void AddLife()
    {
        Lifes += 1;
    }

    public void ReduceLife()
    {
        Lifes -= 1;
    }

    public void ReadyForInput()
    {
        CanInput = true;
    }

    public void Respawn()
    {
        PlayerVelocity = Vector3.Zero;
        InputDir = Vector2.Zero;
        CurrentHealth = MaxHealth;
        Rotation = SpawnRoration;
        Position = SpawnPoint;
        ResetCamera();
        ResetInvulnerability();
        Hud.UpdateHealthbar(MaxHealth);
        isDead = false;
    }

    public void ResetCamera()
    {
        CameraController.Position = Position;
        CameraController.Rotation = Rotation;
    }

    public void ResetInvulnerability()
    {
        IsHurt = false;
        IsInvulnerable = false;
        currentInvulnerabilityTime = 0f;
    }

    public void SetSpawnParameters(Vector3 spawnPoint, Vector3 spawnRotation)
    {
        SpawnPoint = spawnPoint;
        SpawnRoration = spawnRotation;
    }

    public void HitWall()
    {
        if (MovementSpeed > 6f && WallDetector.IsColliding())
        {
            PlaySoundOnHit();
            MovementSpeed = 0f;
            Velocity = new Vector3(0f, Velocity.Y, 0f);
        }
    }

    public float CalculateSpeedBoost()
    {
        if (groundDetector.IsColliding() && MovementSpeed > 6f)
        {
            var normal = groundDetector.GetCollisionNormal();
            Vector3 forward = GlobalTransform.Basis.Z.Normalized();

            Vector3 slopeDir = normal.Cross(normal.Cross(Vector3.Up)).Normalized();

            float slopeAngle = Mathf.RadToDeg(Mathf.Acos(normal.Dot(Vector3.Up)));
            float downHillDot = slopeDir.Dot(forward);

            if (downHillDot > 0.5f && slopeAngle > 25f)
            {
                return 2f;
            }
            else if (downHillDot > 0.5f && slopeAngle > 25f)
            {
                return 0.5f;
            }
            else
            {
                return 1f;
            }
        }
        else
        {
            return 1f;
        }
    }

    public void DetermineTopSpeedConditions(float factor, float delta)
    {
        if (TopSpeedTimer < 0)
        {
            TopSpeedTimer = 0;
        }
        
        if (TopSpeedTimer > 5.5f)
        {
            TopSpeedTimer = 5.5f;
        }

        if(IsHurt || (IsOnWall() && WallDetector.IsColliding()) || InputDir.Y >= 0)
        {
            hasTopSpeed = false;
            TopSpeedTimer = 0;
            UpdateSpeedGauge();
        }
        
        if(MovementSpeed >= 6f && TopSpeedTimer < 5.5f)
        {
            TopSpeedTimer += delta * factor;
            UpdateSpeedGauge();

            if (!hasTopSpeed && TopSpeedTimer >= TopSpeedThresholdTime)
            {
                hasTopSpeed = true;
                PlaySoundTopSpeed();
            }
        }
        else if (TopSpeedTimer > 0 && (IsOnFloor() || TimeSinceLeftFloor > JumpTimeThreshold))
        {
            TopSpeedTimer -= delta;
            UpdateSpeedGauge();
        }
    }

    public void UpdateSpeedGauge()
    {
        Hud.UpdateSpeedGauge(TopSpeedTimer);
    }

    public void ReplaceWithDissolveShader()
    {
        var dissolveShaderDonna = GD.Load<ShaderMaterial>("res://Shaders/DonnaDissolveShader.tres");
        var dissolveShaderDonnaEyeLeft = GD.Load<ShaderMaterial>("res://Shaders/DonnaEyeLeftDissolveShader.tres");
        var dissolveShaderDonnaEyeRight = GD.Load<ShaderMaterial>("res://Shaders/DonnaEyeRightDissolveShader.tres");

        foreach (MeshInstance3D mesh in donnaMeshs)
        {
            var count = mesh.GetSurfaceOverrideMaterialCount();
            for (int i=0; i<count; i++)
            {
                if (mesh.Name == "Donna_Eyes")
                {
                    if(i == 0)
                    {
                        mesh.SetSurfaceOverrideMaterial(i, dissolveShaderDonnaEyeRight);
                    }
                    else
                    {
                        mesh.SetSurfaceOverrideMaterial(i, dissolveShaderDonnaEyeLeft);
                    }
                }
                else
                {
                    mesh.SetSurfaceOverrideMaterial(i, dissolveShaderDonna);
                }
            }
        }
    }

    public void Dissolve(float time, float from, float to)
    {
        DissolveValue += time;

        float progress = Mathf.Lerp(from, to, DissolveValue / DissolveTime);
        progress = Mathf.Clamp(progress, -1f, 1f);

        var dissolveShaderDonna = GD.Load<ShaderMaterial>("res://Shaders/DonnaDissolveShader.tres");
        var dissolveShaderDonnaOutline = GD.Load<ShaderMaterial>("res://Shaders/DonnaOutlineDissolveShader.tres");

        var dissolveShaderDonnaEyeLeft = GD.Load<ShaderMaterial>("res://Shaders/DonnaEyeLeftDissolveShader.tres");
        var dissolveShaderDonnaEyeRight = GD.Load<ShaderMaterial>("res://Shaders/DonnaEyeRightDissolveShader.tres");

        if (DissolveValue > 1f)
        {
            DissolveValue = 1f;
            progress = 1f;
        }
        
        if (DissolveValue < -1f)
        {
            DissolveValue = -1f;
            progress = -1f;
        }

        dissolveShaderDonna.SetShaderParameter("Dissolve", progress);
        dissolveShaderDonnaOutline.SetShaderParameter("Dissolve", progress);

        dissolveShaderDonnaEyeLeft.SetShaderParameter("Dissolve", progress);
        dissolveShaderDonnaEyeRight.SetShaderParameter("Dissolve", progress);
    }

    public void ReplaceWithStandardMaterial()
    {
        var standardMaterial = GD.Load<Material>("res://Materials/donna.tres");
        var standardMaterialLeftEye = GD.Load<Material>("res://Materials/donna_left_eye.tres");
        var standardMaterialRightEye = GD.Load<Material>("res://Materials/donna_right_eye.tres");

        foreach (MeshInstance3D mesh in donnaMeshs)
        {
            var count = mesh.GetSurfaceOverrideMaterialCount();
            for (int i = 0; i < count; i++)
            {
                
                if (mesh.Name == "Donna_Eyes")
                {
                    if (i == 0)
                    {
                        mesh.SetSurfaceOverrideMaterial(i, standardMaterialRightEye);
                    }
                    else
                    {
                        mesh.SetSurfaceOverrideMaterial(i, standardMaterialLeftEye);
                    }
                }
                else
                {
                    mesh.SetSurfaceOverrideMaterial(i, standardMaterial);
                }
            }
        }
    }
}