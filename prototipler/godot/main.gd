extends Node3D
## MarsKod görsel denemesi — taslak 5 (Godot, gerçekçi). Sahnenin tamamını koddan kurar:
## fotoğraf dokulu Mars arazisi, mavi Mars gün batımı, Phobos ve Deimos, kayan yıldızlar ve düşen meteorlar,
## NASA Perseverance gezgini, gerçekçi koloni, hologram kod bölgesi ve buzlu cam arayüz.
##
## Ekran görüntüsü almak için: godot --path . -- --shots=<dosya-öneki> --at=2,4,6

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

const CODE_TOP := 1606.0
const LINE_H := 86.0
const CHIP_ICON_POS := Vector2(792, 176)

const CAM_POS := Vector3(0, 4.5, 7.0)
const CAM_TARGET := Vector3(0, 0.0, -1.6)
const ROVER_YAW := 0.0  # NASA modelinin önü doğuya baksın diye düzeltme

var world: Node3D
var camera: Camera3D
var sun: DirectionalLight3D
var rover: Node3D
var rover_body: Node3D
var rover_cell := Vector2i.ZERO
var rover_start := Vector2i.ZERO
var dust_trail: GPUParticles3D
var ices := {}
var ice_materials: Array[ShaderMaterial] = []
var blinkers: Array = []
var spinners: Array[Node3D] = []
var ice_count := 0
var time := 0.0
var shake := 0.0
var rng := RandomNumberGenerator.new()

var hud: Control
var ice_label: Label
var chip_bar: ColorRect
var mission_bar: ColorRect
var line_bar: Control
var line_numbers: Array[Label] = []

var noise_dunes := FastNoiseLite.new()
var noise_detail := FastNoiseLite.new()
var noise_mesa := FastNoiseLite.new()
var noise_rock := FastNoiseLite.new()

var font_heading: FontFile
var font_label: FontFile
var font_body: FontFile
var font_code: FontFile

# ortak malzemeler
var m_hull: StandardMaterial3D
var m_hull_dark: StandardMaterial3D
var m_steel: StandardMaterial3D
var m_solar: StandardMaterial3D
var m_glass: StandardMaterial3D
var m_window: StandardMaterial3D
var m_concrete: StandardMaterial3D
var m_orange: StandardMaterial3D
var m_rock: StandardMaterial3D
var rock_meshes: Array[ArrayMesh] = []


func _ready() -> void:
	rng.seed = 2026
	font_heading = load("res://fonts/ChakraPetch_700Bold.ttf")
	font_label = load("res://fonts/ChakraPetch_600SemiBold.ttf")
	font_body = load("res://fonts/ChakraPetch_500Medium.ttf")
	font_code = load("res://fonts/JetBrainsMono_400Regular.ttf")
	noise_dunes.seed = 11
	noise_dunes.frequency = 0.05
	noise_dunes.fractal_octaves = 4
	noise_detail.seed = 23
	noise_detail.frequency = 0.3
	noise_mesa.seed = 5
	noise_mesa.frequency = 0.03
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
	_build_colony()
	_build_boulders()
	_build_moons()
	_build_ambient_dust()
	_build_hud()
	_setup_shots()
	_meteor_loop()
	_run_demo()


func _process(delta: float) -> void:
	time += delta
	for i in ice_materials.size():
		ice_materials[i].set_shader_parameter("glow", 1.0 + 0.35 * sin(time * 2.0 + i * 1.3))
	for b in blinkers:
		var on := fmod(time + b[2], 1.6) < 0.22
		(b[0] as OmniLight3D).light_energy = 2.5 if on else 0.0
		(b[1] as StandardMaterial3D).emission_energy_multiplier = 7.0 if on else 0.3
	for s in spinners:
		s.rotation.y += delta * 0.3
	if rover_body:
		rover_body.position.y = abs(sin(time * 4.0)) * 0.006
	if camera:
		var a := sin(time * 0.2) * 0.03
		var jitter := Vector3(randf_range(-1, 1), randf_range(-1, 1), 0) * shake
		camera.position = CAM_POS.rotated(Vector3.UP, a) + jitter
		camera.look_at(CAM_TARGET + jitter * 0.5)
		shake = move_toward(shake, 0.0, delta * 0.35)


# --- malzemeler ---

func _std(color: Color, rough: float, metal := 0.0) -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.albedo_color = color
	m.roughness = rough
	m.metallic = metal
	return m


func _make_materials() -> void:
	m_hull = _std(Color(0.7, 0.67, 0.63), 0.55, 0.1)
	m_hull_dark = _std(Color(0.28, 0.3, 0.33), 0.45, 0.6)
	m_steel = _std(Color(0.78, 0.79, 0.81), 0.22, 1.0)
	m_solar = _std(Color(0.04, 0.07, 0.17), 0.1, 0.7)
	m_concrete = _std(Color(0.36, 0.3, 0.27), 0.92)
	m_orange = _std(Color(1.0, 0.42, 0.12), 0.5)
	m_orange.emission_enabled = true
	m_orange.emission = Color(1.0, 0.35, 0.08)
	m_orange.emission_energy_multiplier = 0.3
	m_glass = _std(Color(0.72, 0.9, 1.0, 0.28), 0.04)
	m_glass.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	m_glass.metallic_specular = 0.9
	m_window = _std(Color(1.0, 0.82, 0.58), 0.3)
	m_window.emission_enabled = true
	m_window.emission = Color(1.0, 0.78, 0.5)
	m_window.emission_energy_multiplier = 3.0
	m_rock = StandardMaterial3D.new()
	m_rock.albedo_texture = load("res://textures/Rock061_Color.jpg")
	m_rock.normal_enabled = true
	m_rock.normal_texture = load("res://textures/Rock061_NormalGL.jpg")
	m_rock.roughness_texture = load("res://textures/Rock061_Roughness.jpg")
	m_rock.albedo_color = Color(1.0, 0.6, 0.45)
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
	sky.sky_material = sky_mat
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_energy = 0.9
	env.reflected_light_source = Environment.REFLECTION_SOURCE_SKY
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.tonemap_exposure = 1.15
	env.fog_enabled = true
	env.fog_light_color = Color("#8A6A62")
	env.fog_light_energy = 0.6
	env.fog_density = 0.011
	env.fog_sky_affect = 0.0
	env.fog_sun_scatter = 0.35
	env.glow_enabled = true
	env.glow_intensity = 0.7
	env.glow_bloom = 0.03
	env.glow_hdr_threshold = 1.0
	env.adjustment_enabled = true
	env.adjustment_saturation = 1.1
	env.adjustment_contrast = 1.1
	var we := WorldEnvironment.new()
	we.environment = env
	add_child(we)


func _build_lights() -> void:
	# alçak gün batımı güneşi: solda, ufka yakın; uzun gölgeler
	sun = DirectionalLight3D.new()
	sun.light_color = Color("#FFE7D0")
	sun.light_energy = 2.2
	sun.shadow_enabled = true
	sun.shadow_blur = 1.0
	sun.directional_shadow_max_distance = 45.0
	add_child(sun)
	sun.look_at_from_position(Vector3(-20, 5.5, -9), Vector3.ZERO)
	# kameranın arkasından hafif dolgu: oyun alanı okunaklı kalsın
	var fill := DirectionalLight3D.new()
	fill.light_color = Color("#AFC4FF")
	fill.light_energy = 0.45
	add_child(fill)
	fill.look_at_from_position(Vector3(3, 6, 10), Vector3.ZERO)


func _build_camera() -> void:
	camera = Camera3D.new()
	camera.keep_aspect = Camera3D.KEEP_WIDTH
	camera.fov = 58.0
	camera.v_offset = -1.4
	camera.far = 400.0
	camera.position = CAM_POS
	var attrs := CameraAttributesPractical.new()
	attrs.dof_blur_far_enabled = true
	attrs.dof_blur_far_distance = 20.0
	attrs.dof_blur_far_transition = 18.0
	attrs.dof_blur_amount = 0.05
	camera.attributes = attrs
	add_child(camera)
	camera.look_at(CAM_TARGET)


# --- arazi ---

func _height(x: float, z: float) -> float:
	var d := maxf(absf(x), absf(z))
	var outside := smoothstep(3.5, 6.5, d)
	var h := outside * (noise_dunes.get_noise_2d(x, z) * 0.9 + noise_detail.get_noise_2d(x, z) * 0.12)
	var far := smoothstep(18.0, 38.0, d)
	var m := noise_mesa.get_noise_2d(x, z)
	h += far * (smoothstep(0.05, 0.14, m) * 6.0 + smoothstep(0.28, 0.36, m) * 5.0)
	h += far * noise_dunes.get_noise_2d(x * 0.25, z * 0.25) * 3.0
	return h


func _build_terrain() -> void:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var n := 200
	var coords := PackedFloat32Array()
	for i in n + 1:
		var u := i / float(n) * 2.0 - 1.0
		coords.append(signf(u) * (absf(u) * 30.0 + pow(absf(u), 4.0) * 150.0))
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
	mat.shader = load("res://terrain.gdshader")
	mat.set_shader_parameter("ground_albedo", load("res://textures/Ground079S_Color.jpg"))
	mat.set_shader_parameter("ground_normal", load("res://textures/Ground079S_NormalGL.jpg"))
	mat.set_shader_parameter("ground_rough", load("res://textures/Ground079S_Roughness.jpg"))
	mat.set_shader_parameter("dune_albedo", load("res://textures/Ground093C_Color.jpg"))
	mat.set_shader_parameter("dune_normal", load("res://textures/Ground093C_NormalGL.jpg"))
	mat.set_shader_parameter("rock_albedo", load("res://textures/Rock061_Color.jpg"))
	mat.set_shader_parameter("rock_normal", load("res://textures/Rock061_NormalGL.jpg"))
	mi.material_override = mat
	mi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	world.add_child(mi)


# --- kod bölgesi (hologram ızgara + projektörler) ---

func _build_zone() -> void:
	var grid := MeshInstance3D.new()
	var plane := PlaneMesh.new()
	plane.size = Vector2(COLS, ROWS)
	grid.mesh = plane
	var gmat := ShaderMaterial.new()
	gmat.shader = load("res://hologrid.gdshader")
	grid.material_override = gmat
	grid.position.y = 0.015
	grid.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	world.add_child(grid)
	var hx := COLS / 2.0 + 0.55
	var hz := ROWS / 2.0 + 0.55
	var phase := 0.0
	for corner in [Vector3(-hx, 0, -hz), Vector3(hx, 0, -hz), Vector3(hx, 0, hz), Vector3(-hx, 0, hz)]:
		_floodlight(corner, phase)
		phase += 0.4


func _floodlight(base: Vector3, phase: float) -> void:
	var height := 2.4
	_mesh(_cylinder(0.045, 0.07, height, 8), m_steel, base + Vector3(0, height / 2.0, 0))
	# üç ayak
	for k in 3:
		var a := k * TAU / 3.0
		var leg := _mesh(_cylinder(0.02, 0.02, 0.9, 6), m_steel, base + Vector3(cos(a) * 0.22, 0.4, sin(a) * 0.22))
		leg.rotation = Vector3(sin(a) * 0.5, 0, -cos(a) * 0.5)
	var head := Node3D.new()
	head.position = base + Vector3(0, height, 0)
	world.add_child(head)
	head.look_at(Vector3(0, 0, 0), Vector3.UP)
	var box := BoxMesh.new()
	box.size = Vector3(0.36, 0.22, 0.12)
	var lamp := MeshInstance3D.new()
	lamp.mesh = box
	lamp.material_override = m_hull_dark
	head.add_child(lamp)
	var face := MeshInstance3D.new()
	var fbox := BoxMesh.new()
	fbox.size = Vector3(0.3, 0.16, 0.02)
	face.mesh = fbox
	face.material_override = m_window
	face.position = Vector3(0, 0, -0.065)
	head.add_child(face)
	var spot := SpotLight3D.new()
	spot.light_color = Color("#FFF1DC")
	spot.light_energy = 2.4
	spot.spot_range = 11.0
	spot.spot_angle = 34.0
	spot.shadow_enabled = false
	head.add_child(spot)
	_add_blinker(base + Vector3(0, height + 0.2, 0), Color("#FF7A2A"), phase)


func _build_map() -> void:
	for gy in ROWS:
		var row: String = MAP[gy]
		for gx in COLS:
			var cell := Vector2i(gx, gy)
			var pos := _cell_pos(cell)
			match row[gx]:
				"I":
					var ice := _ice_cluster(pos, gx * 31 + gy * 7)
					ices[cell] = ice
				"R":
					_boulder(pos, rng.randf_range(0.55, 0.7), rng.randf() * TAU)
				"@":
					rover_start = cell
					_build_rover()
				"D":
					_cargo_pod(pos)


# --- buz kristali kümesi ---

func _ice_cluster(pos: Vector3, seed: int) -> Node3D:
	var r := RandomNumberGenerator.new()
	r.seed = seed
	var cluster := Node3D.new()
	cluster.position = pos
	world.add_child(cluster)
	var mat := ShaderMaterial.new()
	mat.shader = load("res://ice.gdshader")
	ice_materials.append(mat)
	# yerdeki buzlanma
	var frost := MeshInstance3D.new()
	var disk := CylinderMesh.new()
	disk.top_radius = 0.42
	disk.bottom_radius = 0.46
	disk.height = 0.03
	frost.mesh = disk
	var fmat := _std(Color(0.75, 0.82, 0.88), 0.7)
	fmat.emission_enabled = true
	fmat.emission = Color(0.55, 0.85, 1.0)
	fmat.emission_energy_multiplier = 0.1
	frost.material_override = fmat
	frost.position.y = 0.012
	cluster.add_child(frost)
	# 7–10 altıgen kristal, ortadakiler daha uzun
	var count := r.randi_range(7, 10)
	for k in count:
		var radius := r.randf_range(0.035, 0.07)
		var height := r.randf_range(0.18, 0.5) * (1.3 if k < 2 else 1.0)
		var crystal := Node3D.new()
		var body := MeshInstance3D.new()
		body.mesh = _cylinder(radius, radius, height, 6)
		body.material_override = mat
		body.position.y = height / 2.0
		crystal.add_child(body)
		var tip := MeshInstance3D.new()
		tip.mesh = _cylinder(0.0, radius, radius * 2.2, 6)
		tip.material_override = mat
		tip.position.y = height + radius * 1.1
		crystal.add_child(tip)
		var spread := 0.05 if k < 2 else r.randf_range(0.08, 0.26)
		var ang := r.randf() * TAU
		crystal.position = Vector3(cos(ang) * spread, 0, sin(ang) * spread)
		crystal.rotation = Vector3(r.randf_range(-0.55, 0.55), r.randf() * TAU, r.randf_range(-0.55, 0.55))
		cluster.add_child(crystal)
	var glow := OmniLight3D.new()
	glow.light_color = Color("#6FD8FF")
	glow.light_energy = 1.1
	glow.omni_range = 1.4
	glow.position = Vector3(0, 0.35, 0)
	cluster.add_child(glow)
	cluster.set_meta("scale", cluster.scale)
	return cluster


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
			var r := 0.5 * (1.0 + n1 * 0.45 + n2 * 0.12)
			p = nrm * r
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
	mi.position = pos + Vector3(0, size * 0.05, 0)
	world.add_child(mi)
	return mi


func _build_boulders() -> void:
	var placed := 0
	while placed < 140:
		var x := rng.randf_range(-22.0, 22.0)
		var z := rng.randf_range(-20.0, 10.0)
		if maxf(absf(x), absf(z)) < 4.4:
			continue
		if z < -8.0 and absf(x) < 12.0 and z > -18.0:
			if rng.randf() < 0.7:
				continue
		var size := rng.randf_range(0.08, 0.45)
		if rng.randf() < 0.08:
			size = rng.randf_range(0.9, 2.2)
		_boulder(Vector3(x, _height(x, z) - size * 0.12, z), size, rng.randf() * TAU)
		placed += 1


# --- koloni ---

func _build_colony() -> void:
	var gz := -11.0
	# ana habitat kubbesi
	_dome(Vector3(-6.8, _height(-6.8, gz), gz), 2.3, m_hull, true)
	# cam sera kubbesi, içinde bitkiler
	var green := Vector3(-2.2, _height(-2.2, gz - 1.6), gz - 1.6)
	_dome(green, 1.7, m_glass, false)
	for k in 9:
		var a := k * TAU / 9.0
		var plant := MeshInstance3D.new()
		var s := SphereMesh.new()
		s.radius = 0.28
		s.height = 0.4
		plant.mesh = s
		var pm := _std(Color(0.2, 0.7, 0.3), 0.8)
		pm.emission_enabled = true
		pm.emission = Color(0.2, 1.0, 0.4)
		pm.emission_energy_multiplier = 0.6
		plant.material_override = pm
		plant.position = green + Vector3(cos(a) * 0.9, 0.15, sin(a) * 0.9)
		world.add_child(plant)
	var grow := OmniLight3D.new()
	grow.light_color = Color("#8CFFB0")
	grow.light_energy = 2.5
	grow.omni_range = 3.5
	grow.position = green + Vector3(0, 0.8, 0)
	world.add_child(grow)
	# bağlantı tüneli
	_tunnel(Vector3(-4.6, _height(-4.6, gz - 0.6) + 0.55, gz - 0.6), 2.4)
	# laboratuvar kulesi ve iletişim direği
	var lab := Vector3(3.2, _height(3.2, gz), gz)
	_mesh(_cylinder(1.05, 1.1, 2.6, 24), m_hull, lab + Vector3(0, 1.3, 0))
	_mesh(_cylinder(1.12, 1.12, 0.12, 24), m_orange, lab + Vector3(0, 0.9, 0))
	_dome(lab + Vector3(0, 2.6, 0), 1.05, m_hull, false)
	for k in 6:
		var a := k * TAU / 6.0
		var w := _mesh(_box(0.34, 0.2, 0.05), m_window, lab + Vector3(cos(a) * 1.1, 1.8, sin(a) * 1.1))
		w.rotation.y = -a + PI / 2.0
	_mesh(_cylinder(0.07, 0.1, 4.2, 10), m_hull_dark, lab + Vector3(0.6, 5.3, 0))
	var dish := Node3D.new()
	dish.position = lab + Vector3(0.6, 7.2, 0)
	world.add_child(dish)
	var bowl := MeshInstance3D.new()
	var bs := SphereMesh.new()
	bs.radius = 0.55
	bs.height = 0.35
	bs.is_hemisphere = true
	bowl.mesh = bs
	bowl.material_override = m_hull
	bowl.rotation = Vector3(-1.9, 0, 0)
	dish.add_child(bowl)
	spinners.append(dish)
	_add_blinker(lab + Vector3(0.6, 7.6, 0), Color("#FF2E2E"), 0.9)
	# güneş paneli tarlası
	for row in 3:
		for col in 5:
			var p := Vector3(6.2 + col * 1.25, 0, -6.5 - row * 1.3)
			p.y = _height(p.x, p.z)
			_mesh(_cylinder(0.03, 0.03, 0.5, 6), m_steel, p + Vector3(0, 0.25, 0))
			var panel := _mesh(_box(1.1, 0.04, 0.75), m_solar, p + Vector3(0, 0.55, 0))
			panel.rotation = Vector3(0.5, -0.3, 0)
	# roket ve rampası
	var pad := Vector3(10.5, _height(10.5, -16.0), -16.0)
	_mesh(_cylinder(3.2, 3.3, 0.22, 40), m_concrete, pad + Vector3(0, 0.11, 0))
	var ring := MeshInstance3D.new()
	var torus := TorusMesh.new()
	torus.inner_radius = 2.85
	torus.outer_radius = 3.0
	ring.mesh = torus
	ring.material_override = m_orange
	ring.position = pad + Vector3(0, 0.24, 0)
	world.add_child(ring)
	_mesh(_cylinder(0.85, 0.85, 7.0, 32), m_steel, pad + Vector3(0, 3.72, 0))
	_mesh(_cylinder(0.0, 0.85, 2.4, 32), m_steel, pad + Vector3(0, 8.42, 0))
	for k in 3:
		var a := k * TAU / 3.0
		var fin := _mesh(_box(0.08, 1.4, 1.0), m_hull_dark, pad + Vector3(cos(a) * 0.95, 1.0, sin(a) * 0.95))
		fin.rotation.y = -a
	_add_blinker(pad + Vector3(0, 9.75, 0), Color("#FF2E2E"), 0.2)
	_steam(pad + Vector3(0, 0.3, 0))
	# boru hattı: koloniden kod bölgesine
	for k in 6:
		var p := Vector3(-1.2, 0, -4.4 - k * 1.1)
		p.y = _height(p.x, p.z) + 0.1
		var pipe := _mesh(_cylinder(0.07, 0.07, 1.12, 10), m_hull_dark, p)
		pipe.rotation.x = PI / 2.0


func _dome(center: Vector3, radius: float, mat: Material, windows: bool) -> void:
	var dome := MeshInstance3D.new()
	var s := SphereMesh.new()
	s.radius = radius
	s.height = radius
	s.is_hemisphere = true
	s.radial_segments = 40
	s.rings = 16
	dome.mesh = s
	dome.material_override = mat
	dome.position = center
	world.add_child(dome)
	var base := MeshInstance3D.new()
	var t := TorusMesh.new()
	t.inner_radius = radius - 0.05
	t.outer_radius = radius + 0.12
	base.mesh = t
	base.material_override = m_hull_dark
	base.position = center + Vector3(0, 0.05, 0)
	world.add_child(base)
	if windows:
		for k in 10:
			var a := k * TAU / 10.0
			var w := _mesh(_box(0.3, 0.16, 0.04), m_window, center + Vector3(cos(a) * radius * 0.93, radius * 0.32, sin(a) * radius * 0.93))
			w.rotation.y = -a + PI / 2.0
			w.rotation.x = -0.35
		var door := _mesh(_box(0.6, 0.8, 0.3), m_hull_dark, center + Vector3(0, 0.4, radius - 0.05))
		door.name = "door"


func _tunnel(center: Vector3, length: float) -> void:
	var t := _mesh(_cylinder(0.55, 0.55, length, 24), m_hull, center)
	t.rotation.z = PI / 2.0
	for k in 3:
		var ring := _mesh(_cylinder(0.6, 0.6, 0.08, 24), m_hull_dark, center + Vector3(-length / 2.0 + k * length / 2.0, 0, 0))
		ring.rotation.z = PI / 2.0
	var strip := _mesh(_box(length * 0.7, 0.08, 0.04), m_window, center + Vector3(0, 0.18, 0.54))


func _cargo_pod(pos: Vector3) -> void:
	_mesh(_box(0.78, 0.46, 0.6), m_hull, pos + Vector3(0, 0.23, 0))
	_mesh(_box(0.8, 0.07, 0.62), m_orange, pos + Vector3(0, 0.4, 0))
	for k in 3:
		_mesh(_box(0.03, 0.47, 0.62), m_hull_dark, pos + Vector3(-0.3 + k * 0.3, 0.23, 0))
	_add_blinker(pos + Vector3(0.3, 0.52, 0.2), Color("#5FE0FF"), 0.3)


# --- Phobos ve Deimos ---

func _build_moons() -> void:
	var mat := StandardMaterial3D.new()
	mat.albedo_texture = load("res://textures/Rock062_Color.jpg")
	mat.normal_enabled = true
	mat.normal_texture = load("res://textures/Rock062_NormalGL.jpg")
	mat.albedo_color = Color(0.62, 0.56, 0.52)
	mat.uv1_triplanar = true
	mat.uv1_scale = Vector3(0.6, 0.6, 0.6)
	mat.disable_fog = true
	for spec in [[Vector3(0.52, 0.34, -0.78) * 120.0, 7.5, 1.6], [Vector3(-0.2, 0.5, -0.84) * 140.0, 3.0, 1.1]]:
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
			verts[i] = p * (1.0 + bump)
			verts[i].x *= spec[2]
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


# --- meteorlar ---

func _meteor_loop() -> void:
	await _wait(3.0)
	while true:
		await _fireball()
		await _wait(rng.randf_range(6.0, 9.0))


func _fireball() -> void:
	# açık arazideki çarpma noktaları (koloninin ve mesaların arkasında kalmasın)
	var spots := [Vector3(-8.5, 0, -19), Vector3(-3, 0, -22), Vector3(4, 0, -22), Vector3(7.5, 0, -24)]
	var target: Vector3 = spots[rng.randi() % spots.size()] + Vector3(rng.randf_range(-1.5, 1.5), 0, rng.randf_range(-1.5, 1.5))
	target.y = _height(target.x, target.z)
	var start := target + Vector3(-18.0 if target.x > 0 else 18.0, 30.0, -10.0)
	var head := Node3D.new()
	head.position = start
	world.add_child(head)
	var core := MeshInstance3D.new()
	var s := SphereMesh.new()
	s.radius = 0.35
	s.height = 0.7
	core.mesh = s
	var cm := StandardMaterial3D.new()
	cm.albedo_color = Color(1.0, 0.9, 0.7)
	cm.emission_enabled = true
	cm.emission = Color(1.0, 0.75, 0.45)
	cm.emission_energy_multiplier = 12.0
	cm.disable_fog = true
	s.radius = 0.25
	s.height = 0.5
	core.material_override = cm
	head.add_child(core)
	var light := OmniLight3D.new()
	light.light_color = Color("#FFB070")
	light.light_energy = 5.0
	light.omni_range = 18.0
	head.add_child(light)
	var fire := _particles(400, 0.7, Vector3.ZERO, 180.0, 0.2, 0.8, Vector3.ZERO, 0.6, 1.4,
		[Color(1, 0.9, 0.7, 1), Color(1, 0.45, 0.1, 0.7), Color(0.5, 0.1, 0.02, 0.0)], 0.5, 3.0)
	head.add_child(fire)
	var smoke := _particles(200, 3.0, Vector3.UP, 180.0, 0.1, 0.4, Vector3(0.4, 0.2, 0), 1.0, 2.5,
		[Color(0.3, 0.26, 0.26, 0.35), Color(0.25, 0.22, 0.22, 0.0)], 0.7, 0.0)
	head.add_child(smoke)
	var tw := create_tween()
	tw.tween_property(head, "position", target, 1.7).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_IN)
	await tw.finished
	fire.emitting = false
	smoke.emitting = false
	core.visible = false
	light.visible = false
	_impact(target)
	get_tree().create_timer(3.0).timeout.connect(head.queue_free)


func _impact(p: Vector3) -> void:
	# parlama
	var flash := OmniLight3D.new()
	flash.light_color = Color("#FFC080")
	flash.light_energy = 40.0
	flash.omni_range = 40.0
	flash.position = p + Vector3(0, 2, 0)
	world.add_child(flash)
	var ft := create_tween()
	ft.tween_property(flash, "light_energy", 0.0, 1.2).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	ft.tween_callback(flash.queue_free)
	# ateş topu
	var ball := MeshInstance3D.new()
	var q := QuadMesh.new()
	q.size = Vector2(2.4, 2.4)
	ball.mesh = q
	var bm := StandardMaterial3D.new()
	bm.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	bm.billboard_mode = BaseMaterial3D.BILLBOARD_ENABLED
	bm.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
	bm.albedo_texture = _soft_dot()
	bm.albedo_color = Color(4.0, 2.2, 0.8, 1.0)
	bm.disable_fog = true
	ball.material_override = bm
	ball.position = p + Vector3(0, 1.0, 0)
	ball.scale = Vector3.ONE * 0.1
	world.add_child(ball)
	var bt := create_tween().set_parallel(true)
	bt.tween_property(ball, "scale", Vector3.ONE * 3.2, 0.5).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	bt.tween_property(bm, "albedo_color", Color(0, 0, 0, 1), 0.9).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)
	bt.chain().tween_callback(ball.queue_free)
	# şok dalgası halkası
	var ring := MeshInstance3D.new()
	var t := TorusMesh.new()
	t.inner_radius = 0.9
	t.outer_radius = 1.0
	ring.mesh = t
	var rm := StandardMaterial3D.new()
	rm.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	rm.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	rm.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
	rm.albedo_color = Color(1.0, 0.75, 0.5, 0.8)
	rm.disable_fog = true
	ring.material_override = rm
	ring.position = p + Vector3(0, 0.3, 0)
	ring.scale = Vector3(1, 0.15, 1)
	world.add_child(ring)
	var rt := create_tween().set_parallel(true)
	rt.tween_property(ring, "scale", Vector3(12, 0.4, 12), 1.4).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	rt.tween_property(rm, "albedo_color:a", 0.0, 1.4)
	rt.chain().tween_callback(ring.queue_free)
	# kıvılcımlar ve yükselen toz bulutu
	var sparks := _particles(160, 1.6, Vector3.UP, 60.0, 6.0, 14.0, Vector3(0, -9.8, 0), 0.6, 1.4,
		[Color(1, 0.9, 0.6, 1), Color(1, 0.4, 0.1, 0.0)], 0.07, 6.0)
	sparks.one_shot = true
	sparks.explosiveness = 0.95
	sparks.position = p + Vector3(0, 0.5, 0)
	world.add_child(sparks)
	sparks.emitting = true
	var plume := _particles(160, 7.0, Vector3.UP, 30.0, 2.0, 5.5, Vector3(0.4, -0.35, 0), 6.0, 14.0,
		[Color(0.5, 0.33, 0.26, 0.0), Color(0.5, 0.34, 0.27, 0.85), Color(0.45, 0.32, 0.26, 0.0)], 0.5, 0.0)
	plume.one_shot = true
	plume.explosiveness = 0.7
	plume.position = p + Vector3(0, 0.5, 0)
	world.add_child(plume)
	plume.emitting = true
	get_tree().create_timer(9.0).timeout.connect(sparks.queue_free)
	get_tree().create_timer(9.0).timeout.connect(plume.queue_free)
	# kamera hafifçe sarsılır (uzaklığa göre)
	shake = clampf(0.9 / maxf(p.length() * 0.15, 1.0), 0.03, 0.12)


# --- robot (NASA Perseverance) ---

func _build_rover() -> void:
	rover = Node3D.new()
	world.add_child(rover)
	rover_body = Node3D.new()
	rover.add_child(rover_body)
	var model := _spawn_glb("res://models/nasa/perseverance.glb", 1.45, rover_body)
	model.rotation.y = ROVER_YAW
	var head := SpotLight3D.new()
	head.light_color = Color("#FFF2C8")
	head.light_energy = 3.0
	head.spot_range = 3.0
	head.spot_angle = 30.0
	head.position = Vector3(0.35, 0.45, 0)
	head.rotation_degrees = Vector3(-20, -90, 0)
	rover_body.add_child(head)
	_add_blinker(Vector3(-0.2, 0.75, 0.15), Color("#5FE0FF"), 0.5, rover_body)
	dust_trail = _particles(60, 1.6, Vector3.UP, 45.0, 0.25, 0.6, Vector3(0.15, -0.2, 0), 1.5, 3.2,
		[Color(0.72, 0.45, 0.32, 0.6), Color(0.72, 0.45, 0.32, 0.0)], 0.05, 0.0)
	dust_trail.emitting = false
	dust_trail.position = Vector3(-0.35, 0.05, 0)
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
	tw.tween_property(rover, "position", _cell_pos(target), 0.95).set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_IN_OUT)
	dust_trail.emitting = true
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
	_beam(rover.position + Vector3(0, 0.8, 0), pos + Vector3(0, 0.25, 0))
	await _wait(0.25)
	var tw := create_tween()
	tw.tween_property(ice, "scale", Vector3.ONE * 1.15, 0.12).set_trans(Tween.TRANS_SINE)
	tw.tween_property(ice, "scale", Vector3.ONE * 0.001, 0.32).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_IN)
	# kristal kırıntıları + ince havada buzun doğrudan buhara dönüşmesi (süblimleşme)
	var shards := _particles(50, 1.0, Vector3.UP, 70.0, 1.4, 3.0, Vector3(0, -6, 0), 0.6, 1.3,
		[Color(0.85, 0.97, 1.0, 1.0), Color(0.6, 0.9, 1.0, 0.0)], 0.03, 3.0)
	shards.one_shot = true
	shards.explosiveness = 0.95
	shards.position = pos + Vector3(0, 0.3, 0)
	world.add_child(shards)
	shards.emitting = true
	var vapor := _particles(24, 2.2, Vector3.UP, 30.0, 0.3, 0.6, Vector3(0.15, 0.2, 0), 1.0, 2.2,
		[Color(0.85, 0.95, 1.0, 0.0), Color(0.85, 0.95, 1.0, 0.2), Color(0.85, 0.95, 1.0, 0.0)], 0.12, 0.0)
	vapor.one_shot = true
	vapor.explosiveness = 0.6
	vapor.position = pos + Vector3(0, 0.2, 0)
	world.add_child(vapor)
	vapor.emitting = true
	get_tree().create_timer(3.0).timeout.connect(shards.queue_free)
	get_tree().create_timer(3.0).timeout.connect(vapor.queue_free)
	_float_text("+1 buz", pos + Vector3(0, 0.95, 0))
	_fly_icon(pos + Vector3(0, 0.4, 0))
	await tw.finished
	ice.visible = false


func _beam(from: Vector3, to: Vector3) -> void:
	var beam := MeshInstance3D.new()
	var cyl := CylinderMesh.new()
	cyl.top_radius = 0.018
	cyl.bottom_radius = 0.045
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
		ice.scale = Vector3.ONE
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

	_glass(Rect2(32, 72, 1016, 200), 44)
	_text("DÜNYA 1 · İNİŞ BÖLGESİ", font_label, 30, Color("#5FD8FF"), Vector2(80, 104))
	_text("Buz Avı", font_heading, 66, Color("#F3EEFA"), Vector2(80, 140))
	var badge := _glass(Rect2(336, 158, 170, 56), 28, Color(1.0, 0.48, 0.24, 0.85), Color(1, 0.8, 0.6, 0.6))
	(badge.material as ShaderMaterial).set_shader_parameter("blur", 0.0)
	_text("BÖLÜM 3", font_heading, 30, Color("#1A0E08"), Vector2(360, 164))
	_glass(Rect2(700, 102, 316, 142), 36, Color(0.02, 0.05, 0.1, 0.55), Color(0.4, 0.85, 1.0, 0.5))
	var icon := _crystal_icon(0.9)
	icon.position = CHIP_ICON_POS
	hud.add_child(icon)
	ice_label = _text("0/3", font_heading, 58, Color("#F3EEFA"), Vector2(840, 128))
	ice_label.pivot_offset = Vector2(50, 36)
	_bar(Rect2(760, 214, 180, 10), Color(1, 1, 1, 0.12))
	chip_bar = _bar(Rect2(760, 214, 0, 10), Color("#5FE0FF"))

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
	light.omni_range = 1.4
	light.position = pos
	(parent if parent else world).add_child(bulb)
	(parent if parent else world).add_child(light)
	blinkers.append([light, mat, phase])


## Parçacık sistemi kurar. colors: zaman içinde renk/saydamlık geçişi.
func _particles(amount: int, lifetime: float, direction: Vector3, spread: float, vmin: float, vmax: float,
		gravity: Vector3, smin: float, smax: float, colors: Array, radius: float, emission: float) -> GPUParticles3D:
	var p := GPUParticles3D.new()
	p.amount = amount
	p.lifetime = lifetime
	p.local_coords = false
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
	p.fixed_fps = 0
	var mesh := QuadMesh.new()
	mesh.size = Vector2.ONE * radius * 2.0
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color.WHITE
	mat.vertex_color_use_as_albedo = true
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	# yumuşak kenarlı leke: kameraya dönük kare, ortası dolu kenarı saydam
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.billboard_mode = BaseMaterial3D.BILLBOARD_PARTICLES
	mat.albedo_texture = _soft_dot()
	mat.depth_draw_mode = BaseMaterial3D.DEPTH_DRAW_DISABLED
	if emission > 0.0:
		# ışık saçan parçacık (ateş, kıvılcım): üst üste bindikçe parlar
		mat.blend_mode = BaseMaterial3D.BLEND_MODE_ADD
		mat.albedo_color = Color(emission, emission, emission)
		mat.disable_fog = true
	mesh.material = mat
	p.draw_pass_1 = mesh
	return p


var _soft_tex: GradientTexture2D

func _soft_dot() -> GradientTexture2D:
	if _soft_tex:
		return _soft_tex
	var g := Gradient.new()
	g.set_color(0, Color(1, 1, 1, 1))
	g.set_color(1, Color(1, 1, 1, 0))
	g.add_point(0.4, Color(1, 1, 1, 0.55))
	_soft_tex = GradientTexture2D.new()
	_soft_tex.gradient = g
	_soft_tex.fill = GradientTexture2D.FILL_RADIAL
	_soft_tex.fill_from = Vector2(0.5, 0.5)
	_soft_tex.fill_to = Vector2(1.0, 0.5)
	_soft_tex.width = 64
	_soft_tex.height = 64
	return _soft_tex


func _steam(pos: Vector3) -> void:
	var p := _particles(50, 4.0, Vector3(0.3, 1, 0), 25.0, 0.2, 0.5, Vector3(0.12, 0.04, 0), 4.0, 8.0,
		[Color(0.9, 0.86, 0.84, 0.0), Color(0.9, 0.86, 0.84, 0.3), Color(0.9, 0.86, 0.84, 0.0)], 0.1, 0.0)
	p.preprocess = 4.0
	p.position = pos
	world.add_child(p)


func _build_ambient_dust() -> void:
	# havada asılı ince toz
	var motes := _particles(160, 10.0, Vector3(1, 0.05, 0.2), 25.0, 0.1, 0.35, Vector3.ZERO, 0.8, 1.6,
		[Color(1.0, 0.85, 0.7, 0.0), Color(1.0, 0.85, 0.7, 0.55), Color(1.0, 0.85, 0.7, 0.0)], 0.015, 1.2)
	motes.preprocess = 10.0
	var pm := motes.process_material as ParticleProcessMaterial
	pm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	pm.emission_box_extents = Vector3(10, 2.2, 9)
	motes.position = Vector3(0, 1.8, -1.0)
	world.add_child(motes)
	# yere yakın rüzgarla sürüklenen toz perdeleri
	var drift := _particles(40, 8.0, Vector3(1, 0.02, 0.1), 10.0, 0.6, 1.2, Vector3.ZERO, 8.0, 16.0,
		[Color(0.75, 0.5, 0.38, 0.0), Color(0.75, 0.5, 0.38, 0.12), Color(0.75, 0.5, 0.38, 0.0)], 0.12, 0.0)
	drift.preprocess = 8.0
	var dpm := drift.process_material as ParticleProcessMaterial
	dpm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	dpm.emission_box_extents = Vector3(3, 0.3, 12)
	drift.position = Vector3(-14, 0.4, -6)
	world.add_child(drift)


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
