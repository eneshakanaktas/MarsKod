extends Node3D
## MarsKod görsel denemesi (Godot). Sahnenin tamamını koddan kurar:
## yüzen Mars adası, ışıklar, gezegen, parçacıklar, robotun kodu "çalıştırma" animasyonu ve arayüz.
##
## Ekran görüntüsü almak için: godot --path . -- --shots=<dosya-öneki> --at=2,4,6

const CELL := 1.0
const COLS := 6
const ROWS := 6

# Kamera: adayı ekranın üst yarısına yerleştirir (alt yarıda kod paneli var)
const CAM_HEIGHT := 7.9
const CAM_DIST := 10.2
const CAM_TARGET := Vector3(0, -0.5, 3.9)

# Harita: . boş · I buz · R kaya · @ robot · D depo · L laboratuvar · G sera
const MAP := [
	"..R.L.",
	".R...I",
	"@III..",
	"I.....",
	"..R.D.",
	"G..R.I",
]

# Oyun alanının çevresindeki süsler (ızgaranın dışı; oyuna etkisi yok)
const DECOR := [
	["rocket", Vector2i(-1, -1)],
	["satelliteDish_large", Vector2i(6, 0)],
	["astronautA", Vector2i(-1, 3)],
	["barrels", Vector2i(6, 5)],
	["machine_generator", Vector2i(6, 3)],
	["rocks_smallA", Vector2i(2, -1)],
	["craterLarge", Vector2i(4, 6)],
	["rocks_smallB", Vector2i(-1, 5)],
	["astronautB", Vector2i(1, 6)],
]

const CODE_LINES := [
	"[color=#FF9E64]for[/color] i [color=#FF9E64]in[/color] [color=#7DCFFF]range[/color]([color=#E0AF68]3[/color]):",
	"    [color=#7DCFFF]move[/color]([color=#9ECE6A]\"east\"[/color])",
	"    [color=#FF9E64]if[/color] [color=#7DCFFF]ice_here[/color]():",
	"        [color=#7DCFFF]collect[/color]()",
]

var world: Node3D
var camera: Camera3D
var rover: Node3D
var rover_body: Node3D
var rover_cell := Vector2i.ZERO
var rover_start := Vector2i.ZERO
var dust_trail: GPUParticles3D
var ices := {}  # Vector2i -> Node3D
var crystal_materials: Array[StandardMaterial3D] = []
var blink_lights: Array[OmniLight3D] = []
var dish: Node3D
var ice_count := 0
var ice_label: Label
var code_label: RichTextLabel
var time := 0.0

var font_heading: FontFile
var font_label: FontFile
var font_body: FontFile
var font_code: FontFile


func _ready() -> void:
	font_heading = load("res://fonts/ChakraPetch_700Bold.ttf")
	font_label = load("res://fonts/ChakraPetch_600SemiBold.ttf")
	font_body = load("res://fonts/ChakraPetch_500Medium.ttf")
	font_code = load("res://fonts/JetBrainsMono_400Regular.ttf")

	world = Node3D.new()
	add_child(world)
	_build_environment()
	_build_lights()
	_build_camera()
	_build_planet()
	_build_island()
	_build_map()
	_build_decor()
	_build_ambient_dust()
	_build_hud()
	_setup_shots()
	_run_demo()


func _process(delta: float) -> void:
	time += delta
	# buz kristalleri nefes alır gibi parlar
	for i in crystal_materials.size():
		crystal_materials[i].emission_energy_multiplier = 1.1 + 0.45 * sin(time * 2.2 + i * 1.3)
	# anten ışıkları yanıp söner
	for i in blink_lights.size():
		blink_lights[i].light_energy = 2.5 if fmod(time + i * 0.4, 1.4) < 0.25 else 0.0
	if dish:
		dish.rotation.y += delta * 0.4
	# robot beklerken hafifçe yaylanır
	if rover_body:
		rover_body.position.y = abs(sin(time * 5.0)) * 0.025
	# kamera çok yavaş salınır: sahne canlı dursun
	if camera:
		var a := sin(time * 0.25) * 0.05
		camera.position = Vector3(sin(a) * CAM_DIST, CAM_HEIGHT, cos(a) * CAM_DIST)
		camera.look_at(CAM_TARGET)


# --- ortam ---

func _build_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	var sky_mat := ShaderMaterial.new()
	sky_mat.shader = load("res://space_sky.gdshader")
	sky.sky_material = sky_mat
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("#7A5A70")
	env.ambient_light_energy = 0.55
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.tonemap_exposure = 1.05
	env.glow_enabled = true
	env.glow_intensity = 0.55
	env.glow_bloom = 0.0
	env.glow_hdr_threshold = 1.0
	env.adjustment_enabled = true
	env.adjustment_saturation = 1.15
	env.adjustment_contrast = 1.06
	var we := WorldEnvironment.new()
	we.environment = env
	add_child(we)


func _build_lights() -> void:
	# ana güneş: sol ön üstten, sıcak, yumuşak gölgeli
	var sun := DirectionalLight3D.new()
	sun.light_color = Color("#FFE9D6")
	sun.light_energy = 1.35
	sun.shadow_enabled = true
	sun.shadow_blur = 1.5
	sun.directional_shadow_max_distance = 30.0
	add_child(sun)
	sun.look_at_from_position(Vector3(-5, 9, 6), Vector3.ZERO)
	# arka kenar ışığı: soğuk mor, nesnelerin hatlarını ayırır
	var rim := DirectionalLight3D.new()
	rim.light_color = Color("#8A7CFF")
	rim.light_energy = 0.55
	add_child(rim)
	rim.look_at_from_position(Vector3(6, 4, -8), Vector3.ZERO)
	# adanın altından uzaya yansıyan turuncu ışık
	var under := OmniLight3D.new()
	under.light_color = Color("#FF7A3D")
	under.light_energy = 4.0
	under.omni_range = 9.0
	under.position = Vector3(0, -3.0, 2.5)
	add_child(under)


func _build_camera() -> void:
	camera = Camera3D.new()
	# telefon dikey: görüş açısı genişliğe göre sabitlensin ki ada her ekranda sığsın
	camera.keep_aspect = Camera3D.KEEP_WIDTH
	camera.fov = 44.0
	camera.position = Vector3(0, CAM_HEIGHT, CAM_DIST)
	add_child(camera)
	camera.look_at(CAM_TARGET)


func _build_planet() -> void:
	var planet := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 7.0
	sphere.height = 14.0
	planet.mesh = sphere
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color("#6F58A8")
	mat.roughness = 1.0
	mat.rim_enabled = true
	mat.rim = 1.0
	mat.rim_tint = 0.2
	mat.emission_enabled = true
	mat.emission = Color("#2A1A55")
	mat.emission_energy_multiplier = 0.6
	planet.material_override = mat
	planet.position = Vector3(-17, 6, -34)
	add_child(planet)
	# halka
	var ring := MeshInstance3D.new()
	var torus := TorusMesh.new()
	torus.inner_radius = 9.0
	torus.outer_radius = 12.5
	torus.rings = 64
	ring.mesh = torus
	var ring_mat := StandardMaterial3D.new()
	ring_mat.albedo_color = Color(0.85, 0.7, 1.0, 0.35)
	ring_mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	ring_mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	ring.material_override = ring_mat
	ring.scale = Vector3(1, 0.04, 1)
	ring.rotation_degrees = Vector3(18, 0, -22)
	planet.add_child(ring)


# --- ada ---

func _build_island() -> void:
	# oyun alanı + çevresinde bir süs halkası kadar zemin
	for gy in range(-1, ROWS + 1):
		for gx in range(-1, COLS + 1):
			var tile := _spawn("terrain", _cell_pos(Vector2i(gx, gy)), 1.0)
			_set_cast_shadow(tile, false)
	# pürüzlü kenarlı, aşağı doğru daralan kaya katmanları
	var size := float(COLS + 2)
	var tiers := [
		[size + 0.12, 0.55, 0.10, Color("#C4623F"), -0.02],
		[size - 0.5, 0.6, 0.22, Color("#9C4830"), -0.57],
		[size - 1.6, 0.7, 0.35, Color("#733324"), -1.17],
		[size - 3.2, 0.7, 0.4, Color("#4E2219"), -1.87],
		[size - 5.0, 0.6, 0.35, Color("#351610"), -2.57],
	]
	var tier_seed := 1
	for t in tiers:
		_rock_tier(t[0], t[1], t[2], t[3], t[4], tier_seed)
		tier_seed += 1
	_build_grid_lines()


func _rock_tier(width: float, height: float, jag: float, color: Color, top: float, seed: int) -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = seed * 7919
	var pts := PackedVector2Array()
	var half := width / 2.0
	var corners := [Vector2(-half, -half), Vector2(half, -half), Vector2(half, half), Vector2(-half, half)]
	for c in 4:
		var a: Vector2 = corners[c]
		var b: Vector2 = corners[(c + 1) % 4]
		var normal := Vector2(b.y - a.y, -(b.x - a.x)).normalized()
		# pürüz, noktalar arası mesafeyi aşmasın: kenarlar kesişirse şekil çizilemez
		var safe_jag := minf(jag, width / 12.0 * 0.35)
		for i in 12:
			var off := 0.0 if i == 0 else rng.randf_range(-safe_jag, safe_jag)
			pts.append(a.lerp(b, i / 12.0) + normal * off)
	var poly := CSGPolygon3D.new()
	poly.polygon = pts
	poly.depth = height
	poly.smooth_faces = false
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = 1.0
	poly.material = mat
	poly.rotation_degrees.x = -90.0
	poly.position.y = top
	world.add_child(poly)


func _build_grid_lines() -> void:
	var im := ImmediateMesh.new()
	var mat := StandardMaterial3D.new()
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.albedo_color = Color(0.32, 0.1, 0.04, 0.45)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	im.surface_begin(Mesh.PRIMITIVE_LINES, mat)
	var x0 := -COLS / 2.0
	var z0 := -ROWS / 2.0
	for i in COLS + 1:
		im.surface_add_vertex(Vector3(x0 + i, 0.006, z0))
		im.surface_add_vertex(Vector3(x0 + i, 0.006, z0 + ROWS))
	for j in ROWS + 1:
		im.surface_add_vertex(Vector3(x0, 0.006, z0 + j))
		im.surface_add_vertex(Vector3(x0 + COLS, 0.006, z0 + j))
	im.surface_end()
	var mi := MeshInstance3D.new()
	mi.mesh = im
	mi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	world.add_child(mi)


func _build_map() -> void:
	for gy in ROWS:
		var row: String = MAP[gy]
		for gx in COLS:
			var cell := Vector2i(gx, gy)
			var pos := _cell_pos(cell)
			var turn := _rand(gx, gy, 5) * TAU
			match row[gx]:
				"I":
					var model := "rock_crystalsLargeA" if _rand(gx, gy, 6) < 0.5 else "rock_crystalsLargeB"
					var ice := _spawn(model, pos, 0.88, turn)
					_make_icy(ice)
					ices[cell] = ice
				"R":
					var rmodel := "rock_largeA" if _rand(gx, gy, 7) < 0.5 else "rock_largeB"
					_spawn(rmodel, pos, 0.72, turn)
				"@":
					rover_start = cell
					_build_rover(cell)
				"D":
					_spawn("hangar_smallA", pos, 0.96, -PI / 2.0)
				"L":
					_spawn("hangar_roundB", pos, 0.88)
					_add_blink_light(pos + Vector3(0.25, 0.75, 0.0), Color("#FF3B3B"))
				"G":
					var gh := _spawn("hangar_roundGlass", pos, 0.9)
					_make_greenhouse(gh)
				_:
					if _rand(gx, gy, 8) < 0.2:
						_spawn("crater", pos + Vector3(_rand(gx, gy, 9) - 0.5, 0, _rand(gx, gy, 10) - 0.5) * 0.3, 0.5, turn)


func _build_decor() -> void:
	for d in DECOR:
		var decor_name: String = d[0]
		var cell: Vector2i = d[1]
		var pos := _cell_pos(cell)
		match decor_name:
			"rocket":
				var base := _spawn("rocket_baseA", pos, 0.7)
				var fuel := _spawn("rocket_fuelA", pos + Vector3(0, base.get_meta("height"), 0), 0.55)
				_spawn("rocket_topA", pos + Vector3(0, base.get_meta("height") + fuel.get_meta("height"), 0), 0.55)
				_add_blink_light(pos + Vector3(0, base.get_meta("height") + fuel.get_meta("height") + 0.5, 0), Color("#39D5FF"))
			"satelliteDish_large":
				dish = _spawn(decor_name, pos, 0.85)
			_:
				_spawn(decor_name, pos, 0.55 if decor_name.begins_with("astronaut") else 0.7, _rand(cell.x, cell.y, 3) * TAU)


func _build_ambient_dust() -> void:
	var p := GPUParticles3D.new()
	p.amount = 90
	p.lifetime = 9.0
	p.preprocess = 9.0
	var pm := ParticleProcessMaterial.new()
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	pm.emission_box_extents = Vector3(5.0, 1.4, 5.0)
	pm.direction = Vector3(1, 0.2, 0)
	pm.spread = 180.0
	pm.initial_velocity_min = 0.03
	pm.initial_velocity_max = 0.12
	pm.gravity = Vector3.ZERO
	pm.color_ramp = _fade_ramp(Color(1.0, 0.85, 0.7, 0.0), Color(1.0, 0.85, 0.7, 0.7))
	p.process_material = pm
	p.draw_pass_1 = _particle_mesh(0.018, Color("#FFD9B8"), 1.6)
	p.position = Vector3(0, 1.2, 0)
	world.add_child(p)


# --- robot ---

func _build_rover(cell: Vector2i) -> void:
	rover = Node3D.new()
	world.add_child(rover)
	rover_body = Node3D.new()
	rover.add_child(rover_body)
	var model := _spawn("rover", Vector3.ZERO, 0.78, 0.0, rover_body)
	model.rotation.y = -PI / 2.0  # modelin önü doğuya baksın
	# farların önüne düşen ışık
	var head := SpotLight3D.new()
	head.light_color = Color("#FFF2C8")
	head.light_energy = 3.0
	head.spot_range = 2.2
	head.spot_angle = 35.0
	head.position = Vector3(0.25, 0.3, 0)
	head.rotation_degrees = Vector3(-25, -90, 0)
	rover_body.add_child(head)
	_add_blink_light(Vector3(-0.1, 0.55, 0), Color("#39D5FF"), rover_body)
	# tekerleklerden kalkan toz
	dust_trail = GPUParticles3D.new()
	dust_trail.amount = 40
	dust_trail.lifetime = 1.2
	dust_trail.emitting = false
	dust_trail.local_coords = false
	var pm := ParticleProcessMaterial.new()
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	pm.emission_box_extents = Vector3(0.12, 0.02, 0.2)
	pm.direction = Vector3(0, 1, 0)
	pm.spread = 45.0
	pm.initial_velocity_min = 0.3
	pm.initial_velocity_max = 0.7
	pm.gravity = Vector3(0, -0.3, 0)
	pm.scale_min = 1.0
	pm.scale_max = 2.4
	pm.color_ramp = _fade_ramp(Color(0.85, 0.55, 0.38, 0.75), Color(0.85, 0.55, 0.38, 0.0))
	dust_trail.process_material = pm
	dust_trail.draw_pass_1 = _particle_mesh(0.05, Color("#D9956E"), 0.0)
	dust_trail.position = Vector3(-0.3, 0.05, 0)
	rover.add_child(dust_trail)
	_place_rover(cell)


func _place_rover(cell: Vector2i) -> void:
	rover_cell = cell
	rover.position = _cell_pos(cell)
	rover.rotation.y = 0.0


func _move_rover(target: Vector2i) -> void:
	var dir := Vector2(target - rover_cell)
	var yaw := atan2(-dir.y, dir.x)
	var tw := create_tween().set_parallel(true)
	tw.tween_property(rover, "rotation:y", yaw, 0.25).set_trans(Tween.TRANS_SINE)
	tw.tween_property(rover, "position", _cell_pos(target), 0.75).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	dust_trail.emitting = true
	await tw.finished
	dust_trail.emitting = false
	rover_cell = target


func _collect(cell: Vector2i) -> void:
	var ice: Node3D = ices[cell]
	var pos := ice.position
	var tw := create_tween()
	tw.tween_property(ice, "scale", ice.scale * 1.15, 0.12).set_trans(Tween.TRANS_SINE)
	tw.tween_property(ice, "scale", Vector3.ONE * 0.001, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_IN)
	_burst(pos + Vector3(0, 0.35, 0))
	_float_text("+1 buz", pos + Vector3(0, 0.9, 0))
	await tw.finished
	ice.visible = false
	ice_count += 1
	ice_label.text = "%d/3" % ice_count
	var pop := create_tween()
	pop.tween_property(ice_label, "scale", Vector2(1.35, 1.35), 0.1)
	pop.tween_property(ice_label, "scale", Vector2.ONE, 0.2)


func _burst(pos: Vector3) -> void:
	var p := GPUParticles3D.new()
	p.one_shot = true
	p.amount = 60
	p.lifetime = 1.0
	p.explosiveness = 0.95
	var pm := ParticleProcessMaterial.new()
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_SPHERE
	pm.emission_sphere_radius = 0.2
	pm.direction = Vector3.UP
	pm.spread = 75.0
	pm.initial_velocity_min = 1.6
	pm.initial_velocity_max = 3.4
	pm.gravity = Vector3(0, -6, 0)
	pm.scale_min = 0.6
	pm.scale_max = 1.4
	pm.color_ramp = _fade_ramp(Color(1, 1, 1, 1), Color(1, 1, 1, 0))
	p.process_material = pm
	p.draw_pass_1 = _particle_mesh(0.035, Color("#BFF4FF"), 4.0)
	p.position = pos
	world.add_child(p)
	p.emitting = true
	get_tree().create_timer(1.5).timeout.connect(p.queue_free)


func _float_text(text: String, pos: Vector3) -> void:
	var label := Label3D.new()
	label.text = text
	label.font = font_heading
	label.font_size = 64
	label.pixel_size = 0.0075
	label.modulate = Color("#BFF4FF")
	label.outline_modulate = Color(0.05, 0.1, 0.2)
	label.outline_size = 12
	label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	label.no_depth_test = true
	label.position = pos
	world.add_child(label)
	var tw := create_tween().set_parallel(true)
	tw.tween_property(label, "position", pos + Vector3(0, 0.7, 0), 1.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tw.tween_property(label, "modulate:a", 0.0, 1.1).set_delay(0.3)
	tw.chain().tween_callback(label.queue_free)


# Robot, kod panelindeki programı satır satır "çalıştırır"; sonra sahne başa döner.
func _run_demo() -> void:
	while true:
		_reset_demo()
		await _wait(1.4)
		for i in 3:
			_highlight(0)
			await _wait(0.45)
			_highlight(1)
			await _move_rover(rover_cell + Vector2i(1, 0))
			_highlight(2)
			await _wait(0.45)
			if ices.has(rover_cell) and ices[rover_cell].visible:
				_highlight(3)
				await _collect(rover_cell)
				await _wait(0.2)
		_highlight(-1)
		await _wait(2.6)


func _reset_demo() -> void:
	_place_rover(rover_start)
	for cell in ices:
		var ice: Node3D = ices[cell]
		ice.visible = true
		ice.scale = ice.get_meta("scale")
	ice_count = 0
	if ice_label:
		ice_label.text = "0/3"
	_highlight(-1)


func _wait(seconds: float) -> void:
	await get_tree().create_timer(seconds).timeout


# --- arayüz ---

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	var root := Control.new()
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layer.add_child(root)

	var world_label := _label("DÜNYA 1 · İNİŞ BÖLGESİ", font_label, 34, Color("#39D5FF"))
	world_label.position = Vector2(56, 120)
	root.add_child(world_label)
	var level_label := _label("Bölüm 3: Buz Avı", font_heading, 64, Color("#EDE7F6"))
	level_label.position = Vector2(56, 160)
	root.add_child(level_label)

	var chip := _panel(Color(0.08, 0.1, 0.18, 0.85), Color("#26304A"), 36)
	chip.position = Vector2(760, 150)
	chip.custom_minimum_size = Vector2(264, 96)
	root.add_child(chip)
	var chip_row := HBoxContainer.new()
	chip_row.alignment = BoxContainer.ALIGNMENT_CENTER
	chip_row.add_theme_constant_override("separation", 16)
	chip.add_child(chip_row)
	chip_row.add_child(_label("◆", font_heading, 44, Color("#8FE6FF")))
	ice_label = _label("0/3", font_heading, 52, Color("#EDE7F6"))
	ice_label.pivot_offset = Vector2(40, 30)
	chip_row.add_child(ice_label)

	var mission := _panel(Color(0.22, 0.83, 1.0, 0.08), Color(0.22, 0.83, 1.0, 0.35), 28)
	mission.position = Vector2(48, 1400)
	mission.custom_minimum_size = Vector2(984, 110)
	root.add_child(mission)
	var mission_text := _label("Görev: Doğudaki buzları topla.", font_body, 42, Color("#EDE7F6"))
	mission_text.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	mission.add_child(mission_text)

	var code := _panel(Color("#141A2E"), Color("#26304A"), 36)
	code.position = Vector2(48, 1540)
	code.custom_minimum_size = Vector2(984, 540)
	root.add_child(code)
	code_label = RichTextLabel.new()
	code_label.bbcode_enabled = true
	code_label.fit_content = true
	code_label.scroll_active = false
	code_label.add_theme_font_override("normal_font", font_code)
	code_label.add_theme_font_size_override("normal_font_size", 44)
	code_label.add_theme_color_override("default_color", Color("#E6E9F5"))
	code_label.add_theme_constant_override("line_separation", 18)
	code.add_child(code_label)

	var step_btn := _panel(Color(0.22, 0.83, 1.0, 0.12), Color(0.22, 0.83, 1.0, 0.45), 36)
	step_btn.position = Vector2(48, 2120)
	step_btn.custom_minimum_size = Vector2(476, 150)
	root.add_child(step_btn)
	var step_text := _label("Adım adım", font_heading, 50, Color("#39D5FF"))
	step_text.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	step_text.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	step_btn.add_child(step_text)

	var run_btn := _panel(Color("#FF7A3D"), Color("#FF9A66"), 36)
	run_btn.position = Vector2(556, 2120)
	run_btn.custom_minimum_size = Vector2(476, 150)
	root.add_child(run_btn)
	var run_text := _label("Çalıştır", font_heading, 54, Color("#1A0E08"))
	run_text.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	run_text.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	run_btn.add_child(run_text)

	_highlight(-1)


func _highlight(line: int) -> void:
	if not code_label:
		return
	var out := ""
	for i in CODE_LINES.size():
		var number := "[color=#4B5270]%d[/color]  " % (i + 1)
		var body: String = CODE_LINES[i]
		if i == line:
			out += "[bgcolor=#2B4270]" + number + body + "   [/bgcolor]\n"
		else:
			out += number + body + "\n"
	code_label.text = out


func _label(text: String, font: Font, size: int, color: Color) -> Label:
	var l := Label.new()
	l.text = text
	l.add_theme_font_override("font", font)
	l.add_theme_font_size_override("font_size", size)
	l.add_theme_color_override("font_color", color)
	return l


func _panel(bg: Color, border: Color, radius: int) -> PanelContainer:
	var p := PanelContainer.new()
	var box := StyleBoxFlat.new()
	box.bg_color = bg
	box.border_color = border
	box.set_border_width_all(3)
	box.set_corner_radius_all(radius)
	box.content_margin_left = 40
	box.content_margin_right = 40
	box.content_margin_top = 24
	box.content_margin_bottom = 24
	p.add_theme_stylebox_override("panel", box)
	return p


# --- yardımcılar ---

func _cell_pos(cell: Vector2i) -> Vector3:
	return Vector3((cell.x - (COLS - 1) / 2.0) * CELL, 0.0, (cell.y - (ROWS - 1) / 2.0) * CELL)


## Kareye özgü, her seferinde aynı çıkan rastgele sayı (0–1)
func _rand(x: int, y: int, salt: int) -> float:
	var h := hash(Vector3i(x, y, salt))
	return float(h % 10007) / 10007.0


## Modeli kurar: tabanı y=0'a, ortası (x, z)'ye oturur; yatayda `size` kadar yer kaplar.
func _spawn(model: String, pos: Vector3, size: float, rot_y := 0.0, parent: Node3D = null) -> Node3D:
	var scene: PackedScene = load("res://models/%s.glb" % model)
	var inst: Node3D = scene.instantiate()
	_true_colors(inst)
	var box := _mesh_aabb(inst, Transform3D.IDENTITY)
	var pivot := Node3D.new()
	pivot.add_child(inst)
	inst.position = -Vector3(box.get_center().x, box.position.y, box.get_center().z)
	var k := size / maxf(box.size.x, box.size.z)
	pivot.scale = Vector3.ONE * k
	pivot.position = pos
	pivot.rotation.y = rot_y
	pivot.set_meta("height", box.size.y * k)
	pivot.set_meta("scale", pivot.scale)
	(parent if parent else world).add_child(pivot)
	return pivot


var _color_cache := {}  # kaynak malzeme -> düzeltilmiş kopyası


## Kenney renkleri sRGB olarak tasarlanmış ama dosyada doğrusal diye okunuyor; soluk kalmasın
## diye gerçek tonlarına çevrilir (Mars kiremit kırmızısı, turuncu şeritler...).
func _true_colors(node: Node) -> void:
	for mi in _meshes(node):
		for s in mi.mesh.get_surface_count():
			var src := mi.mesh.surface_get_material(s) as StandardMaterial3D
			if not src:
				continue
			if not _color_cache.has(src):
				var m := src.duplicate() as StandardMaterial3D
				m.albedo_color = src.albedo_color.srgb_to_linear()
				m.resource_name = src.resource_name
				_color_cache[src] = m
			mi.set_surface_override_material(s, _color_cache[src])


func _mesh_aabb(node: Node, xf: Transform3D) -> AABB:
	var t := xf
	if node is Node3D:
		t = xf * (node as Node3D).transform
	var box := AABB()
	var has := false
	if node is MeshInstance3D:
		box = t * (node as MeshInstance3D).get_aabb()
		has = true
	for c in node.get_children():
		var cb := _mesh_aabb(c, t)
		if cb.size != Vector3.ZERO:
			box = cb if not has else box.merge(cb)
			has = true
	return box


func _meshes(node: Node) -> Array[MeshInstance3D]:
	var out: Array[MeshInstance3D] = []
	if node is MeshInstance3D:
		out.append(node)
	for c in node.get_children():
		out.append_array(_meshes(c))
	return out


func _set_cast_shadow(node: Node, on: bool) -> void:
	for m in _meshes(node):
		m.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_ON if on else GeometryInstance3D.SHADOW_CASTING_SETTING_OFF


## Kristalli kayayı buza çevirir: kaya kısmı buz beyazı, kristaller camgöbeği ve kendinden parlak.
func _make_icy(node: Node3D) -> void:
	for mi in _meshes(node):
		for s in mi.mesh.get_surface_count():
			var src := mi.mesh.surface_get_material(s) as StandardMaterial3D
			if not src:
				continue
			var m := src.duplicate() as StandardMaterial3D
			if src.resource_name == "crystal":
				m.albedo_color = Color("#9BEBFF")
				m.emission_enabled = true
				m.emission = Color("#35C4FF")
				m.roughness = 0.15
				crystal_materials.append(m)
			else:
				m.albedo_color = Color("#F4FCFF")
				m.emission_enabled = true
				m.emission = Color("#9FE4FF")
				m.emission_energy_multiplier = 0.35
				m.roughness = 0.35
			mi.set_surface_override_material(s, m)
	var glow := OmniLight3D.new()
	glow.light_color = Color("#6FD8FF")
	glow.light_energy = 0.8
	glow.omni_range = 1.1
	glow.position = Vector3(0, 0.4, 0)
	node.add_child(glow)


## Seranın kubbesi: içeriden yeşil ışık vuran cam
func _make_greenhouse(node: Node3D) -> void:
	for mi in _meshes(node):
		for s in mi.mesh.get_surface_count():
			var src := mi.mesh.surface_get_material(s) as StandardMaterial3D
			if src and src.resource_name == "dark":
				var m := src.duplicate() as StandardMaterial3D
				m.albedo_color = Color(0.5, 0.95, 0.75, 0.8)
				m.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
				m.emission_enabled = true
				m.emission = Color("#3EDC8C")
				m.emission_energy_multiplier = 1.2
				m.roughness = 0.1
				mi.set_surface_override_material(s, m)


func _add_blink_light(pos: Vector3, color: Color, parent: Node3D = null) -> void:
	var bulb := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.035
	sphere.height = 0.07
	bulb.mesh = sphere
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.emission_enabled = true
	mat.emission = color
	mat.emission_energy_multiplier = 4.0
	bulb.material_override = mat
	bulb.position = pos
	var light := OmniLight3D.new()
	light.light_color = color
	light.omni_range = 0.9
	light.position = pos
	(parent if parent else world).add_child(bulb)
	(parent if parent else world).add_child(light)
	blink_lights.append(light)


func _particle_mesh(radius: float, color: Color, emission: float) -> Mesh:
	var mesh := SphereMesh.new()
	mesh.radius = radius
	mesh.height = radius * 2.0
	mesh.radial_segments = 6
	mesh.rings = 3
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.vertex_color_use_as_albedo = true
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	if emission > 0.0:
		mat.emission_enabled = true
		mat.emission = color
		mat.emission_energy_multiplier = emission
	mesh.material = mat
	return mesh


func _fade_ramp(from: Color, to: Color) -> GradientTexture1D:
	var g := Gradient.new()
	g.set_color(0, from)
	g.set_color(1, to)
	var tex := GradientTexture1D.new()
	tex.gradient = g
	return tex


# --- ekran görüntüsü (geliştirme için) ---

func _setup_shots() -> void:
	var prefix := ""
	var times: Array[float] = []
	for a in OS.get_cmdline_user_args():
		if a.begins_with("--shots="):
			prefix = a.substr(8)
		elif a.begins_with("--at="):
			for t in a.substr(5).split(","):
				times.append(float(t))
	if prefix == "" or times.is_empty():
		return
	for i in times.size():
		var path := "%s-%d.png" % [prefix, i + 1]
		get_tree().create_timer(times[i]).timeout.connect(func() -> void: _shot(path))
	get_tree().create_timer(times.max() + 0.5).timeout.connect(func() -> void: get_tree().quit())


func _shot(path: String) -> void:
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png(path)
