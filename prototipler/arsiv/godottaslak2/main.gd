extends Node3D
## MarsKod görsel denemesi — godottaslak2. Sahnenin tamamını koddan kurar:
## ufka uzanan Mars yüzeyi, ışıklı kod bölgesi, arkada yaşayan koloni, robotun kodu satır satır
## çalıştırma animasyonu ve buzlu cam arayüz.
##
## Ekran görüntüsü almak için: godot --path . -- --shots=<dosya-öneki> --at=2,4,6

const COLS := 6
const ROWS := 6

# Kod bölgesi haritası: . boş · I buz · R kaya · @ robot · D depo
const MAP := [
	"..R...",
	"....I.",
	"@III..",
	"......",
	"..R.D.",
	".I...R",
]

const CODE_LINES := [
	"[color=#FF9E64]for[/color] i [color=#FF9E64]in[/color] [color=#7DCFFF]range[/color]([color=#E0AF68]3[/color]):",
	"    [color=#7DCFFF]move[/color]([color=#9ECE6A]\"east\"[/color])",
	"    [color=#FF9E64]if[/color] [color=#7DCFFF]ice_here[/color]():",
	"        [color=#7DCFFF]collect[/color]()",
]

# Arayüz ölçüleri (1080 × 2400 tasarım alanı)
const CODE_TOP := 1606.0
const LINE_H := 86.0
const CHIP_ICON_POS := Vector2(792, 176)

# Kamera
const CAM_POS := Vector3(0, 6.9, 7.4)
const CAM_TARGET := Vector3(0, -0.3, 0.7)

var world: Node3D
var camera: Camera3D
var rover: Node3D
var rover_body: Node3D
var rover_cell := Vector2i.ZERO
var rover_start := Vector2i.ZERO
var dust_trail: GPUParticles3D
var ices := {}  # Vector2i -> Node3D
var crystal_materials: Array[StandardMaterial3D] = []
var blinkers: Array = []  # [OmniLight3D, StandardMaterial3D, faz]
var spinners: Array[Node3D] = []
var ship: Node3D
var ice_count := 0
var time := 0.0

var hud: Control
var ice_label: Label
var chip_bar: ColorRect
var mission_bar: ColorRect
var line_bar: Control
var code_labels: Array[RichTextLabel] = []
var line_numbers: Array[Label] = []

var noise_dunes := FastNoiseLite.new()
var noise_detail := FastNoiseLite.new()
var noise_mesa := FastNoiseLite.new()

var font_heading: FontFile
var font_label: FontFile
var font_body: FontFile
var font_code: FontFile


func _ready() -> void:
	font_heading = load("res://fonts/ChakraPetch_700Bold.ttf")
	font_label = load("res://fonts/ChakraPetch_600SemiBold.ttf")
	font_body = load("res://fonts/ChakraPetch_500Medium.ttf")
	font_code = load("res://fonts/JetBrainsMono_400Regular.ttf")
	noise_dunes.seed = 11
	noise_dunes.frequency = 0.06
	noise_dunes.fractal_octaves = 3
	noise_detail.seed = 23
	noise_detail.frequency = 0.25
	noise_mesa.seed = 5
	noise_mesa.frequency = 0.045

	world = Node3D.new()
	add_child(world)
	_build_environment()
	_build_lights()
	_build_camera()
	_build_terrain()
	_build_zone()
	_build_map()
	_build_colony()
	_build_scatter()
	_build_ship()
	_build_ambient_dust()
	_build_hud()
	_setup_shots()
	_run_demo()


func _process(delta: float) -> void:
	time += delta
	for i in crystal_materials.size():
		crystal_materials[i].emission_energy_multiplier = 1.0 + 0.45 * sin(time * 2.2 + i * 1.3)
	for b in blinkers:
		var on := fmod(time + b[2], 1.6) < 0.22
		(b[0] as OmniLight3D).light_energy = 2.2 if on else 0.0
		(b[1] as StandardMaterial3D).emission_energy_multiplier = 6.0 if on else 0.3
	for s in spinners:
		s.rotation.y += delta * 0.35
	if rover_body:
		rover_body.position.y = abs(sin(time * 5.0)) * 0.02
	if camera:
		var a := sin(time * 0.22) * 0.035
		camera.position = CAM_POS.rotated(Vector3.UP, a)
		camera.look_at(CAM_TARGET)


# --- ortam ve ışık ---

func _build_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	var sky_mat := ShaderMaterial.new()
	sky_mat.shader = load("res://mars_sky.gdshader")
	sky.sky_material = sky_mat
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_energy = 0.55
	env.ambient_light_sky_contribution = 0.85
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.tonemap_exposure = 0.9
	env.fog_enabled = true
	env.fog_light_color = Color("#D08C62")
	env.fog_light_energy = 0.75
	env.fog_density = 0.0085
	env.fog_sky_affect = 0.35
	env.glow_enabled = true
	env.glow_intensity = 0.6
	env.glow_bloom = 0.02
	env.glow_hdr_threshold = 1.0
	env.adjustment_enabled = true
	env.adjustment_saturation = 1.18
	env.adjustment_contrast = 1.14
	var we := WorldEnvironment.new()
	we.environment = env
	add_child(we)


func _build_lights() -> void:
	# alçak altın saat güneşi: sol önden, uzun gölgeler
	var sun := DirectionalLight3D.new()
	sun.light_color = Color("#FFD8B0")
	sun.light_energy = 1.55
	sun.shadow_enabled = true
	sun.shadow_blur = 1.2
	sun.directional_shadow_max_distance = 40.0
	add_child(sun)
	sun.look_at_from_position(Vector3(-9, 7, 6), Vector3.ZERO)
	# arka kenar ışığı: soğuk mavi, hatları ayırır
	var rim := DirectionalLight3D.new()
	rim.light_color = Color("#9FB8FF")
	rim.light_energy = 0.4
	add_child(rim)
	rim.look_at_from_position(Vector3(7, 5, -9), Vector3.ZERO)


func _build_camera() -> void:
	camera = Camera3D.new()
	camera.keep_aspect = Camera3D.KEEP_WIDTH
	camera.fov = 52.0
	camera.v_offset = -1.75  # sahneyi ekranın üst yarısına taşır (alt yarıda kod paneli var)
	camera.position = CAM_POS
	var attrs := CameraAttributesPractical.new()
	attrs.dof_blur_far_enabled = true
	attrs.dof_blur_far_distance = 17.0
	attrs.dof_blur_far_transition = 14.0
	attrs.dof_blur_amount = 0.06
	camera.attributes = attrs
	add_child(camera)
	camera.look_at(CAM_TARGET)


# --- Mars yüzeyi ---

func _height(x: float, z: float) -> float:
	var d := maxf(absf(x), absf(z))
	var near := smoothstep(3.8, 7.5, d)
	var h := -0.22 + near * (noise_dunes.get_noise_2d(x, z) * 0.9 + noise_detail.get_noise_2d(x, z) * 0.12)
	var far := smoothstep(13.0, 26.0, d)
	h += far * smoothstep(0.12, 0.2, noise_mesa.get_noise_2d(x, z)) * 6.0
	return h


func _build_terrain() -> void:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var half := 48
	var step := 1.0
	var low := Color("#9A3E24")
	var high := Color("#C97649")
	var mesa := Color("#B85C3C")
	for iz in range(-half, half):
		for ix in range(-half, half):
			var x0 := ix * step
			var z0 := iz * step
			var p00 := Vector3(x0, _height(x0, z0), z0)
			var p10 := Vector3(x0 + step, _height(x0 + step, z0), z0)
			var p01 := Vector3(x0, _height(x0, z0 + step), z0 + step)
			var p11 := Vector3(x0 + step, _height(x0 + step, z0 + step), z0 + step)
			var k := 0
			for tri in [[p00, p10, p01], [p10, p11, p01]]:
				var cy: float = (tri[0].y + tri[1].y + tri[2].y) / 3.0
				var shade := 0.9 + 0.2 * _rand(ix, iz, k)
				var col := low.lerp(high, clampf((cy + 0.4) / 1.4, 0.0, 1.0))
				if cy > 1.5:
					col = mesa.lerp(high, clampf((cy - 1.5) / 5.0, 0.0, 1.0) * 0.6)
				_tri(st, tri[0], tri[1], tri[2], col * shade)
				k += 1
	var mesh := st.commit()
	var mi := MeshInstance3D.new()
	mi.mesh = mesh
	var mat := StandardMaterial3D.new()
	mat.vertex_color_use_as_albedo = true
	mat.roughness = 1.0
	mi.material_override = mat
	world.add_child(mi)


## Yukarı bakan, köşeli gölgelenmiş üçgen (Godot'da ön yüz saat yönündedir)
func _tri(st: SurfaceTool, a: Vector3, b: Vector3, c: Vector3, color: Color) -> void:
	var n := (b - a).cross(c - a)
	if n.y > 0.0:
		var t := b
		b = c
		c = t
		n = -n
	var normal := -n.normalized()
	st.set_color(color)
	st.set_normal(normal)
	st.add_vertex(a)
	st.add_vertex(b)
	st.add_vertex(c)


# --- kod bölgesi ---

func _build_zone() -> void:
	# metal platform
	var base := MeshInstance3D.new()
	var box := BoxMesh.new()
	box.size = Vector3(COLS + 0.5, 0.36, ROWS + 0.5)
	base.mesh = box
	var metal := StandardMaterial3D.new()
	metal.albedo_color = Color("#8E8A92")
	metal.metallic = 0.4
	metal.roughness = 0.45
	base.material_override = metal
	base.position.y = -0.18
	world.add_child(base)
	# turuncu uyarı şeridi
	var stripe := MeshInstance3D.new()
	var sbox := BoxMesh.new()
	sbox.size = Vector3(COLS + 0.52, 0.06, ROWS + 0.52)
	stripe.mesh = sbox
	var orange := StandardMaterial3D.new()
	orange.albedo_color = Color("#FF8A3D")
	orange.emission_enabled = true
	orange.emission = Color("#FF6A1A")
	orange.emission_energy_multiplier = 0.4
	stripe.material_override = orange
	stripe.position.y = -0.1
	world.add_child(stripe)
	# ışıklı zemin
	var zone_floor := MeshInstance3D.new()
	var plane := PlaneMesh.new()
	plane.size = Vector2(COLS, ROWS)
	zone_floor.mesh = plane
	var zmat := ShaderMaterial.new()
	zmat.shader = load("res://zone.gdshader")
	zone_floor.material_override = zmat
	zone_floor.position.y = 0.002
	zone_floor.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	world.add_child(zone_floor)
	# köşelerde işaret direkleri
	var hx := COLS / 2.0 + 0.2
	var hz := ROWS / 2.0 + 0.2
	var phase := 0.0
	for corner in [Vector3(-hx, 0, -hz), Vector3(hx, 0, -hz), Vector3(hx, 0, hz), Vector3(-hx, 0, hz)]:
		var pole := MeshInstance3D.new()
		var cyl := CylinderMesh.new()
		cyl.top_radius = 0.035
		cyl.bottom_radius = 0.05
		cyl.height = 0.6
		pole.mesh = cyl
		pole.material_override = metal
		pole.position = corner + Vector3(0, 0.3, 0)
		world.add_child(pole)
		_add_blinker(corner + Vector3(0, 0.64, 0), Color("#FF8A3D"), phase)
		phase += 0.4


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
					var ice := _spawn(model, pos, 0.86, turn)
					_make_icy(ice)
					ices[cell] = ice
				"R":
					_spawn("rock_largeA" if _rand(gx, gy, 7) < 0.5 else "rock_largeB", pos, 0.72, turn)
				"@":
					rover_start = cell
					_build_rover()
				"D":
					_spawn("hangar_smallA", pos, 0.95, -PI / 2.0)


# --- arkadaki koloni ---

func _build_colony() -> void:
	_spawn("hangar_largeA", Vector3(-6.2, _height(-6.2, -8.0), -8.0), 3.2, 0.3)
	_spawn("hangar_roundA", Vector3(-2.6, _height(-2.6, -9.6), -9.6), 1.9)
	var greenhouse := _spawn("hangar_roundGlass", Vector3(0.4, _height(0.4, -8.8), -8.8), 2.1)
	_make_greenhouse(greenhouse)
	var lab := _spawn("hangar_roundB", Vector3(3.4, _height(3.4, -8.4), -8.4), 1.9)
	var dish := _spawn("satelliteDish", Vector3(3.4, _height(3.4, -8.4) + lab.get_meta("height"), -8.4), 0.8)
	spinners.append(dish)
	_add_blinker(Vector3(3.4, _height(3.4, -8.4) + lab.get_meta("height") + 0.9, -8.4), Color("#FF3B3B"), 0.7)
	for i in 3:
		_spawn("corridor", Vector3(-4.4 + i * 1.0, _height(-4.4 + i, -8.9), -8.9), 1.0, PI / 2.0)
	# roket ve rampası
	var rx := 8.2
	var rz := -8.6
	var ground := _height(rx, rz)
	_spawn("platform_large", Vector3(rx, ground, rz), 2.6)
	var base := _spawn("rocket_baseA", Vector3(rx, ground + 0.12, rz), 1.2)
	var fuel := _spawn("rocket_fuelA", Vector3(rx, ground + 0.12 + base.get_meta("height"), rz), 0.95)
	var top := _spawn("rocket_topA", Vector3(rx, ground + 0.12 + base.get_meta("height") + fuel.get_meta("height"), rz), 0.95)
	_add_blinker(Vector3(rx, top.position.y + top.get_meta("height") + 0.15, rz), Color("#39D5FF"), 0.2)
	_steam(Vector3(rx, ground + 0.2, rz))
	var big_dish := _spawn("satelliteDish_large", Vector3(-8.8, _height(-8.8, -2.5), -2.5), 1.8, 0.8)
	spinners.append(big_dish)
	_spawn("craft_cargoA", Vector3(-7.4, _height(-7.4, 1.8), 1.8), 2.0, 2.4)
	_spawn("machine_generator", Vector3(5.2, _height(5.2, 1.2), 1.2), 0.9, 1.2)
	_spawn("barrels", Vector3(5.0, _height(5.0, 3.2), 3.2), 0.8, 0.4)
	_spawn("astronautA", Vector3(-4.2, _height(-4.2, -6.4), -6.4), 0.55, 0.6)
	_spawn("astronautB", Vector3(-3.6, _height(-3.6, -6.1), -6.1), 0.55, -2.4)
	_spawn("astronautA", Vector3(5.6, _height(5.6, 2.2), 2.2), 0.55, 3.8)


func _build_scatter() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 99
	var placed := 0
	while placed < 70:
		var x := rng.randf_range(-16.0, 16.0)
		var z := rng.randf_range(-14.0, 12.0)
		if maxf(absf(x), absf(z)) < 4.2:
			continue
		var r := rng.randf()
		var model := "rocks_smallA" if r < 0.35 else ("rocks_smallB" if r < 0.6 else ("rock" if r < 0.8 else "crater"))
		var size := rng.randf_range(0.4, 1.1) if model != "crater" else rng.randf_range(0.8, 1.8)
		_spawn(model, Vector3(x, _height(x, z) - 0.03, z), size, rng.randf() * TAU)
		placed += 1
	# birkaç büyük kaya
	for p in [Vector3(-10.5, 0, 4.5), Vector3(11.0, 0, -2.0), Vector3(-12.5, 0, -6.0), Vector3(9.5, 0, 6.5)]:
		_spawn("rock_largeA", Vector3(p.x, _height(p.x, p.z), p.z), 2.4, _rand(int(p.x), int(p.z), 1) * TAU)


# --- gökyüzünden geçen uzay aracı ---

func _build_ship() -> void:
	ship = _spawn("craft_speederA", Vector3.ZERO, 1.6, 0.0)
	var glow := OmniLight3D.new()
	glow.light_color = Color("#6FD8FF")
	glow.light_energy = 3.0
	glow.omni_range = 3.0
	ship.add_child(glow)
	var trail := GPUParticles3D.new()
	trail.amount = 60
	trail.lifetime = 1.4
	trail.local_coords = false
	var pm := ParticleProcessMaterial.new()
	pm.direction = Vector3(-1, 0, 0)
	pm.spread = 8.0
	pm.initial_velocity_min = 0.5
	pm.initial_velocity_max = 1.0
	pm.gravity = Vector3.ZERO
	pm.scale_min = 1.0
	pm.scale_max = 2.0
	pm.color_ramp = _fade_ramp(Color(0.6, 0.9, 1.0, 0.9), Color(0.6, 0.9, 1.0, 0.0))
	trail.process_material = pm
	trail.draw_pass_1 = _particle_mesh(0.06, Color("#9FE4FF"), 3.0)
	ship.add_child(trail)
	_fly_ship()


func _fly_ship() -> void:
	while true:
		ship.position = Vector3(-34, 7.5, -22)
		ship.rotation = Vector3(0.05, -PI / 2.0, 0.08)
		var tw := create_tween()
		tw.tween_property(ship, "position", Vector3(34, 9.5, -16), 9.0).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
		await tw.finished
		await _wait(4.0)


# --- robot ---

func _build_rover() -> void:
	rover = Node3D.new()
	world.add_child(rover)
	rover_body = Node3D.new()
	rover.add_child(rover_body)
	var model := _spawn("rover", Vector3.ZERO, 0.82, 0.0, rover_body)
	model.rotation.y = -PI / 2.0
	var head := SpotLight3D.new()
	head.light_color = Color("#FFF2C8")
	head.light_energy = 4.0
	head.spot_range = 2.6
	head.spot_angle = 32.0
	head.position = Vector3(0.28, 0.32, 0)
	head.rotation_degrees = Vector3(-22, -90, 0)
	rover_body.add_child(head)
	_add_blinker(Vector3(-0.12, 0.6, 0), Color("#39D5FF"), 0.5, rover_body)
	dust_trail = GPUParticles3D.new()
	dust_trail.amount = 50
	dust_trail.lifetime = 1.3
	dust_trail.emitting = false
	dust_trail.local_coords = false
	var pm := ParticleProcessMaterial.new()
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	pm.emission_box_extents = Vector3(0.12, 0.02, 0.22)
	pm.direction = Vector3(0, 1, 0)
	pm.spread = 50.0
	pm.initial_velocity_min = 0.3
	pm.initial_velocity_max = 0.7
	pm.gravity = Vector3(0, -0.25, 0)
	pm.scale_min = 1.2
	pm.scale_max = 2.8
	pm.color_ramp = _fade_ramp(Color(0.86, 0.56, 0.38, 0.7), Color(0.86, 0.56, 0.38, 0.0))
	dust_trail.process_material = pm
	dust_trail.draw_pass_1 = _particle_mesh(0.05, Color("#D9956E"), 0.0)
	dust_trail.position = Vector3(-0.32, 0.05, 0)
	rover.add_child(dust_trail)
	_place_rover(rover_start)


func _place_rover(cell: Vector2i) -> void:
	rover_cell = cell
	rover.position = _cell_pos(cell)
	rover.rotation.y = 0.0


func _move_rover(target: Vector2i) -> void:
	await _show_path(rover_cell, target)
	var dir := Vector2(target - rover_cell)
	var yaw := atan2(-dir.y, dir.x)
	var tw := create_tween().set_parallel(true)
	tw.tween_property(rover, "rotation:y", yaw, 0.25).set_trans(Tween.TRANS_SINE)
	tw.tween_property(rover, "position", _cell_pos(target), 0.8).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_IN_OUT)
	dust_trail.emitting = true
	await tw.finished
	dust_trail.emitting = false
	rover_cell = target


## Robot gitmeden önce yolunu parlayan oklarla gösterir.
func _show_path(from: Vector2i, to: Vector2i) -> void:
	var a := _cell_pos(from)
	var b := _cell_pos(to)
	var yaw := atan2(-(b.z - a.z), b.x - a.x)
	for k in 3:
		var chevron := MeshInstance3D.new()
		var prism := PrismMesh.new()
		prism.size = Vector3(0.22, 0.2, 0.02)
		chevron.mesh = prism
		var mat := StandardMaterial3D.new()
		mat.albedo_color = Color(0.4, 0.9, 1.0, 0.0)
		mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
		mat.emission_enabled = true
		mat.emission = Color("#39D5FF")
		mat.emission_energy_multiplier = 2.5
		mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
		chevron.material_override = mat
		chevron.position = a.lerp(b, (k + 1) / 4.0) + Vector3(0, 0.03, 0)
		chevron.rotation = Vector3(-PI / 2.0, yaw - PI / 2.0, 0)
		chevron.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		world.add_child(chevron)
		var tw := create_tween()
		tw.tween_interval(k * 0.08)
		tw.tween_property(mat, "albedo_color:a", 0.95, 0.12)
		tw.tween_interval(0.45)
		tw.tween_property(mat, "albedo_color:a", 0.0, 0.3)
		tw.tween_callback(chevron.queue_free)
	await _wait(0.3)


func _collect(cell: Vector2i) -> void:
	var ice: Node3D = ices[cell]
	var pos := ice.position
	_beam(rover.position + Vector3(0, 0.55, 0), pos + Vector3(0, 0.3, 0))
	await _wait(0.2)
	var tw := create_tween()
	tw.tween_property(ice, "scale", ice.scale * 1.18, 0.12).set_trans(Tween.TRANS_SINE)
	tw.tween_property(ice, "scale", Vector3.ONE * 0.001, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_IN)
	_burst(pos + Vector3(0, 0.35, 0))
	_float_text("+1 buz", pos + Vector3(0, 0.95, 0))
	_fly_icon(pos + Vector3(0, 0.4, 0))
	await tw.finished
	ice.visible = false


## Robotun anteninden buza uzanan tarayıcı ışın
func _beam(from: Vector3, to: Vector3) -> void:
	var beam := MeshInstance3D.new()
	var cyl := CylinderMesh.new()
	cyl.top_radius = 0.025
	cyl.bottom_radius = 0.06
	cyl.height = maxf(from.distance_to(to), 0.1)
	beam.mesh = cyl
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.5, 0.95, 1.0, 0.0)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.emission_enabled = true
	mat.emission = Color("#6FE0FF")
	mat.emission_energy_multiplier = 4.0
	beam.material_override = mat
	beam.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	world.add_child(beam)
	beam.position = (from + to) / 2.0
	if from.distance_to(to) > 0.01 and absf((to - from).normalized().dot(Vector3.UP)) < 0.99:
		beam.look_at(to, Vector3.UP)
		beam.rotate_object_local(Vector3.RIGHT, PI / 2.0)
	var tw := create_tween()
	tw.tween_property(mat, "albedo_color:a", 0.85, 0.1)
	tw.tween_interval(0.35)
	tw.tween_property(mat, "albedo_color:a", 0.0, 0.25)
	tw.tween_callback(beam.queue_free)


func _burst(pos: Vector3) -> void:
	var p := GPUParticles3D.new()
	p.one_shot = true
	p.amount = 70
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
	label.font_size = 72
	label.pixel_size = 0.007
	label.modulate = Color("#CFF7FF")
	label.outline_modulate = Color(0.04, 0.08, 0.18)
	label.outline_size = 14
	label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	label.no_depth_test = true
	label.position = pos
	world.add_child(label)
	var tw := create_tween().set_parallel(true)
	tw.tween_property(label, "position", pos + Vector3(0, 0.8, 0), 1.1).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tw.tween_property(label, "modulate:a", 0.0, 0.8).set_delay(0.35)
	tw.chain().tween_callback(label.queue_free)


## Toplanan buzun simgesi ekranda uçarak sayaca gider; sayaç ve görev çubuğu dolar.
func _fly_icon(world_pos: Vector3) -> void:
	var screen := camera.unproject_position(world_pos)
	var start: Vector2 = get_viewport().get_final_transform().affine_inverse() * screen
	var icon := _crystal_icon(1.0)
	icon.position = start
	hud.add_child(icon)
	var mid := start.lerp(CHIP_ICON_POS, 0.5) + Vector2(80, -160)
	var tw := create_tween()
	tw.tween_method(func(t: float) -> void:
		var p1 := start.lerp(mid, t)
		var p2 := mid.lerp(CHIP_ICON_POS, t)
		icon.position = p1.lerp(p2, t)
		icon.scale = Vector2.ONE * lerpf(1.6, 0.9, t)
		icon.rotation = t * TAU * 0.5, 0.0, 1.0, 0.75).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	await tw.finished
	icon.queue_free()
	ice_count += 1
	ice_label.text = "%d/3" % ice_count
	var pop := create_tween()
	pop.tween_property(ice_label, "scale", Vector2(1.3, 1.3), 0.1)
	pop.tween_property(ice_label, "scale", Vector2.ONE, 0.2)
	var fill := create_tween().set_parallel(true)
	fill.tween_property(chip_bar, "size:x", 180.0 * ice_count / 3.0, 0.35).set_trans(Tween.TRANS_CUBIC)
	fill.tween_property(mission_bar, "size:x", 900.0 * ice_count / 3.0, 0.35).set_trans(Tween.TRANS_CUBIC)


# Robot, kod panelindeki programı satır satır "çalıştırır"; sonra sahne başa döner.
func _run_demo() -> void:
	while true:
		_reset_demo()
		await _wait(1.2)
		for i in 3:
			_highlight(0)
			await _wait(0.45)
			_highlight(1)
			await _move_rover(rover_cell + Vector2i(1, 0))
			_highlight(2)
			await _wait(0.4)
			if ices.has(rover_cell) and ices[rover_cell].visible:
				_highlight(3)
				await _collect(rover_cell)
				await _wait(0.5)
		_highlight(-1)
		await _wait(2.8)


func _reset_demo() -> void:
	_place_rover(rover_start)
	for cell in ices:
		var ice: Node3D = ices[cell]
		ice.visible = true
		ice.scale = ice.get_meta("scale")
	ice_count = 0
	if ice_label:
		ice_label.text = "0/3"
		chip_bar.size.x = 0.0
		mission_bar.size.x = 0.0
	_highlight(-1)


func _wait(seconds: float) -> void:
	await get_tree().create_timer(seconds).timeout


# --- arayüz ---

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	hud = Control.new()
	hud.set_anchors_preset(Control.PRESET_FULL_RECT)
	hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layer.add_child(hud)

	# üst çubuk
	_glass(Rect2(32, 72, 1016, 200), 44)
	_text("DÜNYA 1 · İNİŞ BÖLGESİ", font_label, 30, Color("#5FD8FF"), Vector2(80, 104))
	_text("Buz Avı", font_heading, 66, Color("#F3EEFA"), Vector2(80, 140))
	var badge := _glass(Rect2(336, 158, 170, 56), 28, Color(1.0, 0.48, 0.24, 0.85), Color(1, 0.8, 0.6, 0.6))
	(badge.material as ShaderMaterial).set_shader_parameter("blur", 0.0)
	_text("BÖLÜM 3", font_heading, 30, Color("#1A0E08"), Vector2(360, 164))
	# kaynak sayacı
	_glass(Rect2(700, 102, 316, 142), 36, Color(0.02, 0.05, 0.1, 0.55), Color(0.4, 0.85, 1.0, 0.5))
	var icon := _crystal_icon(0.9)
	icon.position = CHIP_ICON_POS
	hud.add_child(icon)
	ice_label = _text("0/3", font_heading, 58, Color("#F3EEFA"), Vector2(840, 128))
	ice_label.pivot_offset = Vector2(50, 36)
	_bar(Rect2(760, 214, 180, 10), Color(1, 1, 1, 0.12))
	chip_bar = _bar(Rect2(760, 214, 0, 10), Color("#5FE0FF"))

	# görev kartı
	_glass(Rect2(32, 1352, 1016, 184), 40)
	var target := Node2D.new()
	target.position = Vector2(96, 1420)
	for r in [[30.0, Color("#FF7A3D")], [20.0, Color("#1A1426")], [11.0, Color("#FF7A3D")]]:
		var c := Polygon2D.new()
		c.polygon = _circle(r[0])
		c.color = r[1]
		target.add_child(c)
	hud.add_child(target)
	_text("GÖREV", font_label, 26, Color("#FF9E64"), Vector2(150, 1382))
	_text("Doğudaki 3 buzu topla", font_body, 42, Color("#F3EEFA"), Vector2(150, 1414))
	_bar(Rect2(90, 1492, 900, 12), Color(1, 1, 1, 0.1))
	mission_bar = _bar(Rect2(90, 1492, 0, 12), Color("#FF8A3D"))

	# kod editörü
	_glass(Rect2(32, 1560, 1016, 548), 40, Color(0.03, 0.05, 0.11, 0.86))
	var dot_x := 84.0
	for col in [Color("#FF5F57"), Color("#FEBC2E"), Color("#28C840")]:
		var dot := Polygon2D.new()
		dot.polygon = _circle(10.0)
		dot.color = col
		dot.position = Vector2(dot_x, 1600)
		hud.add_child(dot)
		dot_x += 34.0
	_text("robot.py", font_label, 30, Color("#8A90A8"), Vector2(200, 1580))
	var sep := ColorRect.new()
	sep.color = Color(1, 1, 1, 0.07)
	sep.position = Vector2(56, 1636)
	sep.size = Vector2(968, 2)
	hud.add_child(sep)
	# o anki satırı gösteren şerit (satırdan satıra kayar)
	line_bar = Control.new()
	line_bar.position = Vector2(48, CODE_TOP + 40)
	line_bar.modulate.a = 0.0
	var glow := TextureRect.new()
	var grad := GradientTexture2D.new()
	var g := Gradient.new()
	g.set_color(0, Color(0.25, 0.8, 1.0, 0.32))
	g.set_color(1, Color(0.25, 0.8, 1.0, 0.0))
	grad.gradient = g
	grad.fill_to = Vector2(1, 0)
	glow.texture = grad
	glow.size = Vector2(984, LINE_H - 10)
	glow.stretch_mode = TextureRect.STRETCH_SCALE
	line_bar.add_child(glow)
	var accent := ColorRect.new()
	accent.color = Color("#5FE0FF")
	accent.size = Vector2(6, LINE_H - 10)
	line_bar.add_child(accent)
	hud.add_child(line_bar)
	for i in CODE_LINES.size():
		var y := CODE_TOP + 40 + i * LINE_H
		line_numbers.append(_text(str(i + 1), font_code, 40, Color("#4B5270"), Vector2(84, y + 8)))
		var rt := RichTextLabel.new()
		rt.bbcode_enabled = true
		rt.fit_content = true
		rt.scroll_active = false
		rt.autowrap_mode = TextServer.AUTOWRAP_OFF
		rt.add_theme_font_override("normal_font", font_code)
		rt.add_theme_font_size_override("normal_font_size", 44)
		rt.add_theme_color_override("default_color", Color("#E6E9F5"))
		rt.position = Vector2(150, y + 4)
		rt.size = Vector2(860, LINE_H)
		rt.text = CODE_LINES[i]
		hud.add_child(rt)
		code_labels.append(rt)

	# düğmeler
	_glass(Rect2(32, 2140, 492, 152), 40, Color(0.05, 0.12, 0.2, 0.7), Color(0.37, 0.88, 1.0, 0.8))
	var step_icon := Polygon2D.new()
	step_icon.polygon = PackedVector2Array([Vector2(0, -18), Vector2(24, 0), Vector2(0, 18)])
	step_icon.color = Color("#5FE0FF")
	step_icon.position = Vector2(150, 2216)
	hud.add_child(step_icon)
	var step_bar := ColorRect.new()
	step_bar.color = Color("#5FE0FF")
	step_bar.position = Vector2(180, 2198)
	step_bar.size = Vector2(7, 36)
	hud.add_child(step_bar)
	_text("Adım adım", font_heading, 48, Color("#5FE0FF"), Vector2(206, 2184))

	var run := Panel.new()
	var box := StyleBoxFlat.new()
	box.bg_color = Color("#FF7A3D")
	box.set_corner_radius_all(40)
	box.border_color = Color("#FFB085")
	box.border_width_top = 3
	box.shadow_color = Color(1.0, 0.45, 0.15, 0.45)
	box.shadow_size = 26
	run.add_theme_stylebox_override("panel", box)
	run.position = Vector2(556, 2140)
	run.size = Vector2(492, 152)
	hud.add_child(run)
	var play := Polygon2D.new()
	play.polygon = PackedVector2Array([Vector2(0, -22), Vector2(34, 0), Vector2(0, 22)])
	play.color = Color("#1A0E08")
	play.position = Vector2(690, 2216)
	hud.add_child(play)
	_text("Çalıştır", font_heading, 52, Color("#1A0E08"), Vector2(744, 2180))

	_highlight(-1)


func _highlight(line: int) -> void:
	if not line_bar:
		return
	for i in line_numbers.size():
		line_numbers[i].add_theme_color_override("font_color", Color("#5FE0FF") if i == line else Color("#4B5270"))
	if line < 0:
		create_tween().tween_property(line_bar, "modulate:a", 0.0, 0.25)
		return
	var y := CODE_TOP + 40 + line * LINE_H - 6
	var tw := create_tween().set_parallel(true)
	tw.tween_property(line_bar, "modulate:a", 1.0, 0.15)
	tw.tween_property(line_bar, "position:y", y, 0.22).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)


func _glass(rect: Rect2, radius: float, tint := Color(0.035, 0.05, 0.11, 0.8), border := Color(0.55, 0.85, 1.0, 0.35)) -> ColorRect:
	var r := ColorRect.new()
	r.position = rect.position
	r.size = rect.size
	r.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var mat := ShaderMaterial.new()
	mat.shader = load("res://glass.gdshader")
	mat.set_shader_parameter("rect_size", rect.size)
	mat.set_shader_parameter("radius", radius)
	mat.set_shader_parameter("tint", tint)
	mat.set_shader_parameter("border", border)
	r.material = mat
	hud.add_child(r)
	return r


func _text(text: String, font: Font, size: int, color: Color, pos: Vector2) -> Label:
	var l := Label.new()
	l.text = text
	l.add_theme_font_override("font", font)
	l.add_theme_font_size_override("font_size", size)
	l.add_theme_color_override("font_color", color)
	l.position = pos
	hud.add_child(l)
	return l


func _bar(rect: Rect2, color: Color) -> ColorRect:
	var r := ColorRect.new()
	r.color = color
	r.position = rect.position
	r.size = rect.size
	hud.add_child(r)
	return r


func _circle(radius: float) -> PackedVector2Array:
	var pts := PackedVector2Array()
	for i in 32:
		var a := i / 32.0 * TAU
		pts.append(Vector2(cos(a), sin(a)) * radius)
	return pts


## Buz kristali simgesi (arayüz için)
func _crystal_icon(k: float) -> Node2D:
	var n := Node2D.new()
	var outer := Polygon2D.new()
	outer.polygon = PackedVector2Array([Vector2(0, -30), Vector2(22, -8), Vector2(13, 28), Vector2(-13, 28), Vector2(-22, -8)])
	outer.color = Color("#5FD8FF")
	n.add_child(outer)
	var inner := Polygon2D.new()
	inner.polygon = PackedVector2Array([Vector2(0, -30), Vector2(22, -8), Vector2(0, 2), Vector2(-22, -8)])
	inner.color = Color("#D8F8FF")
	n.add_child(inner)
	n.scale = Vector2.ONE * k
	return n


# --- yardımcılar ---

func _cell_pos(cell: Vector2i) -> Vector3:
	return Vector3(cell.x - (COLS - 1) / 2.0, 0.0, cell.y - (ROWS - 1) / 2.0)


## Kareye özgü, her seferinde aynı çıkan rastgele sayı (0–1)
func _rand(x: int, y: int, salt: int) -> float:
	return float(hash(Vector3i(x, y, salt)) % 10007) / 10007.0


## Modeli kurar: tabanı pos.y'ye, ortası (x, z)'ye oturur; yatayda `size` kadar yer kaplar.
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


var _color_cache := {}


## Kenney renkleri sRGB olarak tasarlanmış ama dosyada doğrusal diye okunuyor; gerçek tonlarına çevir.
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
				m.albedo_color = Color("#F2FBFF")
				m.emission_enabled = true
				m.emission = Color("#9FE4FF")
				m.emission_energy_multiplier = 0.3
				m.roughness = 0.35
			mi.set_surface_override_material(s, m)
	var glow := OmniLight3D.new()
	glow.light_color = Color("#6FD8FF")
	glow.light_energy = 0.9
	glow.omni_range = 1.2
	glow.position = Vector3(0, 0.4, 0)
	node.add_child(glow)


func _make_greenhouse(node: Node3D) -> void:
	for mi in _meshes(node):
		for s in mi.mesh.get_surface_count():
			var src := mi.mesh.surface_get_material(s) as StandardMaterial3D
			if src and src.resource_name == "dark":
				var m := src.duplicate() as StandardMaterial3D
				m.albedo_color = Color(0.5, 0.95, 0.75, 0.85)
				m.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
				m.emission_enabled = true
				m.emission = Color("#3EDC8C")
				m.emission_energy_multiplier = 1.3
				m.roughness = 0.1
				mi.set_surface_override_material(s, m)


func _add_blinker(pos: Vector3, color: Color, phase: float, parent: Node3D = null) -> void:
	var bulb := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.04
	sphere.height = 0.08
	bulb.mesh = sphere
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.emission_enabled = true
	mat.emission = color
	bulb.material_override = mat
	bulb.position = pos
	var light := OmniLight3D.new()
	light.light_color = color
	light.omni_range = 1.2
	light.position = pos
	(parent if parent else world).add_child(bulb)
	(parent if parent else world).add_child(light)
	blinkers.append([light, mat, phase])


func _steam(pos: Vector3) -> void:
	var p := GPUParticles3D.new()
	p.amount = 40
	p.lifetime = 3.5
	p.preprocess = 3.5
	var pm := ParticleProcessMaterial.new()
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_SPHERE
	pm.emission_sphere_radius = 0.5
	pm.direction = Vector3(0.3, 1, 0)
	pm.spread = 20.0
	pm.initial_velocity_min = 0.2
	pm.initial_velocity_max = 0.5
	pm.gravity = Vector3(0.1, 0.05, 0)
	pm.scale_min = 3.0
	pm.scale_max = 6.0
	pm.color_ramp = _fade_ramp(Color(1, 0.95, 0.9, 0.35), Color(1, 0.9, 0.85, 0.0))
	p.process_material = pm
	p.draw_pass_1 = _particle_mesh(0.08, Color("#F4E8E0"), 0.0)
	p.position = pos
	world.add_child(p)


func _build_ambient_dust() -> void:
	var p := GPUParticles3D.new()
	p.amount = 120
	p.lifetime = 9.0
	p.preprocess = 9.0
	var pm := ParticleProcessMaterial.new()
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	pm.emission_box_extents = Vector3(9.0, 2.0, 8.0)
	pm.direction = Vector3(1, 0.1, 0.2)
	pm.spread = 25.0
	pm.initial_velocity_min = 0.15
	pm.initial_velocity_max = 0.45
	pm.gravity = Vector3.ZERO
	pm.color_ramp = _fade_ramp(Color(1.0, 0.85, 0.7, 0.0), Color(1.0, 0.85, 0.7, 0.75))
	p.process_material = pm
	p.draw_pass_1 = _particle_mesh(0.02, Color("#FFD9B8"), 1.4)
	p.position = Vector3(0, 1.6, -1.0)
	world.add_child(p)


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
