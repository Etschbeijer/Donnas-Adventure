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
	await create_timer(0.1).timeout
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
