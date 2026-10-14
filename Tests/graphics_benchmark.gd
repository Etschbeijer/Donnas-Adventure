extends SceneTree

func _initialize():
	call_deferred("_run")

func _run():
	Engine.max_fps = 0
	DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
	var arguments := OS.get_cmdline_user_args()
	var label: String = arguments[0] if arguments.size() > 0 else "current"
	label = label.validate_filename()
	var level: Node3D = load("res://Levels/Nodes/level_1.tscn").instantiate()
	root.add_child(level)
	current_scene = level
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	level.process_mode = Node.PROCESS_MODE_DISABLED
	await process_frame
	await process_frame
	level.get_node("PlayerCharacter").call("ReplaceWithStandardMaterial")
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1920, 1080)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	viewport.world_3d = root.world_3d
	viewport.msaa_3d = root.msaa_3d
	viewport.use_taa = root.use_taa
	root.add_child(viewport)
	root.disable_3d = true
	await create_timer(2.0).timeout
	var camera := Camera3D.new()
	viewport.add_child(camera)
	camera.position = Vector3(0, 5, 10)
	camera.look_at(Vector3(0, 0, -20))
	camera.make_current()
	RenderingServer.viewport_set_measure_render_time(viewport.get_viewport_rid(), true)
	for frame in range(120):
		await RenderingServer.frame_post_draw
	var samples: Array[float] = []
	var cpu_samples: Array[float] = []
	for frame in range(240):
		await RenderingServer.frame_post_draw
		var gpu_ms := RenderingServer.viewport_get_measured_render_time_gpu(viewport.get_viewport_rid())
		if gpu_ms > 0:
			samples.append(gpu_ms)
			cpu_samples.append(RenderingServer.viewport_get_measured_render_time_cpu(viewport.get_viewport_rid()))
	samples.sort()
	cpu_samples.sort()
	var water: MeshInstance3D = level.get_node("Water")
	var result := {
		"label": label,
		"render_size": [viewport.size.x, viewport.size.y],
		"samples": samples.size(),
		"gpu_median_ms": samples[samples.size() / 2] if samples.size() else 0.0,
		"gpu_p95_ms": samples[int(samples.size() * 0.95)] if samples.size() else 0.0,
		"cpu_render_median_ms": cpu_samples[cpu_samples.size() / 2] if cpu_samples.size() else 0.0,
		"water_triangles": water.mesh.get_faces().size() / 3,
		"msaa": viewport.msaa_3d,
		"taa": viewport.use_taa,
		"draw_calls": RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_TOTAL_DRAW_CALLS_IN_FRAME),
		"primitives": RenderingServer.get_rendering_info(RenderingServer.RENDERING_INFO_TOTAL_PRIMITIVES_IN_FRAME)
	}
	var report := FileAccess.open("res://.godot/graphics-" + label + ".json", FileAccess.WRITE)
	report.store_string(JSON.stringify(result, "\t"))
	report.close()
	print(JSON.stringify(result))
	# Capture a deterministic shader-time frame separately from the timing run.
	Engine.time_scale = 0.0
	for frame in range(10):
		await RenderingServer.frame_post_draw
	viewport.get_texture().get_image().save_png("res://.godot/graphics-" + label + ".png")
	level.queue_free()
	current_scene = null
	await process_frame
	RenderingServer.frame_pre_draw.emit()
	viewport.world_3d = null
	viewport.queue_free()
	await process_frame
	await RenderingServer.frame_post_draw
	Engine.time_scale = 1.0
	for player in root.get_node("SoundManager").get_children():
		if player is AudioStreamPlayer2D:
			player.stop()
	await create_timer(0.1).timeout
	quit(0 if samples.size() else 1)
