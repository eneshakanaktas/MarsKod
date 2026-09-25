extends Node3D
## MarsKod görsel denemesi — godottaslak4 (Godot'daki son taslak): Mars kanyonu, sinema görünümü.
## Katmanlı kayalıklarla çevrili kanyon tabanı, kanyonun ucunda batan güneş, alçak toz sisi,
## uzakta dönen toz hortumu, ufka düşen meteor, NASA Perseverance gezgini, doğal (tozlu) buz yatakları.
## Oyuna girişte "Animasyon ve efektler" anahtarı; oyun içinde sağ üstteki düğmeyle de tek tuşla kapanır.
##
## Ekran görüntüsü: godot --path . -- --shots=<önek> --at=2,4,6 [--anim=off] [--giris] [--intro]

const COLS := 6
const ROWS := 6

# Kod bölgesi haritası: . boş · I buz · R kaya · @ robot · D kargo kapsülü
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

const AMBER := Color("#FFB547")
const CODE_TOP := 1500.0
const LINE_H := 88.0
const CHIP_ICON_POS := Vector2(806, 156)
const MISSION_W := 560.0

const CAM_POS := Vector3(0, 3.9, 7.2)
const CAM_TARGET := Vector3(0, 0.1, -2.2)
const INTRO_POS := Vector3(-2.5, 17.0, -30.0)
const INTRO_TARGET := Vector3(0, 0.0, -2.0)

var anim_on := true
var started := false
var intro_t := 1.0
var focus_amt := 0.0
var focus_point := Vector3.ZERO

var world: Node3D
var camera: Camera3D
var cam_attrs: CameraAttributesPractical
var sun: DirectionalLight3D
var rover: Node3D
var rover_body: Node3D
var rover_cell := Vector2i.ZERO
var rover_start := Vector2i.ZERO
var dust_trail: GPUParticles3D
var ices := {}
var ice_materials: Array[ShaderMaterial] = []
var blinkers: Array = []
var fx_nodes: Array[Node3D] = []  # efekt kapalıyken gizlenen şeyler
var devil: Node3D
var devil_parts: Array[Node3D] = []
var ice_count := 0
var time := 0.0
var shake := 0.0
var rng := RandomNumberGenerator.new()

var cinema_mat: ShaderMaterial
var hud: Control
var ice_label: Label
var chip_bar: ColorRect
var mission_bar: ColorRect
var line_bar: Control
var line_numbers: Array[Label] = []
var fx_button: Button
var caption: Control
var start_screen: Control
var start_switch_knob: Panel
var start_switch_bg: StyleBoxFlat
var start_switch_label: Label

var noise_wall := FastNoiseLite.new()
var noise_dunes := FastNoiseLite.new()
var noise_detail := FastNoiseLite.new()
var noise_rock := FastNoiseLite.new()

var font_heading: FontFile
var font_label: FontFile
var font_body: FontFile
var font_code: FontFile

var m_hull: StandardMaterial3D
var m_hull_dark: StandardMaterial3D
var m_gold: StandardMaterial3D
var m_window: StandardMaterial3D
var m_orange: StandardMaterial3D
var m_rock: StandardMaterial3D
var rock_meshes: Array[ArrayMesh] = []
var _soft_tex: GradientTexture2D


func _ready() -> void:
	rng.seed = 7
	font_heading = load("res://fonts/ChakraPetch_700Bold.ttf")
	font_label = load("res://fonts/ChakraPetch_600SemiBold.ttf")
	font_body = load("res://fonts/ChakraPetch_500Medium.ttf")
	font_code = load("res://fonts/JetBrainsMono_400Regular.ttf")
	noise_wall.seed = 3
	noise_wall.frequency = 0.045
	noise_wall.fractal_octaves = 4
	noise_dunes.seed = 11
	noise_dunes.frequency = 0.06
	noise_detail.seed = 23
	noise_detail.frequency = 0.35
	noise_rock.seed = 77
	noise_rock.frequency = 1.4

	world = Node3D.new()
	add_child(world)
	_make_materials()
	_build_environment()
	_build_lights()
	_build_camera()
	_build_terrain()
	_build_zone()
	_build_map()
	_build_scenery()
	_build_boulders()
	_build_moons()
	_build_dust_devil()
	_build_ambient_dust()
	_build_cinema_layer()
	_build_hud()

	var shots := _shot_args()
	if shots.is_empty() or OS.get_cmdline_user_args().has("--giris"):
		_build_start_screen()
	else:
		_set_anim(not OS.get_cmdline_user_args().has("--anim=off"))
		_begin(OS.get_cmdline_user_args().has("--intro"))
	_setup_shots(shots)
	_meteor_loop()
	_run_demo()


func _process(delta: float) -> void:
	time += delta
	for i in ice_materials.size():
		ice_materials[i].set_shader_parameter("glow", 0.85 + 0.2 * sin(time * 1.6 + i * 1.3))
	for b in blinkers:
		var on := fmod(time + b[2], 1.8) < 0.2
		(b[0] as OmniLight3D).light_energy = 2.0 if on else 0.0
		(b[1] as StandardMaterial3D).emission_energy_multiplier = 6.0 if on else 0.3
	if rover_body and anim_on:
		rover_body.position.y = absf(sin(time * 5.0)) * 0.005
	if devil and devil.visible:
		for k in devil_parts.size():
			devil_parts[k].rotation.y += delta * (2.2 + k * 1.3)
		devil.position.x = -9.0 + sin(time * 0.05) * 9.0
		devil.position.z = -30.0 + cos(time * 0.08) * 3.0
		devil.position.y = _height(devil.position.x, devil.position.z)
	_update_camera(delta)


func _update_camera(delta: float) -> void:
	if not camera:
		return
	var pos := CAM_POS
	var target := CAM_TARGET
	if anim_on:
		# kamera nefes alır gibi çok hafif süzülür ve robotu yumuşakça izler
		var follow := Vector3((rover.position.x if rover else 0.0) * 0.22, 0, 0)
		pos += follow + Vector3(sin(time * 0.31) * 0.16, sin(time * 0.47) * 0.06, 0)
		target += follow * 1.2
		# buz toplanırken yaklaşma
		pos = pos.lerp(focus_point + Vector3(0.6, 1.9, 3.2), focus_amt * 0.5)
		target = target.lerp(focus_point, focus_amt * 0.6)
		# giriş: kamera kanyona süzülerek iner
		if intro_t < 1.0:
			var e := intro_t * intro_t * (3.0 - 2.0 * intro_t)
			var mid := Vector3(1.5, 8.0, 12.0)
			var a := INTRO_POS.lerp(mid, e)
			var b := mid.lerp(pos, e)
			pos = a.lerp(b, e)
			target = INTRO_TARGET.lerp(target, e)
		var jitter := Vector3(randf_range(-1, 1), randf_range(-1, 1), 0) * shake
		pos += jitter
		target += jitter * 0.4
		shake = move_toward(shake, 0.0, delta * 0.3)
	camera.position = pos
	camera.look_at(target)
	if cam_attrs:
		cam_attrs.dof_blur_far_distance = pos.distance_to(target) + 7.0


# --- efekt anahtarı ---

func _set_anim(on: bool) -> void:
	anim_on = on
	for n in fx_nodes:
		if is_instance_valid(n):
			n.visible = on
			if n is GPUParticles3D:
				(n as GPUParticles3D).emitting = on
	if devil:
		devil.visible = on
	if cinema_mat:
		cinema_mat.set_shader_parameter("grain", 0.045 if on else 0.0)
		cinema_mat.set_shader_parameter("aberration", 0.0018 if on else 0.0)
	if cam_attrs:
		cam_attrs.dof_blur_far_enabled = on
	if not on:
		shake = 0.0
		focus_amt = 0.0
		intro_t = 1.0
		if cinema_mat:
			cinema_mat.set_shader_parameter("bars", 0.0)
			cinema_mat.set_shader_parameter("fade", 0.0)
		if caption:
			caption.modulate.a = 0.0
		if hud and started:
			hud.modulate.a = 1.0
	if fx_button:
		fx_button.text = "✦  Efektler: " + ("Açık" if on else "Kapalı")
		fx_button.modulate = Color.WHITE if on else Color(1, 1, 1, 0.7)


func _begin(with_intro: bool) -> void:
	started = true
	if with_intro and anim_on:
		intro_t = 0.0
		hud.modulate.a = 0.0
		cinema_mat.set_shader_parameter("bars", 1.0)
		cinema_mat.set_shader_parameter("fade", 1.0)
		var tw := create_tween()
		tw.tween_method(func(v: float) -> void: cinema_mat.set_shader_parameter("fade", v), 1.0, 0.0, 1.2)
		var cam := create_tween()
		cam.tween_property(self, "intro_t", 1.0, 5.5).set_trans(Tween.TRANS_LINEAR)
		var cap := create_tween()
		cap.tween_interval(1.0)
		cap.tween_property(caption, "modulate:a", 1.0, 0.8)
		cap.tween_interval(2.2)
		cap.tween_property(caption, "modulate:a", 0.0, 0.8)
		var bars := create_tween()
		bars.tween_interval(4.6)
		bars.tween_method(func(v: float) -> void: cinema_mat.set_shader_parameter("bars", v), 1.0, 0.0, 1.0).set_trans(Tween.TRANS_CUBIC)
		bars.parallel().tween_property(hud, "modulate:a", 1.0, 1.0)
	else:
		intro_t = 1.0
		hud.modulate.a = 1.0


# --- malzemeler ---

func _std(color: Color, rough: float, metal := 0.0) -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.albedo_color = color
	m.roughness = rough
	m.metallic = metal
	return m


func _make_materials() -> void:
	m_hull = _std(Color(0.66, 0.64, 0.6), 0.55, 0.15)
	m_hull_dark = _std(Color(0.2, 0.21, 0.23), 0.5, 0.6)
	m_gold = _std(Color(0.85, 0.62, 0.28), 0.32, 1.0)
	m_orange = _std(Color(1.0, 0.45, 0.14), 0.5)
	m_window = _std(Color(1.0, 0.82, 0.58), 0.3)
	m_window.emission_enabled = true
	m_window.emission = Color(1.0, 0.76, 0.48)
	m_window.emission_energy_multiplier = 3.5
	m_rock = StandardMaterial3D.new()
	m_rock.albedo_texture = load("res://textures/Rock061_Color.jpg")
	m_rock.normal_enabled = true
	m_rock.normal_texture = load("res://textures/Rock061_NormalGL.jpg")
	m_rock.roughness_texture = load("res://textures/Rock061_Roughness.jpg")
	m_rock.albedo_color = Color(0.95, 0.56, 0.42)
	m_rock.uv1_triplanar = true
	m_rock.uv1_scale = Vector3(0.9, 0.9, 0.9)
	m_rock.texture_filter = BaseMaterial3D.TEXTURE_FILTER_LINEAR_WITH_MIPMAPS_ANISOTROPIC


# --- ortam ve ışık ---

func _build_environment() -> void:
	var env := Environment.new()
	env.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	var sky_mat := ShaderMaterial.new()
	sky_mat.shader = load("res://mars_twilight_sky.gdshader")
	sky_mat.set_shader_parameter("earth_dir", Vector3(0.3, 0.2, -0.93))
	sky.sky_material = sky_mat
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_energy = 1.05
	env.reflected_light_source = Environment.REFLECTION_SOURCE_SKY
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.tonemap_exposure = 1.0
	env.fog_enabled = true
	env.fog_light_color = Color("#9C7466")
	env.fog_light_energy = 0.7
	env.fog_density = 0.0045
	env.fog_sky_affect = 0.0
	env.fog_sun_scatter = 0.18
	# kanyon tabanında çöken alçak toz sisi
	env.fog_height = 0.6
	env.fog_height_density = 0.05
	env.glow_enabled = true
	env.glow_intensity = 0.8
	env.glow_bloom = 0.05
	env.glow_hdr_threshold = 1.1
	env.adjustment_enabled = true
	env.adjustment_saturation = 1.05
	env.adjustment_contrast = 1.12
	var we := WorldEnvironment.new()
	we.environment = env
	add_child(we)


func _build_lights() -> void:
	# kanyonun ucunda, ufka çok yakın batan güneş: sahne arkadan aydınlanır, gölgeler kameraya doğru uzar
	sun = DirectionalLight3D.new()
	sun.light_color = Color("#FFE2C4")
	sun.light_energy = 2.4
	sun.shadow_enabled = true
	sun.shadow_blur = 1.2
	sun.directional_shadow_max_distance = 60.0
	add_child(sun)
	sun.look_at_from_position(Vector3(-22, 8, -60), Vector3.ZERO)
	# kameranın arkasından soğuk, zayıf dolgu (gökyüzü yansıması)
	var fill := DirectionalLight3D.new()
	fill.light_color = Color("#E6D8FF")
	fill.light_energy = 0.85
	add_child(fill)
	fill.look_at_from_position(Vector3(4, 7, 12), Vector3.ZERO)


func _build_camera() -> void:
	camera = Camera3D.new()
	camera.keep_aspect = Camera3D.KEEP_WIDTH
	camera.fov = 60.0
	camera.v_offset = -0.7
	camera.far = 500.0
	cam_attrs = CameraAttributesPractical.new()
	cam_attrs.dof_blur_far_enabled = true
	cam_attrs.dof_blur_far_distance = 16.0
	cam_attrs.dof_blur_far_transition = 35.0
	cam_attrs.dof_blur_amount = 0.06
	camera.attributes = cam_attrs
	add_child(camera)
	camera.position = CAM_POS
	camera.look_at(CAM_TARGET)


# --- kanyon ---

func _height(x: float, z: float) -> float:
	# kanyon uzaklaştıkça genişler; duvarlar dalgalı ve basamaklı
	var width := 9.0 + maxf(0.0, -z - 15.0) * 0.16
	var ax := absf(x) + noise_wall.get_noise_2d(x, z) * 3.0
	var t := smoothstep(width, width + 9.0, ax)
	var rim := 30.0 + noise_wall.get_noise_2d(x * 0.5 + 40.0, z * 0.5) * 9.0
	var raw := t * rim
	var step_h := 3.6
	var k := raw / step_h
	var terraced := (floorf(k) + smoothstep(0.55, 1.0, fposmod(k, 1.0))) * step_h
	var h := lerpf(raw, terraced, 0.75)
	# taban: kod bölgesi düz, çevresinde hafif kumul dalgaları
	var d := maxf(absf(x), absf(z))
	h += smoothstep(3.6, 6.5, d) * (noise_dunes.get_noise_2d(x, z) * 0.45 + noise_detail.get_noise_2d(x, z) * 0.08)
	# kanyonun ucunda, ufukta uzak yaylalar
	h += smoothstep(-100.0, -170.0, z) * (14.0 + noise_wall.get_noise_2d(x * 0.3, 7.0) * 10.0)
	# kameranın arkası da kapalı olsun
	h += smoothstep(14.0, 30.0, z) * 12.0
	return h


func _build_terrain() -> void:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var n := 220
	var coords := PackedFloat32Array()
	for i in n + 1:
		var u := i / float(n) * 2.0 - 1.0
		coords.append(signf(u) * (absf(u) * 34.0 + pow(absf(u), 4.0) * 170.0))
	for iz in n + 1:
		for ix in n + 1:
			var x := coords[ix]
			var z := coords[iz]
			st.set_uv(Vector2(x, z) * 0.4)
			st.add_vertex(Vector3(x, _height(x, z), z))
	for iz in n:
		for ix in n:
			var v00 := iz * (n + 1) + ix
			var v10 := v00 + 1
			var v01 := v00 + (n + 1)
			var v11 := v01 + 1
			st.add_index(v00)
			st.add_index(v10)
			st.add_index(v01)
			st.add_index(v10)
			st.add_index(v11)
			st.add_index(v01)
	st.generate_normals()
	st.generate_tangents()
	var mi := MeshInstance3D.new()
	mi.mesh = st.commit()
	var mat := ShaderMaterial.new()
	mat.shader = load("res://canyon.gdshader")
	mat.set_shader_parameter("ground_albedo", load("res://textures/Ground079S_Color.jpg"))
	mat.set_shader_parameter("ground_normal", load("res://textures/Ground079S_NormalGL.jpg"))
	mat.set_shader_parameter("ground_rough", load("res://textures/Ground079S_Roughness.jpg"))
	mat.set_shader_parameter("dune_albedo", load("res://textures/Ground093C_Color.jpg"))
	mat.set_shader_parameter("dune_normal", load("res://textures/Ground093C_NormalGL.jpg"))
	mat.set_shader_parameter("rock_albedo", load("res://textures/Rock062_Color.jpg"))
	mat.set_shader_parameter("rock_normal", load("res://textures/Rock062_NormalGL.jpg"))
	mi.material_override = mat
	world.add_child(mi)


# --- kod bölgesi ---

func _build_zone() -> void:
	var grid := MeshInstance3D.new()
	var plane := PlaneMesh.new()
	plane.size = Vector2(COLS, ROWS)
	grid.mesh = plane
	var gmat := ShaderMaterial.new()
	gmat.shader = load("res://hologrid.gdshader")
	gmat.set_shader_parameter("color", Color(0.45, 0.85, 1.0))
	grid.material_override = gmat
	grid.position.y = 0.015
	grid.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	world.add_child(grid)
	# köşelerde alçak işaret kazıkları
	var hx := COLS / 2.0 + 0.3
	var hz := ROWS / 2.0 + 0.3
	var phase := 0.0
	for c in [Vector3(-hx, 0, -hz), Vector3(hx, 0, -hz), Vector3(hx, 0, hz), Vector3(-hx, 0, hz)]:
		_mesh(_cylinder(0.03, 0.04, 0.5, 8), m_hull_dark, c + Vector3(0, 0.25, 0))
		_add_blinker(c + Vector3(0, 0.54, 0), AMBER, phase)
		phase += 0.45


func _build_map() -> void:
	for gy in ROWS:
		var row: String = MAP[gy]
		for gx in COLS:
			var cell := Vector2i(gx, gy)
			var pos := _cell_pos(cell)
			match row[gx]:
				"I":
					ices[cell] = _ice_deposit(pos, gx * 31 + gy * 7)
				"R":
					_boulder(pos, rng.randf_range(0.5, 0.65), rng.randf() * TAU)
				"@":
					rover_start = cell
					_build_rover()
				"D":
					_cargo_pod(pos)


# --- buz yatağı: yere gömülü, tozlu, doğal buz parçaları ---

func _ice_deposit(pos: Vector3, seed_value: int) -> Node3D:
	if rock_meshes.is_empty():
		_build_rock_meshes()
	var r := RandomNumberGenerator.new()
	r.seed = seed_value
	var node := Node3D.new()
	node.position = pos
	world.add_child(node)
	var mat := ShaderMaterial.new()
	mat.shader = load("res://ice.gdshader")
	ice_materials.append(mat)
	# çevresinde soluk kırağı lekesi
	var frost := MeshInstance3D.new()
	var disk := CylinderMesh.new()
	disk.top_radius = 0.4
	disk.bottom_radius = 0.44
	disk.height = 0.01
	frost.mesh = disk
	var fmat := _std(Color(0.7, 0.62, 0.58, 0.3), 0.85)
	fmat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	frost.material_override = fmat
	frost.position.y = 0.006
	node.add_child(frost)
	var count := r.randi_range(4, 6)
	for k in count:
		var chunk := MeshInstance3D.new()
		chunk.mesh = rock_meshes[r.randi() % rock_meshes.size()]
		chunk.material_override = mat
		var s := r.randf_range(0.2, 0.34) * (1.25 if k == 0 else 1.0)
		chunk.scale = Vector3(s, s * r.randf_range(0.9, 1.4), s)
		var ang := r.randf() * TAU
		var spread := 0.0 if k == 0 else r.randf_range(0.12, 0.24)
		chunk.position = Vector3(cos(ang) * spread, -s * 0.12, sin(ang) * spread)
		chunk.rotation = Vector3(r.randf_range(-0.4, 0.4), r.randf() * TAU, r.randf_range(-0.4, 0.4))
		node.add_child(chunk)
	var glow := OmniLight3D.new()
	glow.light_color = Color("#8FDFFF")
	glow.light_energy = 0.35
	glow.omni_range = 1.1
	glow.position = Vector3(0, 0.3, 0)
	node.add_child(glow)
	return node


# --- kayalar ---

func _build_rock_meshes() -> void:
	for v in 5:
		var sphere := SphereMesh.new()
		sphere.radius = 0.5
		sphere.height = 1.0
		sphere.radial_segments = 20
		sphere.rings = 12
		var arrays := sphere.get_mesh_arrays()
		var verts: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
		for i in verts.size():
			var p := verts[i]
			var nrm := p.normalized()
			var n1 := noise_rock.get_noise_3dv(p * 1.6 + Vector3(v * 7.0, 0, 0))
			var n2 := noise_rock.get_noise_3dv(p * 4.0 + Vector3(0, v * 3.0, 0))
			p = nrm * 0.5 * (1.0 + n1 * 0.45 + n2 * 0.12)
			p.y = maxf(p.y, -0.12) * (0.65 + v * 0.06)
			verts[i] = p
		arrays[Mesh.ARRAY_VERTEX] = verts
		var am := ArrayMesh.new()
		am.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
		var st := SurfaceTool.new()
		st.create_from(am, 0)
		st.generate_normals()
		rock_meshes.append(st.commit())


func _boulder(pos: Vector3, size: float, rot: float) -> MeshInstance3D:
	if rock_meshes.is_empty():
		_build_rock_meshes()
	var mi := MeshInstance3D.new()
	mi.mesh = rock_meshes[rng.randi() % rock_meshes.size()]
	mi.material_override = m_rock
	mi.scale = Vector3(size, size * rng.randf_range(0.7, 1.1), size * rng.randf_range(0.8, 1.2))
	mi.rotation.y = rot
	mi.position = pos + Vector3(0, size * 0.04, 0)
	world.add_child(mi)
	return mi


func _build_boulders() -> void:
	# duvar diplerinde kopup düşmüş kaya yığınları + tabanda dağınık taşlar
	var placed := 0
	while placed < 170:
		var x := rng.randf_range(-20.0, 20.0)
		var z := rng.randf_range(-45.0, 1.0)
		if maxf(absf(x), absf(z)) < 4.2:
			continue
		var h := _height(x, z)
		if h > 8.0:
			continue
		var size := rng.randf_range(0.08, 0.35)
		if h > 0.6 or rng.randf() < 0.06:
			size = rng.randf_range(0.5, 1.8)
		_boulder(Vector3(x, h - size * 0.15, z), size, rng.randf() * TAU)
		placed += 1


# --- iniş aracı, uzak üs, kargo ---

func _build_scenery() -> void:
	# altın folyolu iniş aracı (sol arkada)
	var base := Vector3(-5.4, 0, -6.2)
	base.y = _height(base.x, base.z)
	_mesh(_cylinder(0.9, 1.0, 0.7, 8), m_gold, base + Vector3(0, 1.05, 0))
	_mesh(_cylinder(0.7, 0.9, 0.35, 8), m_hull, base + Vector3(0, 1.58, 0))
	for k in 4:
		var a := k * TAU / 4.0 + 0.4
		var leg := _mesh(_cylinder(0.035, 0.035, 1.3, 6), m_hull_dark, base + Vector3(cos(a) * 0.95, 0.55, sin(a) * 0.95))
		leg.rotation = Vector3(sin(a) * 0.35, 0, -cos(a) * 0.35)
		_mesh(_cylinder(0.14, 0.16, 0.05, 10), m_hull_dark, base + Vector3(cos(a) * 1.2, 0.03, sin(a) * 1.2))
	var mast := _mesh(_cylinder(0.02, 0.02, 0.9, 6), m_hull_dark, base + Vector3(0.3, 2.2, 0))
	mast.rotation.z = 0.1
	var dish := MeshInstance3D.new()
	var bs := SphereMesh.new()
	bs.radius = 0.32
	bs.height = 0.2
	bs.is_hemisphere = true
	dish.mesh = bs
	dish.material_override = m_hull
	dish.position = base + Vector3(0.36, 2.65, 0)
	dish.rotation = Vector3(-2.0, 0.6, 0)
	world.add_child(dish)
	_add_blinker(base + Vector3(-0.4, 1.85, 0.3), Color("#FF3B30"), 0.3)
	# kanyonun derinliğinde ışıkları yanan küçük üs (hayat belirtisi, ölçek hissi)
	var hab := Vector3(4.0, 0, -52.0)
	hab.y = _height(hab.x, hab.z)
	var dome := MeshInstance3D.new()
	var ds := SphereMesh.new()
	ds.radius = 2.2
	ds.height = 2.2
	ds.is_hemisphere = true
	dome.mesh = ds
	dome.material_override = m_hull
	dome.position = hab
	world.add_child(dome)
	for k in 7:
		var a := -0.5 + k * 0.25
		var w := _mesh(_box(0.4, 0.2, 0.05), m_window, hab + Vector3(sin(a) * 2.05, 0.5, cos(a) * 2.05))
		w.rotation.y = a
	var hl := OmniLight3D.new()
	hl.light_color = Color("#FFC68A")
	hl.light_energy = 3.0
	hl.omni_range = 7.0
	hl.position = hab + Vector3(0, 1.0, 3.0)
	world.add_child(hl)
	_add_blinker(hab + Vector3(0, 2.4, 0), Color("#FF3B30"), 1.1)


func _cargo_pod(pos: Vector3) -> void:
	_mesh(_box(0.74, 0.44, 0.58), m_hull, pos + Vector3(0, 0.22, 0))
	_mesh(_box(0.76, 0.06, 0.6), m_orange, pos + Vector3(0, 0.38, 0))
	for k in 3:
		_mesh(_box(0.03, 0.45, 0.6), m_hull_dark, pos + Vector3(-0.28 + k * 0.28, 0.22, 0))
	_add_blinker(pos + Vector3(0.28, 0.5, 0.2), Color("#5FE0FF"), 0.3)


# --- Phobos ve Deimos ---

func _build_moons() -> void:
	var mat := StandardMaterial3D.new()
	mat.albedo_texture = load("res://textures/Rock062_Color.jpg")
	mat.normal_enabled = true
	mat.normal_texture = load("res://textures/Rock062_NormalGL.jpg")
	mat.albedo_color = Color(0.6, 0.55, 0.52)
	mat.uv1_triplanar = true
	mat.uv1_scale = Vector3(0.6, 0.6, 0.6)
	mat.disable_fog = true
	# uydular güneşin aydınlattığı tarafıyla görünsün
	mat.emission_enabled = true
	mat.emission_texture = mat.albedo_texture
	mat.emission = Color(0.75, 0.66, 0.6)
	mat.emission_energy_multiplier = 0.3
	for spec in [[Vector3(0.2, 0.36, -0.91) * 200.0, 5.5, 1.5], [Vector3(-0.3, 0.52, -0.8) * 180.0, 3.2, 1.1]]:
		var sphere := SphereMesh.new()
		sphere.radius = 1.0
		sphere.height = 2.0
		sphere.radial_segments = 32
		sphere.rings = 16
		var arrays := sphere.get_mesh_arrays()
		var verts: PackedVector3Array = arrays[Mesh.ARRAY_VERTEX]
		for i in verts.size():
			var p := verts[i]
			var bump := noise_rock.get_noise_3dv(p * 1.3) * 0.22 + noise_rock.get_noise_3dv(p * 3.5) * 0.06
			p = p * (1.0 + bump)
			p.x *= spec[2]
			verts[i] = p
		arrays[Mesh.ARRAY_VERTEX] = verts
		var am := ArrayMesh.new()
		am.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
		var st := SurfaceTool.new()
		st.create_from(am, 0)
		st.generate_normals()
		var moon := MeshInstance3D.new()
		moon.mesh = st.commit()
		moon.material_override = mat
		moon.scale = Vector3.ONE * spec[1]
		moon.position = spec[0]
		moon.rotation = Vector3(0.4, 1.2, 0.3)
		moon.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		world.add_child(moon)


# --- toz hortumu ---

func _build_dust_devil() -> void:
	devil = Node3D.new()
	world.add_child(devil)
	var shader: Shader = load("res://dust_devil.gdshader")
	for k in 2:
		var part := Node3D.new()
		var mi := MeshInstance3D.new()
		var cyl := CylinderMesh.new()
		cyl.top_radius = 1.9 - k * 0.5
		cyl.bottom_radius = 0.3 - k * 0.1
		cyl.height = 11.0 - k * 2.0
		cyl.cap_top = false
		cyl.cap_bottom = false
		cyl.radial_segments = 24
		cyl.rings = 10
		mi.mesh = cyl
		var mat := ShaderMaterial.new()
		mat.shader = shader
		mat.set_shader_parameter("strength", 0.42 - k * 0.1)
		mat.set_shader_parameter("speed", 1.0 + k * 0.6)
		mi.material_override = mat
		mi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		mi.position.y = cyl.height / 2.0
		part.add_child(mi)
		devil.add_child(part)
		devil_parts.append(part)
	var base := _particles(50, 2.5, Vector3.UP, 70.0, 0.4, 1.0, Vector3(0.2, 0.2, 0), 1.0, 2.5,
		[Color(0.66, 0.44, 0.32, 0.0), Color(0.66, 0.44, 0.32, 0.35), Color(0.66, 0.44, 0.32, 0.0)], 0.6, 0.0)
	base.preprocess = 2.5
	devil.add_child(base)


# --- meteor: gökyüzünde yanarak ilerler, kanyonun ucuna düşer ---

func _meteor_loop() -> void:
	await _wait(3.4)
	while true:
		if anim_on and started:
			await _fireball()
		await _wait(rng.randf_range(8.0, 12.0))


func _fireball() -> void:
	var target := Vector3(rng.randf_range(-9.0, 9.0), 0, rng.randf_range(-85.0, -65.0))
	target.y = _height(target.x, target.z)
	var side := -1.0 if target.x > 0 else 1.0
	var start := target + Vector3(side * 40.0, 55.0, -20.0)
	var head := Node3D.new()
	head.position = start
	world.add_child(head)
	var core := MeshInstance3D.new()
	var q := QuadMesh.new()
	q.size = Vector2(1.6, 1.6)
	core.mesh = q
	var cm := StandardMaterial3D.new()
	cm.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	cm.billboard_mode = BaseMaterial3D.BILLBOARD_ENABLED
	cm.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
	cm.albedo_texture = _soft_dot()
	cm.albedo_color = Color(5.0, 4.2, 3.2)
	cm.disable_fog = true
	core.material_override = cm
	head.add_child(core)
	var light := OmniLight3D.new()
	light.light_color = Color("#FFB070")
	light.light_energy = 6.0
	light.omni_range = 40.0
	head.add_child(light)
	var fire := _particles(420, 0.9, Vector3.ZERO, 180.0, 0.1, 0.5, Vector3.ZERO, 0.7, 1.6,
		[Color(1, 0.92, 0.75, 1), Color(1, 0.45, 0.12, 0.6), Color(0.5, 0.1, 0.02, 0.0)], 0.7, 2.5)
	head.add_child(fire)
	fire.emitting = true
	var tw := create_tween()
	tw.tween_property(head, "position", target, 2.2).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	await tw.finished
	fire.emitting = false
	core.visible = false
	light.visible = false
	if anim_on:
		_impact(target)
	get_tree().create_timer(3.0).timeout.connect(head.queue_free)


func _impact(p: Vector3) -> void:
	var flash := OmniLight3D.new()
	flash.light_color = Color("#FFC080")
	flash.light_energy = 60.0
	flash.omni_range = 70.0
	flash.position = p + Vector3(0, 4, 0)
	world.add_child(flash)
	var ft := create_tween()
	ft.tween_property(flash, "light_energy", 0.0, 1.6).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	ft.tween_callback(flash.queue_free)
	var ball := MeshInstance3D.new()
	var q := QuadMesh.new()
	q.size = Vector2(9, 9)
	ball.mesh = q
	var bm := StandardMaterial3D.new()
	bm.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	bm.billboard_mode = BaseMaterial3D.BILLBOARD_ENABLED
	bm.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
	bm.albedo_texture = _soft_dot()
	bm.albedo_color = Color(4.0, 2.2, 0.8)
	bm.disable_fog = true
	ball.material_override = bm
	ball.position = p + Vector3(0, 2.5, 0)
	ball.scale = Vector3.ONE * 0.2
	world.add_child(ball)
	var bt := create_tween().set_parallel(true)
	bt.tween_property(ball, "scale", Vector3.ONE * 1.6, 0.6).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	bt.tween_property(bm, "albedo_color", Color(0, 0, 0), 1.2).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	bt.chain().tween_callback(ball.queue_free)
	var ring := MeshInstance3D.new()
	var t := TorusMesh.new()
	t.inner_radius = 0.9
	t.outer_radius = 1.0
	ring.mesh = t
	var rm := StandardMaterial3D.new()
	rm.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	rm.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	rm.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
	rm.albedo_color = Color(1.0, 0.75, 0.5, 0.7)
	rm.disable_fog = true
	ring.material_override = rm
	ring.position = p + Vector3(0, 0.5, 0)
	ring.scale = Vector3(1, 0.2, 1)
	world.add_child(ring)
	var rt := create_tween().set_parallel(true)
	rt.tween_property(ring, "scale", Vector3(22, 0.6, 22), 1.8).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	rt.tween_property(rm, "albedo_color:a", 0.0, 1.8)
	rt.chain().tween_callback(ring.queue_free)
	var plume := _particles(70, 9.0, Vector3.UP, 30.0, 2.0, 5.0, Vector3(0.5, -0.3, 0), 1.0, 2.2,
		[Color(0.72, 0.5, 0.38, 0.0), Color(0.68, 0.47, 0.36, 0.55), Color(0.6, 0.42, 0.33, 0.0)], 3.0, 0.0)
	plume.one_shot = true
	plume.explosiveness = 0.75
	plume.position = p + Vector3(0, 1.0, 0)
	world.add_child(plume)
	plume.emitting = true
	get_tree().create_timer(11.0).timeout.connect(plume.queue_free)
	# ses gecikmesi gibi: yarım saniye sonra hafif sarsıntı
	get_tree().create_timer(0.6).timeout.connect(func() -> void:
		if anim_on:
			shake = 0.05)


# --- robot (NASA Perseverance) ---

func _build_rover() -> void:
	rover = Node3D.new()
	world.add_child(rover)
	rover_body = Node3D.new()
	rover.add_child(rover_body)
	_spawn_glb("res://models/nasa/perseverance.glb", 1.45, rover_body)
	var head := SpotLight3D.new()
	head.light_color = Color("#FFF2C8")
	head.light_energy = 3.0
	head.spot_range = 3.5
	head.spot_angle = 32.0
	head.position = Vector3(0.4, 0.5, 0)
	head.rotation_degrees = Vector3(-18, -90, 0)
	rover_body.add_child(head)
	_add_blinker(Vector3(-0.2, 0.8, 0.15), Color("#5FE0FF"), 0.5, rover_body)
	dust_trail = _particles(60, 1.8, Vector3.UP, 45.0, 0.25, 0.6, Vector3(0.15, -0.2, 0), 1.5, 3.2,
		[Color(0.7, 0.46, 0.34, 0.5), Color(0.7, 0.46, 0.34, 0.0)], 0.06, 0.0)
	dust_trail.emitting = false
	dust_trail.position = Vector3(-0.4, 0.05, 0)
	rover.add_child(dust_trail)
	_place_rover(rover_start)


func _place_rover(cell: Vector2i) -> void:
	rover_cell = cell
	rover.position = _cell_pos(cell)
	rover.rotation.y = 0.0


func _move_rover(target: Vector2i) -> void:
	if anim_on:
		await _show_path(rover_cell, target)
	var tw := create_tween()
	tw.tween_property(rover, "position", _cell_pos(target), 1.0 if anim_on else 0.45).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_IN_OUT)
	dust_trail.emitting = anim_on
	await tw.finished
	dust_trail.emitting = false
	rover_cell = target


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
		mat.albedo_color = Color(0.5, 0.9, 1.0, 0.0)
		mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
		mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
		chevron.material_override = mat
		chevron.position = a.lerp(b, (k + 1) / 4.0) + Vector3(0, 0.03, 0)
		chevron.rotation = Vector3(-PI / 2.0, yaw - PI / 2.0, 0)
		chevron.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		world.add_child(chevron)
		var tw := create_tween()
		tw.tween_interval(k * 0.08)
		tw.tween_property(mat, "albedo_color:a", 0.9, 0.12)
		tw.tween_interval(0.45)
		tw.tween_property(mat, "albedo_color:a", 0.0, 0.3)
		tw.tween_callback(chevron.queue_free)
	await _wait(0.3)


func _collect(cell: Vector2i) -> void:
	var ice: Node3D = ices[cell]
	var pos := ice.position
	if not anim_on:
		ice.visible = false
		_count_ice()
		return
	# sinema anı: kamera buza yaklaşır
	focus_point = pos
	create_tween().tween_property(self, "focus_amt", 1.0, 0.7).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	await _wait(0.5)
	_beam(rover.position + Vector3(0, 0.85, 0), pos + Vector3(0, 0.2, 0))
	await _wait(0.3)
	var tw := create_tween()
	tw.tween_property(ice, "scale", Vector3.ONE * 1.08, 0.12).set_trans(Tween.TRANS_SINE)
	tw.tween_property(ice, "scale", Vector3(1.0, 0.001, 1.0), 0.6).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	# ince Mars havasında buz erimeden doğrudan buhara döner
	var vapor := _particles(40, 3.0, Vector3.UP, 25.0, 0.25, 0.55, Vector3(0.2, 0.18, 0), 1.0, 2.6,
		[Color(0.88, 0.95, 1.0, 0.0), Color(0.88, 0.95, 1.0, 0.22), Color(0.88, 0.95, 1.0, 0.0)], 0.14, 0.0)
	vapor.one_shot = true
	vapor.explosiveness = 0.35
	vapor.position = pos + Vector3(0, 0.15, 0)
	world.add_child(vapor)
	vapor.emitting = true
	var glints := _particles(24, 1.2, Vector3.UP, 60.0, 0.6, 1.4, Vector3(0, -2.0, 0), 0.5, 1.0,
		[Color(0.8, 0.95, 1.0, 1.0), Color(0.6, 0.9, 1.0, 0.0)], 0.03, 2.0)
	glints.one_shot = true
	glints.explosiveness = 0.9
	glints.position = pos + Vector3(0, 0.25, 0)
	world.add_child(glints)
	glints.emitting = true
	get_tree().create_timer(4.0).timeout.connect(vapor.queue_free)
	get_tree().create_timer(4.0).timeout.connect(glints.queue_free)
	_float_text("+1 buz", pos + Vector3(0, 0.9, 0))
	_fly_icon(pos + Vector3(0, 0.3, 0))
	await tw.finished
	ice.visible = false
	await _wait(0.3)
	create_tween().tween_property(self, "focus_amt", 0.0, 0.9).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)


func _beam(from: Vector3, to: Vector3) -> void:
	var beam := MeshInstance3D.new()
	var cyl := CylinderMesh.new()
	cyl.top_radius = 0.012
	cyl.bottom_radius = 0.03
	cyl.height = maxf(from.distance_to(to), 0.1)
	beam.mesh = cyl
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.6, 0.95, 1.0, 0.0)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	beam.material_override = mat
	beam.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	world.add_child(beam)
	beam.position = (from + to) / 2.0
	if absf((to - from).normalized().dot(Vector3.UP)) < 0.99:
		beam.look_at(to, Vector3.UP)
		beam.rotate_object_local(Vector3.RIGHT, PI / 2.0)
	var tw := create_tween()
	tw.tween_property(mat, "albedo_color:a", 0.7, 0.1)
	tw.tween_interval(0.5)
	tw.tween_property(mat, "albedo_color:a", 0.0, 0.3)
	tw.tween_callback(beam.queue_free)


func _float_text(text: String, pos: Vector3) -> void:
	var label := Label3D.new()
	label.text = text
	label.font = font_heading
	label.font_size = 64
	label.pixel_size = 0.006
	label.modulate = Color("#FFF1DC")
	label.outline_modulate = Color(0.05, 0.03, 0.02)
	label.outline_size = 12
	label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	label.no_depth_test = true
	label.position = pos
	world.add_child(label)
	var tw := create_tween().set_parallel(true)
	tw.tween_property(label, "position", pos + Vector3(0, 0.6, 0), 1.3).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tw.tween_property(label, "modulate:a", 0.0, 0.9).set_delay(0.4)
	tw.chain().tween_callback(label.queue_free)


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
		icon.scale = Vector2.ONE * lerpf(1.4, 0.9, t), 0.0, 1.0, 0.8).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	await tw.finished
	icon.queue_free()
	_count_ice()


func _count_ice() -> void:
	ice_count += 1
	ice_label.text = "%d/3" % ice_count
	if anim_on:
		var pop := create_tween()
		pop.tween_property(ice_label, "scale", Vector2(1.25, 1.25), 0.1)
		pop.tween_property(ice_label, "scale", Vector2.ONE, 0.2)
	var fill := create_tween().set_parallel(true)
	fill.tween_property(chip_bar, "size:x", 150.0 * ice_count / 3.0, 0.35 if anim_on else 0.01)
	fill.tween_property(mission_bar, "size:x", MISSION_W * ice_count / 3.0, 0.35 if anim_on else 0.01)


func _run_demo() -> void:
	while not started:
		await _wait(0.2)
	if intro_t < 1.0:
		await _wait(5.0)
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
		ice.scale = Vector3.ONE
	ice_count = 0
	ice_label.text = "0/3"
	chip_bar.size.x = 0.0
	mission_bar.size.x = 0.0
	_highlight(-1)


func _wait(seconds: float) -> void:
	await get_tree().create_timer(seconds).timeout


# --- sinema katmanı ---

func _build_cinema_layer() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 0
	add_child(layer)
	var rect := ColorRect.new()
	rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	cinema_mat = ShaderMaterial.new()
	cinema_mat.shader = load("res://cinema.gdshader")
	rect.material = cinema_mat
	layer.add_child(rect)
	var cap_layer := CanvasLayer.new()
	cap_layer.layer = 2
	add_child(cap_layer)
	caption = Control.new()
	caption.set_anchors_preset(Control.PRESET_FULL_RECT)
	caption.mouse_filter = Control.MOUSE_FILTER_IGNORE
	caption.modulate.a = 0.0
	cap_layer.add_child(caption)
	var l1 := _label("MELAS KANYONU", font_heading, 64, Color("#F4E9DD"), Vector2(0, 2040), caption)
	l1.size = Vector2(1080, 90)
	l1.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	var l2 := _label("SEKTÖR 7  ·  BUZ YATAKLARI", font_label, 30, AMBER, Vector2(0, 2130), caption)
	l2.size = Vector2(1080, 50)
	l2.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER


# --- arayüz ---

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 1
	add_child(layer)
	hud = Control.new()
	hud.set_anchors_preset(Control.PRESET_FULL_RECT)
	hud.mouse_filter = Control.MOUSE_FILTER_IGNORE
	layer.add_child(hud)

	# üst kısım: çerçevesiz, sahnenin üstünde duran yazılar
	var top := TextureRect.new()
	var g := Gradient.new()
	g.set_color(0, Color(0, 0, 0, 0.55))
	g.set_color(1, Color(0, 0, 0, 0.0))
	var gt := GradientTexture2D.new()
	gt.gradient = g
	gt.fill_from = Vector2(0, 0)
	gt.fill_to = Vector2(0, 1)
	top.texture = gt
	top.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	top.stretch_mode = TextureRect.STRETCH_SCALE
	top.mouse_filter = Control.MOUSE_FILTER_IGNORE
	top.size = Vector2(get_viewport().get_visible_rect().size.x, 420)
	hud.add_child(top)
	_label("SEKTÖR 7 · MELAS KANYONU", font_label, 28, AMBER, Vector2(64, 92))
	var title := _label("Buz Avı", font_heading, 88, Color("#FFF6EC"), Vector2(58, 124))
	title.add_theme_color_override("font_shadow_color", Color(0, 0, 0, 0.6))
	title.add_theme_constant_override("shadow_offset_y", 4)
	_label("Görev: Doğudaki 3 buzu topla", font_body, 36, Color("#EADDCF"), Vector2(64, 240))
	_bar(Rect2(64, 300, MISSION_W, 6), Color(1, 1, 1, 0.15))
	mission_bar = _bar(Rect2(64, 300, 0, 6), AMBER)

	_glass(Rect2(740, 92, 280, 120), 60, Color(0.02, 0.02, 0.03, 0.55), Color(1, 1, 1, 0.14))
	var icon := _crystal_icon(0.8)
	icon.position = CHIP_ICON_POS
	hud.add_child(icon)
	ice_label = _label("0/3", font_heading, 56, Color("#FFF6EC"), Vector2(850, 114))
	ice_label.pivot_offset = Vector2(50, 36)
	_bar(Rect2(806, 188, 150, 5), Color(1, 1, 1, 0.14))
	chip_bar = _bar(Rect2(806, 188, 0, 5), Color("#8FDFFF"))
	fx_button = _pill_button(Rect2(740, 232, 280, 72), "✦  Efektler: Açık", 28, Color(0.02, 0.02, 0.03, 0.55), Color("#FFF6EC"), hud)
	fx_button.pressed.connect(func() -> void: _set_anim(not anim_on))

	# kod paneli
	_glass(Rect2(32, CODE_TOP, 1016, 596), 34, Color(0.012, 0.014, 0.024, 0.8), Color(1, 1, 1, 0.1))
	var tag := Panel.new()
	var tb := StyleBoxFlat.new()
	tb.bg_color = Color(1.0, 0.71, 0.28, 0.16)
	tb.set_corner_radius_all(16)
	tag.add_theme_stylebox_override("panel", tb)
	tag.position = Vector2(72, CODE_TOP + 30)
	tag.size = Vector2(150, 48)
	tag.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hud.add_child(tag)
	_label("PYTHON", font_label, 26, AMBER, Vector2(98, CODE_TOP + 36))
	_label("robot.py", font_code, 30, Color("#8C8A94"), Vector2(246, CODE_TOP + 36))
	var sep := ColorRect.new()
	sep.color = Color(1, 1, 1, 0.06)
	sep.position = Vector2(64, CODE_TOP + 104)
	sep.size = Vector2(952, 2)
	hud.add_child(sep)
	line_bar = Control.new()
	line_bar.position = Vector2(48, CODE_TOP + 124)
	line_bar.modulate.a = 0.0
	var glow := TextureRect.new()
	var lg := Gradient.new()
	lg.set_color(0, Color(1.0, 0.71, 0.28, 0.22))
	lg.set_color(1, Color(1.0, 0.71, 0.28, 0.0))
	var lgt := GradientTexture2D.new()
	lgt.gradient = lg
	lgt.fill_to = Vector2(1, 0)
	glow.texture = lgt
	glow.size = Vector2(984, LINE_H - 10)
	glow.stretch_mode = TextureRect.STRETCH_SCALE
	line_bar.add_child(glow)
	var accent := ColorRect.new()
	accent.color = AMBER
	accent.size = Vector2(5, LINE_H - 10)
	line_bar.add_child(accent)
	hud.add_child(line_bar)
	for i in CODE_LINES.size():
		var y := CODE_TOP + 124 + i * LINE_H
		line_numbers.append(_label(str(i + 1), font_code, 38, Color("#4A4854"), Vector2(84, y + 10)))
		var rt := RichTextLabel.new()
		rt.bbcode_enabled = true
		rt.fit_content = true
		rt.scroll_active = false
		rt.autowrap_mode = TextServer.AUTOWRAP_OFF
		rt.mouse_filter = Control.MOUSE_FILTER_IGNORE
		rt.add_theme_font_override("normal_font", font_code)
		rt.add_theme_font_size_override("normal_font_size", 42)
		rt.add_theme_color_override("default_color", Color("#ECE8F2"))
		rt.position = Vector2(150, y + 6)
		rt.size = Vector2(860, LINE_H)
		rt.text = CODE_LINES[i]
		hud.add_child(rt)

	var step := _pill_button(Rect2(32, 2128, 480, 140), "Adım adım", 46, Color(0.02, 0.02, 0.03, 0.6), Color("#FFF6EC"), hud)
	step.add_theme_font_override("font", font_heading)
	var run := _pill_button(Rect2(568, 2128, 480, 140), "▶  Çalıştır", 48, AMBER, Color("#1B1206"), hud)
	run.add_theme_font_override("font", font_heading)
	_highlight(-1)


func _highlight(line: int) -> void:
	if not line_bar:
		return
	for i in line_numbers.size():
		line_numbers[i].add_theme_color_override("font_color", AMBER if i == line else Color("#4A4854"))
	if line < 0:
		create_tween().tween_property(line_bar, "modulate:a", 0.0, 0.25)
		return
	var y := CODE_TOP + 124 + line * LINE_H - 4
	var tw := create_tween().set_parallel(true)
	tw.tween_property(line_bar, "modulate:a", 1.0, 0.15)
	tw.tween_property(line_bar, "position:y", y, 0.22 if anim_on else 0.01).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)


# --- giriş ekranı ---

func _build_start_screen() -> void:
	var layer := CanvasLayer.new()
	layer.layer = 5
	add_child(layer)
	start_screen = Control.new()
	start_screen.set_anchors_preset(Control.PRESET_FULL_RECT)
	layer.add_child(start_screen)
	var shade := TextureRect.new()
	var g := Gradient.new()
	g.set_color(0, Color(0.0, 0.0, 0.0, 0.25))
	g.set_color(1, Color(0.0, 0.0, 0.0, 0.88))
	var gt := GradientTexture2D.new()
	gt.gradient = g
	gt.fill_from = Vector2(0, 0.2)
	gt.fill_to = Vector2(0, 0.75)
	shade.texture = gt
	shade.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	shade.stretch_mode = TextureRect.STRETCH_SCALE
	shade.size = get_viewport().get_visible_rect().size
	start_screen.add_child(shade)
	hud.modulate.a = 0.0
	var t := _label("MarsKod", font_heading, 128, Color("#FFF6EC"), Vector2(0, 1245), start_screen)
	t.size = Vector2(1080, 170)
	t.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	var s := _label("Python yaz · robotunu yönet · Mars'ta hayatta kal", font_body, 36, Color("#E3D3C4"), Vector2(0, 1410), start_screen)
	s.size = Vector2(1080, 60)
	s.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER

	# efekt anahtarı kartı
	_glass(Rect2(90, 1500, 900, 190), 40, Color(0.03, 0.03, 0.045, 0.72), Color(1, 1, 1, 0.12), start_screen)
	_label("Animasyon ve efektler", font_label, 42, Color("#FFF6EC"), Vector2(140, 1536), start_screen)
	start_switch_label = _label("Açık: sinematik görünüm", font_body, 30, Color("#BBAE9F"), Vector2(140, 1600), start_screen)
	var sw := Panel.new()
	start_switch_bg = StyleBoxFlat.new()
	start_switch_bg.bg_color = AMBER
	start_switch_bg.set_corner_radius_all(40)
	sw.add_theme_stylebox_override("panel", start_switch_bg)
	sw.position = Vector2(790, 1555)
	sw.size = Vector2(150, 80)
	sw.mouse_filter = Control.MOUSE_FILTER_IGNORE
	start_screen.add_child(sw)
	start_switch_knob = Panel.new()
	var kb := StyleBoxFlat.new()
	kb.bg_color = Color("#FFF6EC")
	kb.set_corner_radius_all(32)
	start_switch_knob.add_theme_stylebox_override("panel", kb)
	start_switch_knob.size = Vector2(64, 64)
	start_switch_knob.position = Vector2(78, 8)
	start_switch_knob.mouse_filter = Control.MOUSE_FILTER_IGNORE
	sw.add_child(start_switch_knob)
	var hit := Button.new()
	hit.flat = true
	hit.position = Vector2(90, 1500)
	hit.size = Vector2(900, 190)
	for st in ["focus", "hover", "pressed", "normal"]:
		hit.add_theme_stylebox_override(st, StyleBoxEmpty.new())
	hit.pressed.connect(_toggle_start_switch)
	start_screen.add_child(hit)

	var play := _pill_button(Rect2(90, 1750, 900, 160), "Başla", 60, AMBER, Color("#1B1206"), start_screen)
	play.add_theme_font_override("font", font_heading)
	play.pressed.connect(func() -> void:
		var tw := create_tween()
		tw.tween_property(start_screen, "modulate:a", 0.0, 0.5)
		tw.tween_callback(start_screen.get_parent().queue_free)
		_begin(true))
	var hint := _label("Bu ayarı oyunda sağ üstteki düğmeyle de değiştirebilirsin.", font_body, 28, Color("#8F8478"), Vector2(0, 1950), start_screen)
	hint.size = Vector2(1080, 50)
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER


func _toggle_start_switch() -> void:
	_set_anim(not anim_on)
	var tw := create_tween()
	tw.tween_property(start_switch_knob, "position:x", 78.0 if anim_on else 8.0, 0.18).set_trans(Tween.TRANS_CUBIC)
	start_switch_bg.bg_color = AMBER if anim_on else Color(0.3, 0.3, 0.34)
	start_switch_label.text = "Açık: sinematik görünüm" if anim_on else "Kapalı: sade ve hızlı, hareket efekti yok"


# --- arayüz yardımcıları ---

func _glass(rect: Rect2, radius: float, tint: Color, border: Color, parent: Control = null) -> ColorRect:
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
	(parent if parent else hud).add_child(r)
	return r


func _pill_button(rect: Rect2, text: String, size: int, bg: Color, fg: Color, parent: Control) -> Button:
	var b := Button.new()
	b.text = text
	b.position = rect.position
	b.size = rect.size
	b.add_theme_font_override("font", font_label)
	b.add_theme_font_size_override("font_size", size)
	for state in ["font_color", "font_hover_color", "font_pressed_color", "font_focus_color"]:
		b.add_theme_color_override(state, fg)
	var box := StyleBoxFlat.new()
	box.bg_color = bg
	box.set_corner_radius_all(int(rect.size.y / 2.0))
	box.border_color = Color(1, 1, 1, 0.14) if bg.a < 1.0 else bg.lightened(0.25)
	box.set_border_width_all(2)
	var pressed := box.duplicate() as StyleBoxFlat
	pressed.bg_color = bg.darkened(0.15)
	b.add_theme_stylebox_override("normal", box)
	b.add_theme_stylebox_override("hover", box)
	b.add_theme_stylebox_override("pressed", pressed)
	b.add_theme_stylebox_override("focus", StyleBoxEmpty.new())
	parent.add_child(b)
	return b


func _label(text: String, font: Font, size: int, color: Color, pos: Vector2, parent: Control = null) -> Label:
	var l := Label.new()
	l.text = text
	l.add_theme_font_override("font", font)
	l.add_theme_font_size_override("font_size", size)
	l.add_theme_color_override("font_color", color)
	l.position = pos
	(parent if parent else hud).add_child(l)
	return l


func _bar(rect: Rect2, color: Color) -> ColorRect:
	var r := ColorRect.new()
	r.color = color
	r.position = rect.position
	r.size = rect.size
	r.mouse_filter = Control.MOUSE_FILTER_IGNORE
	hud.add_child(r)
	return r


func _crystal_icon(k: float) -> Node2D:
	var n := Node2D.new()
	var outer := Polygon2D.new()
	outer.polygon = PackedVector2Array([Vector2(0, -30), Vector2(22, -8), Vector2(13, 28), Vector2(-13, 28), Vector2(-22, -8)])
	outer.color = Color("#8FDFFF")
	n.add_child(outer)
	var inner := Polygon2D.new()
	inner.polygon = PackedVector2Array([Vector2(0, -30), Vector2(22, -8), Vector2(0, 2), Vector2(-22, -8)])
	inner.color = Color("#E4F8FF")
	n.add_child(inner)
	n.scale = Vector2.ONE * k
	return n


# --- 3B yardımcılar ---

func _cell_pos(cell: Vector2i) -> Vector3:
	return Vector3(cell.x - (COLS - 1) / 2.0, 0.0, cell.y - (ROWS - 1) / 2.0)


func _cylinder(top: float, bottom: float, height: float, segments: int) -> CylinderMesh:
	var c := CylinderMesh.new()
	c.top_radius = top
	c.bottom_radius = bottom
	c.height = height
	c.radial_segments = segments
	c.rings = 1
	return c


func _box(x: float, y: float, z: float) -> BoxMesh:
	var b := BoxMesh.new()
	b.size = Vector3(x, y, z)
	return b


func _mesh(mesh: Mesh, mat: Material, pos: Vector3) -> MeshInstance3D:
	var mi := MeshInstance3D.new()
	mi.mesh = mesh
	mi.material_override = mat
	mi.position = pos
	world.add_child(mi)
	return mi


## GLB modelini kurar: tabanı y=0'a, ortası merkeze oturur; yatayda `size` kadar yer kaplar.
func _spawn_glb(path: String, size: float, parent: Node3D) -> Node3D:
	var scene: PackedScene = load(path)
	var inst: Node3D = scene.instantiate()
	var box := _mesh_aabb(inst, Transform3D.IDENTITY)
	var pivot := Node3D.new()
	pivot.add_child(inst)
	inst.position = -Vector3(box.get_center().x, box.position.y, box.get_center().z)
	pivot.scale = Vector3.ONE * (size / maxf(box.size.x, box.size.z))
	parent.add_child(pivot)
	return pivot


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


func _add_blinker(pos: Vector3, color: Color, phase: float, parent: Node3D = null) -> void:
	var bulb := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.03
	sphere.height = 0.06
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


func _soft_dot() -> GradientTexture2D:
	if _soft_tex:
		return _soft_tex
	var g := Gradient.new()
	g.set_color(0, Color(1, 1, 1, 1))
	g.set_color(1, Color(1, 1, 1, 0))
	g.add_point(0.4, Color(1, 1, 1, 0.5))
	_soft_tex = GradientTexture2D.new()
	_soft_tex.gradient = g
	_soft_tex.fill = GradientTexture2D.FILL_RADIAL
	_soft_tex.fill_from = Vector2(0.5, 0.5)
	_soft_tex.fill_to = Vector2(1.0, 0.5)
	_soft_tex.width = 64
	_soft_tex.height = 64
	return _soft_tex


## Parçacık sistemi: yumuşak kenarlı, kameraya dönük lekeler. emission > 0 ise ışık saçar (ateş, kıvılcım).
func _particles(amount: int, lifetime: float, direction: Vector3, spread: float, vmin: float, vmax: float,
		gravity: Vector3, smin: float, smax: float, colors: Array, radius: float, emission: float) -> GPUParticles3D:
	var p := GPUParticles3D.new()
	p.amount = amount
	p.lifetime = lifetime
	p.local_coords = false
	p.fixed_fps = 0
	var pm := ParticleProcessMaterial.new()
	pm.direction = direction if direction != Vector3.ZERO else Vector3.UP
	pm.spread = spread
	pm.initial_velocity_min = vmin
	pm.initial_velocity_max = vmax
	pm.gravity = gravity
	pm.scale_min = smin
	pm.scale_max = smax
	var g := Gradient.new()
	g.set_color(0, colors[0])
	g.set_color(1, colors[colors.size() - 1])
	if colors.size() == 3:
		g.add_point(0.35, colors[1])
	var tex := GradientTexture1D.new()
	tex.gradient = g
	pm.color_ramp = tex
	p.process_material = pm
	var mesh := QuadMesh.new()
	mesh.size = Vector2.ONE * radius * 2.0
	var mat := StandardMaterial3D.new()
	mat.vertex_color_use_as_albedo = true
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.billboard_mode = BaseMaterial3D.BILLBOARD_PARTICLES
	mat.albedo_texture = _soft_dot()
	mat.depth_draw_mode = BaseMaterial3D.DEPTH_DRAW_DISABLED
	if emission > 0.0:
		mat.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
		mat.albedo_color = Color(emission, emission, emission)
		mat.disable_fog = true
	mesh.material = mat
	p.draw_pass_1 = mesh
	return p


func _build_ambient_dust() -> void:
	# güneş ışığında parlayan asılı toz zerreleri
	var motes := _particles(140, 10.0, Vector3(1, 0.05, 0.2), 25.0, 0.1, 0.3, Vector3.ZERO, 0.8, 1.6,
		[Color(1.0, 0.86, 0.7, 0.0), Color(1.0, 0.86, 0.7, 0.5), Color(1.0, 0.86, 0.7, 0.0)], 0.014, 1.2)
	motes.preprocess = 10.0
	var pm := motes.process_material as ParticleProcessMaterial
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	pm.emission_box_extents = Vector3(9, 2.2, 8)
	motes.position = Vector3(0, 1.8, -1.0)
	world.add_child(motes)
	fx_nodes.append(motes)
	# yerden savrulan ince toz perdeleri
	var drift := _particles(40, 9.0, Vector3(1, 0.02, 0.1), 10.0, 0.6, 1.2, Vector3.ZERO, 1.0, 2.0,
		[Color(0.75, 0.5, 0.38, 0.0), Color(0.75, 0.5, 0.38, 0.12), Color(0.75, 0.5, 0.38, 0.0)], 1.0, 0.0)
	drift.preprocess = 9.0
	var dpm := drift.process_material as ParticleProcessMaterial
	dpm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	dpm.emission_box_extents = Vector3(3, 0.3, 14)
	drift.position = Vector3(-12, 0.4, -8)
	world.add_child(drift)
	fx_nodes.append(drift)


# --- ekran görüntüsü (geliştirme için) ---

func _shot_args() -> Array[float]:
	var times: Array[float] = []
	for a in OS.get_cmdline_user_args():
		if a.begins_with("--at="):
			for t in a.substr(5).split(","):
				times.append(float(t))
	return times


func _setup_shots(times: Array[float]) -> void:
	var prefix := ""
	for a in OS.get_cmdline_user_args():
		if a.begins_with("--shots="):
			prefix = a.substr(8)
	if prefix == "" or times.is_empty():
		return
	for i in times.size():
		var path := "%s-%d.png" % [prefix, i + 1]
		get_tree().create_timer(times[i]).timeout.connect(func() -> void: _shot(path))
	get_tree().create_timer(times.max() + 0.5).timeout.connect(func() -> void: get_tree().quit())


func _shot(path: String) -> void:
	await RenderingServer.frame_post_draw
	get_viewport().get_texture().get_image().save_png(path)
