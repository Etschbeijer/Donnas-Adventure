extends SceneTree

# Run with the Godot .NET editor after building the C# project:
# godot --headless --path . --script res://Tests/optimization_checks.gd
var failures := 0

func _initialize():
	call_deferred("_run")

func _check(condition: bool, description: String):
	if condition:
		print("PASS: ", description)
	else:
		failures += 1
		push_error("FAIL: " + description)

func _run():
	if change_scene_to_file("res://Levels/Nodes/level_1.tscn") != OK:
		push_error("Could not load the test level")
		quit(1)
		return
	await process_frame
	await process_frame
	var global_node := root.get_node("Global")
	_check(global_node.get("CurrentLevelScenePath") == "res://Levels/Nodes/level_1.tscn", "Loading level_1 stores its scene path in Global")
	var player = current_scene.get_node("PlayerCharacter")
	player.set_physics_process(false)
	var states = player.get_node("StateMachine")
	states.set_process(false)
	states.set_physics_process(false)
	await _check_bursts(global_node)
	await _check_platform_cycles()
	_check_hud(global_node)
	_check_player_controls(player)
	await _check_special_treats(player, global_node)
	await _check_gameplay_effects(player)
	await _check_falling_platform(player)
	await _check_scene_teardown(player)
	await _check_menu_and_sound(global_node)
	current_scene.queue_free()
	current_scene = null
	await process_frame
	# Flush trail callbacks that the headless renderer cannot advance itself.
	RenderingServer.frame_pre_draw.emit()
	for audio_player in root.get_node("SoundManager").get_children():
		if audio_player is AudioStreamPlayer2D:
			audio_player.stop()
	# The audio mixer thread frees stopped playbacks asynchronously, so quitting sooner leaks them at exit.
	await create_timer(0.5).timeout
	print("Optimization checks finished; failures: ", failures)
	quit(failures)

func _check_bursts(global_node: Node):
	var bursts: Array[Node] = []
	for index in range(50):
		global_node.call("CreateGrassExplosion", Vector3.ZERO, 0.2)
		bursts.append(current_scene.get_child(current_scene.get_child_count() - 1))
	var waiting_for_both := true
	var completed := true
	var all_emitting := true
	for burst in bursts:
		var dust := burst.get_node("DustExplosion")
		var grass := burst.get_node("GrassExplosion")
		all_emitting = all_emitting and dust.emitting and grass.emitting
		# The headless renderer doesn't simulate GPU particles. Drive the same
		# completion signals that Godot sends after the last particle dies.
		dust.emit_signal("finished")
		waiting_for_both = waiting_for_both and not burst.is_queued_for_deletion()
		grass.emit_signal("finished")
		completed = completed and burst.is_queued_for_deletion()
	_check(all_emitting, "Both emitters start in all 50 bursts")
	_check(waiting_for_both, "Bursts wait for both emitters to finish")
	_check(completed, "All 50 completed bursts queue their parent for deletion")
	await process_frame
	var all_freed := true
	for burst in bursts:
		all_freed = all_freed and not is_instance_valid(burst)
	_check(all_freed, "All 50 completed burst nodes are freed")
	# Also cover a culled emitter that never sends its Finished signal.
	global_node.call("CreateGrassExplosion", Vector3.ZERO, 0.2)
	var offscreen_burst := current_scene.get_child(current_scene.get_child_count() - 1)
	await create_timer(1.7).timeout
	_check(not is_instance_valid(offscreen_burst), "Offscreen bursts have bounded lifetime even without GPU completion")

func _check_platform_cycles():
	var platform = load("res://Environment/Nodes/grass_platform_3x_2.tscn").instantiate()
	platform.position = Vector3(50, 20, 50)
	platform.set("startPosition", platform.position)
	platform.set("targetPosition", platform.position + Vector3(2, 0, 0))
	platform.set("travelTime", 0.1)
	platform.set("pauseTime", 0.05)
	var initial_tweens := get_processed_tweens()
	root.add_child(platform)
	platform.call("_PhysicsProcess", 1.0 / 60.0)
	_check(platform.get("Velocity") == Vector3.ZERO, "Platform has no startup velocity spike")
	var platform_tween: Tween
	for tween in get_processed_tweens():
		if tween not in initial_tweens:
			platform_tween = tween
	platform_tween.pause()
	platform.set_physics_process(false)
	var cycle_ends := {"count": 0, "stable": true}
	platform_tween.loop_finished.connect(func(_cycle):
		cycle_ends.count += 1
		cycle_ends.stable = cycle_ends.stable and platform.position.is_equal_approx(platform.get("startPosition"))
	)
	platform_tween.custom_step(0.025)
	_check(platform.position.is_equal_approx(platform.get("startPosition")), "Platform preserves its initial pause")
	for step in range(650):
		platform_tween.custom_step(0.01)
	_check(cycle_ends.count >= 20 and cycle_ends.stable, "Platform completes 20 cycles without position drift")
	_check(get_processed_tweens().size() == initial_tweens.size() + 1, "Platform uses one tween across repeated cycles")
	platform.queue_free()
	await process_frame
	await process_frame
	_check(not platform_tween.is_valid(), "Platform tween stops when its node is freed")

func _check_hud(global_node: Node):
	var hud := current_scene.get_node("HUD")
	var label := hud.get_node("PanelTime/LabelTime")
	global_node.call("StopTimer")
	for value in [[0, 59, "00:59"], [1, 0, "01:00"], [59, 59, "59:59"], [60, 0, "60:00"]]:
		global_node.set("Minutes", value[0])
		global_node.set("Seconds", value[1])
		hud.call("UpdateLabelTime")
		_check(label.text == value[2], "HUD timer transition to " + value[2])
	global_node.call("ResetTimer")
	hud.call("UpdateLabelTime")
	_check(label.text == "00:00", "HUD timer resets on a level restart")
	_check(not hud.is_processing(), "HUD has no empty frame callback")
	for state in current_scene.get_node("PlayerCharacter/StateMachine").get_children():
		_check(not state.is_processing(), "Inactive state has no empty frame callback: " + str(state.name))

func _check_player_controls(player: Node):
	player.call("ControlTrusters", true)
	var emitting := true
	for thruster in get_nodes_in_group("Thrusters"):
		emitting = emitting and thruster.get_node("Fire").emitting
	_check(emitting, "Cached thruster nodes turn on")
	player.call("ControlTrusters", false)
	player.call("ControlFeetCollision", true, false)
	player.call("ControlSaltoCollision", true)
	var collision_matches := true
	for hitbox in get_nodes_in_group("HitBoxesPlayer"):
		if "foot" in str(hitbox.name).to_lower():
			collision_matches = collision_matches and not hitbox.monitorable
		if "salto" in str(hitbox.name).to_lower():
			collision_matches = collision_matches and hitbox.monitorable
	_check(collision_matches, "Cached foot and salto collision groups keep their behavior")
	player.call("ReplaceWithDissolveShader")
	_check_material_assignment("res://Shaders/DonnaDissolveShader.tres", "res://Shaders/DonnaEyeLeftDissolveShader.tres", "res://Shaders/DonnaEyeRightDissolveShader.tres", "Dissolve")
	player.set("DissolveValue", 0.0)
	player.call("Dissolve", 0.2, -1.0, 1.0)
	var expected := lerpf(-1.0, 1.0, 0.2 / float(player.get("DissolveTime")))
	for path in ["DonnaDissolveShader", "DonnaOutlineDissolveShader", "DonnaEyeLeftDissolveShader", "DonnaEyeRightDissolveShader"]:
		var material := load("res://Shaders/" + path + ".tres")
		_check(is_equal_approx(float(material.get_shader_parameter("Dissolve")), expected), "Cached dissolve material updates: " + path)
	player.call("ReplaceWithStandardMaterial")
	_check_material_assignment("res://Materials/donna.tres", "res://Materials/donna_left_eye.tres", "res://Materials/donna_right_eye.tres", "Standard")

func _check_material_assignment(body_path: String, left_path: String, right_path: String, label: String):
	var correct := true
	for mesh in get_nodes_in_group("DonnaMesh"):
		for surface in range(mesh.get_surface_override_material_count()):
			var expected_path := body_path
			if mesh.name == "Donna_Eyes":
				expected_path = right_path if surface == 0 else left_path
			correct = correct and mesh.get_surface_override_material(surface) == load(expected_path)
	_check(correct, label + " materials preserve body and left/right eye assignments")

func _check_special_treats(player: Node, global_node: Node):
	global_node.call("ResetSpecialTreats")
	var collected: Array[Node] = []
	for name in ["Cheese", "Cookie", "Meat"]:
		var treat := current_scene.get_node("SpecialTreats/" + name)
		treat.call("OnBodyEntered", player)
		treat.call("OnBodyEntered", player)
		_check(bool(treat.get("isCollected")), name + " collection is guarded against repeated entry")
		var icon := current_scene.get_node("HUD/PanelSpecialTreats/Texture" + name)
		_check(not icon.show_behind_parent, name + " collection updates its HUD icon")
		treat.get_node("AnimationPlayer").emit_signal("animation_finished", "bounce")
		collected.append(treat)
	await process_frame
	var all_freed := true
	for treat in collected:
		all_freed = all_freed and not is_instance_valid(treat)
	_check(all_freed, "Collected special treats still free through their animation signal")

func _run_rail(player: Node):
	var steps := 0
	while bool(player.get("IsOnRail")) and steps < 600:
		player.call("_physics_process", 1.0 / 60.0)
		steps += 1

func _check_gameplay_effects(player: Node):
	var states = player.get_node("StateMachine")
	var start_transform: Transform3D = player.global_transform
	states.call("TransitionTo", "Idle", null)
	player.set("MovementSpeed", 0.0)

	# Loop rail geometry
	var rail: Node3D = load("res://Environment/Nodes/looping.tscn").instantiate()
	rail.position = Vector3(200, 0, 200)
	current_scene.add_child(rail)
	var radius: float = rail.get("Radius")
	var lead_in: float = rail.get("LeadIn")
	var lead_out: float = rail.get("LeadOut")
	var lateral: float = rail.get("LateralOffset")
	var height: float = rail.get("PlayerOriginHeight")
	var base := rail.position
	var length: float = rail.get("Length")
	_check(length > TAU * radius, "Loop rail length exceeds the ring circumference")
	var first: Transform3D = rail.call("SampleTransform", 0.0)
	_check(first.origin.is_equal_approx(base + Vector3(-lateral / 2.0, height, 0.0)), "Loop rail starts at the lead-in with the player origin above the feet")
	var top: Transform3D = first
	var distance := 0.0
	while distance <= length:
		var sample: Transform3D = rail.call("SampleTransform", distance)
		if sample.origin.y > top.origin.y or distance == 0.0:
			top = sample
		distance += 0.05
	_check(absf(top.origin.y - (base.y + 2.0 * radius - height)) < 0.01 and top.basis.y.dot(Vector3.DOWN) > 0.99, "Loop rail top is upside down at twice the radius")
	var exit_transform: Transform3D = rail.call("ExitTransform")
	_check(exit_transform.origin.is_equal_approx(base + Vector3(lateral / 2.0, height, -lead_in - lead_out)) and exit_transform.basis.y.is_equal_approx(Vector3.UP), "Loop rail exit is upright at the end of the lead-out")

	# Riding the rail
	var started: bool = player.call("TryStartRail", rail)
	_check(started and bool(player.get("IsOnRail")) and str(states.get("CurrentStateName")) == "Looping", "Starting the rail puts the player in the Looping state")
	_run_rail(player)
	var end_state := str(states.get("CurrentStateName"))
	var player_transform: Transform3D = player.global_transform
	_check(not bool(player.get("IsOnRail")) and (end_state == "GroundMovement" or end_state == "AirDropMovement") and player_transform.basis.y.y > 0.99, "Finishing the rail releases the player upright into a movement state")

	# Pads and the speed boost
	var speed_pad: Node = load("res://Environment/Nodes/speed_pad.tscn").instantiate()
	var bounce_pad: Node = load("res://Environment/Nodes/bounce_pad.tscn").instantiate()
	_check(is_equal_approx(float(speed_pad.get("BoostSpeed")), 13.0) and is_equal_approx(float(speed_pad.get("BoostDuration")), 1.5), "Speed pad scene keeps its script exports")
	_check(is_equal_approx(float(bounce_pad.get("LaunchVelocity")), 13.0), "Bounce pad scene keeps its script export")
	speed_pad.free()
	player.call("CancelSpeedBoost")
	states.call("TransitionTo", "Idle", null)
	player.set("MovementSpeed", 0.0)
	var boosted: bool = player.call("TryStartSpeedBoost", 13.0, 1.5, Vector3(0, 0, -1))
	_check(boosted and bool(player.get("IsSpeedBoostActive")) and float(player.get("MovementSpeed")) >= 13.0, "Speed boost starts and raises the movement speed")
	player.call("CancelSpeedBoost")
	player.set("MovementSpeed", 0.0)
	var queued: bool = player.call("TryBounce", float(bounce_pad.get("LaunchVelocity")))
	_check(queued and bool(player.get("ShallBounce")), "Bounce queues a launch")
	var launch: float = player.call("ConsumeBounceVelocity")
	var fallback: float = player.call("ConsumeBounceVelocity")
	_check(is_equal_approx(launch, 13.0) and is_equal_approx(fallback, float(player.get("JumpVelocity")) * 0.7), "Bounce velocity is consumed once, then falls back to the slime bounce")
	bounce_pad.free()
	player.set("ShallBounce", false)

	# Queued bounce, boost redirect, rail abort and rail release
	var idle_state = states.get_node("Idle")
	var ground_state = states.get_node("GroundMovement")
	states.call("TransitionTo", "Idle", null)
	player.call("TryBounce", 13.0)
	idle_state.call("PhysicsUpdate", 1.0 / 60.0)
	_check(str(states.get("CurrentStateName")) == "Bounce" and is_equal_approx(float((player.get("PlayerVelocity") as Vector3).y), 13.0), "Idle turns a queued bounce into the Bounce state with the launch velocity")
	states.call("TransitionTo", "GroundMovement", null)
	player.call("TryBounce", 13.0)
	ground_state.call("PhysicsUpdate", 1.0 / 60.0)
	_check(str(states.get("CurrentStateName")) == "Bounce", "GroundMovement turns a queued bounce into the Bounce state")
	player.call("ClearBounceRequest")
	states.call("TransitionTo", "Idle", null)
	player.set("MovementSpeed", 0.0)
	player.call("TryStartSpeedBoost", 13.0, 1.5, Vector3(0, 0, -1))
	idle_state.call("PhysicsUpdate", 1.0 / 60.0)
	_check(str(states.get("CurrentStateName")) == "GroundMovement", "Idle hands an active speed boost to GroundMovement")
	ground_state.call("PhysicsUpdate", 1.0 / 60.0)
	var boost_velocity: Vector3 = player.get("PlayerVelocity")
	_check(Vector2(boost_velocity.x, boost_velocity.z).length() >= 13.0 and boost_velocity.z < 0.0 and absf(boost_velocity.x) < 0.01, "Speed boost keeps its heading along -Z without input")
	player.call("CancelSpeedBoost")
	states.call("TransitionTo", "Idle", null)
	player.set("MovementSpeed", 0.0)

	player.call("TryStartRail", rail)
	var hit_area := Area3D.new()
	hit_area.name = "SlimeHitbox"
	current_scene.add_child(hit_area)
	player.call("OnHurtboxEntered", hit_area)
	_check(bool(player.get("IsOnRail")) and not bool(player.get("IsHurt")), "Hurtbox contact is ignored while riding the rail")
	hit_area.free()
	player.call("Respawn")
	_check(not bool(player.get("IsOnRail")), "Respawn ends an active rail ride")
	states.call("TransitionTo", "Idle", null)
	player.set("MovementSpeed", 0.0)

	player.call("TryStartRail", rail)
	player.call("AbortRail")
	states.get_node("Looping").call("PhysicsUpdate", 1.0 / 60.0)
	_check(str(states.get("CurrentStateName")) == "AirDropMovement", "Looping leaves for AirDropMovement when the rail is gone")
	states.call("TransitionTo", "Idle", null)
	player.set("MovementSpeed", 0.0)

	var rail_flipped: Node3D = load("res://Environment/Nodes/looping.tscn").instantiate()
	rail_flipped.position = Vector3(200, 0, 200)
	rail_flipped.rotation.y = PI
	current_scene.add_child(rail_flipped)
	player.call("TryStartRail", rail_flipped)
	var flipped_top: Transform3D = rail_flipped.call("SampleTransform", 0.0)
	var flipped_distance := 0.0
	while flipped_distance <= float(rail_flipped.get("Length")):
		var flipped_sample: Transform3D = rail_flipped.call("SampleTransform", flipped_distance)
		if flipped_sample.origin.y > flipped_top.origin.y:
			flipped_top = flipped_sample
		flipped_distance += 0.05
	player.global_transform = flipped_top
	player.call("AbortRail")
	var facing: Vector3 = player.global_transform.basis.z
	facing.y = 0.0
	var travel: Vector3 = -rail_flipped.global_transform.basis.z
	_check(facing.normalized().dot(travel) > 0.99, "Aborting at the top of the ring faces along the rail travel direction")
	states.call("TransitionTo", "Idle", null)
	rail_flipped.free()

	var floor_rail: Node3D = load("res://Environment/Nodes/looping.tscn").instantiate()
	# Origin height 0.45 puts the rail exit at the resting player height on the floor (y = 2.45 with the rail at y = 2)
	floor_rail.position = Vector3(0, 2, 18)
	current_scene.add_child(floor_rail)
	player.set("HasDoubleJump", false)
	player.global_transform = start_transform
	player.set("CanInput", false)
	player.call("ControlFeetCollision", false, true)
	player.call("ControlSaltoCollision", true)
	player.call("TryStartRail", floor_rail)
	_run_rail(player)
	var landed_state := str(states.get("CurrentStateName"))
	var landed_boost: bool = player.get("IsSpeedBoostActive")
	var landed_input: bool = player.get("CanInput")
	var landed_jump: bool = player.get("HasDoubleJump")
	var foot_hurtbox: Area3D = player.find_child("HurtBoxFootL", true, false)
	_check(landed_state == "GroundMovement" and landed_boost and landed_input and landed_jump, "Releasing the rail onto the floor grounds the player with a boost, input and the double jump")
	_check(foot_hurtbox.monitorable, "Releasing the rail turns the feet hurtbox back on")
	floor_rail.free()
	player.call("CancelSpeedBoost")
	states.call("TransitionTo", "Idle", null)
	player.set("MovementSpeed", 0.0)

	# Finishing clears rail and boost, and blocks new effects
	player.call("TryStartRail", rail)
	player.call("TryStartSpeedBoost", 13.0, 1.5, Vector3(0, 0, -1))
	player.call("BeginFinishSequence")
	_check(not bool(player.get("IsOnRail")) and not bool(player.get("IsSpeedBoostActive")), "Finish sequence clears the active rail and boost")
	_check(not bool(player.call("TryStartSpeedBoost", 13.0, 1.5, Vector3(0, 0, -1))), "Speed boost is refused after the finish sequence began")
	rail.free()
	player.global_transform = start_transform

func _check_falling_platform(player: Node):
	var start_transform: Transform3D = player.global_transform
	var platform = load("res://Environment/Nodes/earth_platform_1x_1.tscn").instantiate()
	platform.set("stayTime", 0.05)
	platform.position = Vector3(300, 0, 300)
	current_scene.add_child(platform)
	var spawn: Vector3 = platform.position
	var original_layer: int = platform.collision_layer
	platform.call("OnPlatformEntered", player)
	var started := Time.get_ticks_msec()
	while platform.visible and Time.get_ticks_msec() - started < 3000:
		await process_frame
	_check(not platform.visible and (platform.collision_layer & 2) == 0, "Falling platform hides after dropping")
	player.call("Respawn")
	# The platform syncs its transform to physics, so the new position lands a step later.
	await physics_frame
	await physics_frame
	_check(platform.visible and platform.position.is_equal_approx(spawn) and platform.collision_layer == original_layer, "Respawn restores fallen platforms")
	platform.free()
	player.global_transform = start_transform

func _check_scene_teardown(player: Node):
	var hud := current_scene.get_node("HUD")
	hud.call("UpdateTutorial", "Scene teardown test", 0.05)
	player.set("IsInvulnerable", true)
	player.call("blinking", Color.WHITE, Color.RED, 0.02)
	var old_level := current_scene
	change_scene_to_file("res://Menu/Nodes/menu_title.tscn")
	await create_timer(0.2).timeout
	_check(not is_instance_valid(old_level), "Scene teardown frees the level during tutorial and blink callbacks")

func _check_menu_and_sound(global_node: Node):
	var sound := root.get_node("SoundManager")
	sound.call("PlayMusicPuppyPlayingInTheGarden2D")
	sound.call("EnsureMenuMusic")
	_check(not sound.call("IsMusicPuppyPlayingInTheGarden2DPlaying") and sound.call("IsMusicProject2Marioish2DPlaying"), "Shared menu music setup switches from gameplay music")
	var menu_music := sound.get_node("MusicProject2Marioish2D")
	menu_music.seek(1.0)
	await process_frame
	sound.call("EnsureMenuMusic")
	_check(menu_music.get_playback_position() > 0.5, "Shared menu music setup keeps existing playback")
	global_node.get("SaveData").call("ResetHighScores")
	global_node.set("Minutes", 1)
	global_node.set("Seconds", 0)
	change_scene_to_file("res://Menu/Nodes/menu_win.tscn")
	await process_frame
	await process_frame
	current_scene.get_node("PanelScore/TextEditName").text = "Simplification regression"
	current_scene.call("OnButtonAcceptPressed")
	var scores = global_node.get("SaveData").get("PlayerScores")
	var treats = scores[0].get("SpecialTreats")
	_check(treats.size() == 3 and "Cheese" in treats and "Cookie" in treats and "Meat" in treats, "Special-treat rewards save exactly once for each type")
	for destination in [
		["OnButtonScoresPressed", "res://Menu/Nodes/menu_highscores.tscn"],
		["OnButtonSettingsPressed", "res://Menu/Nodes/menu_settings_sound.tscn"],
		["OnButtonCreditsPressed", "res://Menu/Nodes/menu_credits.tscn"]
	]:
		change_scene_to_file("res://Menu/Nodes/menu_title.tscn")
		await process_frame
		await process_frame
		current_scene.call(destination[0])
		await process_frame
		await process_frame
		_check(current_scene.scene_file_path == destination[1], "Title-menu handler opens " + destination[1].get_file())
	change_scene_to_file("res://Menu/Nodes/menu_title.tscn")
	await process_frame
	await process_frame
	var title := current_scene
	title.call("OnButtonPlayPressed")
	_check(title.get_node("LevelSelect").visible and not title.get_node("Buttons").visible, "Title-menu Play shows the level select and hides the main buttons")
	title.call("OnButtonBackPressed")
	_check(not title.get_node("LevelSelect").visible and title.get_node("Buttons").visible, "Title-menu Back restores the main buttons")
	var stored_path = global_node.get("CurrentLevelScenePath")
	global_node.set("CurrentLevelScenePath", "res://Menu/Nodes/menu_title.tscn")
	_check(global_node.call("GetReplayScenePath") == "res://Menu/Nodes/menu_title.tscn", "Replay path returns the stored scene when it exists")
	global_node.set("CurrentLevelScenePath", "")
	_check(global_node.call("GetReplayScenePath") == "res://Levels/Nodes/level_1.tscn", "Replay path falls back to level_1 when empty")
	global_node.set("CurrentLevelScenePath", "res://Levels/Nodes/missing.tscn")
	_check(global_node.call("GetReplayScenePath") == "res://Levels/Nodes/level_1.tscn", "Replay path falls back to level_1 when the scene is missing")
	global_node.set("CurrentLevelScenePath", stored_path)
	change_scene_to_file("res://Menu/Nodes/menu_defeat.tscn")
	await process_frame
	await process_frame
	var defeat := current_scene
	defeat.call("OnButtonPlayAgainPressed")
	_check(current_scene == defeat, "Play Again waits for its sound before changing scenes")
	sound.get_node("SoundPlay").emit_signal("finished")
	await process_frame
	await process_frame
	_check(current_scene.scene_file_path == "res://Levels/Nodes/level_1.tscn", "Shared sound helper resumes Play Again when playback finishes")
	await _check_level_2_from_title(global_node)

func _check_level_2_from_title(global_node: Node):
	change_scene_to_file("res://Menu/Nodes/menu_title.tscn")
	await process_frame
	await process_frame
	var title_menu := current_scene
	title_menu.call("OnButtonLevel2Pressed")
	var started := Time.get_ticks_msec()
	while current_scene == title_menu and Time.get_ticks_msec() - started < 5000:
		await process_frame
	await process_frame
	await process_frame
	await process_frame
	var level_2_path := "res://Levels/Nodes/level_2.tscn"
	_check(current_scene != null and current_scene.scene_file_path == level_2_path, "Title-menu Level 2 button changes to level_2")
	if current_scene == null or current_scene.scene_file_path != level_2_path:
		return
	_check(current_scene.get("LevelId") == "1-2" and global_node.get("CurrentLevelScenePath") == level_2_path, "Level 2 reports its level id and stores its scene path in Global")
	_check(is_equal_approx(float(global_node.get("ExpectedTime")), 75.0) and int(global_node.get("ExpectedTreats")) == 60, "Level 2 passes its score targets to Global")
	var treat_count: int = current_scene.get_node("Treats").get_child_count()
	var specials := current_scene.get_node("SpecialTreats")
	var special_types := {}
	for special in specials.get_children():
		special_types[str(special.name)] = special.get("TreatType")
	_check(treat_count >= 60 and specials.get_child_count() == 3 and special_types.get("Cheese") == 1 and special_types.get("Cookie") == 3 and special_types.get("Meat") == 2 and current_scene.find_child("BoneTreat", true, false) != null, "Level 2 has its treats, three special treats and the bone treat")
	var rail_count := 0
	for loop in current_scene.get_node("Loopings").get_children():
		var script = loop.get_script()
		if script != null and str(script.resource_path).ends_with("LoopRail.cs"):
			rail_count += 1
	_check(rail_count == 2, "Level 2 has exactly two loop rails")
	_check(not current_scene.get_node("HUD/PanelTutorial").visible, "The tutorial box stays hidden until a tutorial trigger fires")
	var tutorial_triggers := current_scene.get_node("Tutorials").get_children()
	var speed_z: float = current_scene.get_node("Pads/SpeedPad1").position.z
	var looping_z: float = current_scene.get_node("Loopings/Looping1").position.z
	var bounce_z: float = current_scene.get_node("Pads/BouncePad1").position.z
	var spawn_z: float = current_scene.get_node("PlayerCharacter").position.z
	var tutorials_ok := tutorial_triggers.size() == 3
	var found_speed := false
	var found_looping := false
	var found_bounce := false
	for trigger in tutorial_triggers:
		var keyboard_text := str(trigger.get("TextKeyboard"))
		if keyboard_text.is_empty() or str(trigger.get("TextGamepad")).is_empty() or trigger.get("Hud") == null:
			tutorials_ok = false
		var trigger_z: float = trigger.position.z
		if trigger_z > spawn_z:
			tutorials_ok = false
		if keyboard_text.contains("arrow pads") and trigger_z > speed_z:
			found_speed = true
		elif keyboard_text.contains("looping") and trigger_z > looping_z:
			found_looping = true
		# The falling bridge ends at z -150, so the bounce tip has to come after it, on the moving platform.
		elif keyboard_text.contains("bounce") and trigger_z > bounce_z and trigger_z < -150.0:
			found_bounce = true
	_check(tutorials_ok and found_speed and found_looping and found_bounce, "Level 2 tutorials explain the speed pads, the looping and the bounce pad")
	var hud := current_scene.get_node("HUD")
	var panel_tutorial: Panel = hud.get_node("PanelTutorial")
	var label_tutorial: Label = panel_tutorial.get_node("LabelTutorial")
	hud.call("UpdateTutorial", "Tutorial visibility test", 0.05)
	var shown := panel_tutorial.visible and label_tutorial.text == "Tutorial visibility test"
	await create_timer(0.2).timeout
	_check(shown and not panel_tutorial.visible and label_tutorial.text.is_empty(), "The tutorial box shows a message and hides again when it runs out")
