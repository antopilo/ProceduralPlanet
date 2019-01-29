extends Node

onready var camera = get_parent().get_node("Player")
onready var lbl_fps = get_node("../Control/fps")
var preScript = preload("res://scripts/softnoise.gd")
var prescript_tree = load("res://scripts/tree_gen.gd")

var material = preload("res://Materials/Material.tres")
var material_water = preload("res://Materials/Water.tres")


# Debug
onready var lbl_current_chunk = get_node("../Control/Current Chunk")

# Noise Settings
var softnoise1
var softnoise2
var softnoise3
var softnoise4

var tree_gen

export var resolution1 = 2048   
export var resolution2 = 1024
export var resolution3 = 512
export var resolution4 = 32

export var amplify = 2
export var amplitude3d = 8
export var amplitude1 = 25
export var amplitude2 = 10
export var amplitude3 = 5
export var amplitude4 = 2

# Levels settings:
var water_offset = 0.5

export var level_water = -1
export var level_grass = 15

var surfacetool

#Color settings
export(Color, RGB) var color_deepwater = Color("14509d")
export(Color, RGB) var color_water = Color("1eb7c3")
export(Color, RGB) var color_sand = Color("fdff35")
export(Color, RGB) var color_grass = Color("85f332")
export(Color, RGB) var color_deepgrass = Color("209204")
export(Color, RGB) var color_rock = Color("209204") 
export(Color, RGB) var color_deeprock = Color("209204")
export(Color, RGB) var color_snow = Color("209204")

# Chunk Settings
var chunk_size = Vector3(16,256,16)
var render_distance = 24

# Cube Settings
var cube_size = 1

var cubes_in_chunk = []
var loaded_chunks = []
var current_chunk = Vector2()
var water 
var water_mesh 

var tickrate = 240 / 60
var deltaTime = 0
var next_update = 0

var vertices = [Vector3(0,0,0),
                Vector3(cube_size,0,0),
                Vector3(cube_size,0,cube_size),
                Vector3(0,0,cube_size),

                Vector3(0,cube_size,0),
                Vector3(cube_size,cube_size,0),
                Vector3(cube_size,cube_size,cube_size),
                Vector3(0,cube_size,cube_size)]

func _ready():
	generate_seed()
	
	water = MeshInstance.new()
	
	water_mesh = PlaneMesh.new()
	water_mesh.size = Vector2(chunk_size.x * render_distance * 4, chunk_size.z * render_distance * 4)
	water_mesh.set_material(material_water)
	load_chunk(Vector2(0,0))
	load_chunk(Vector2(1,0))
	load_chunk(Vector2(0,1))
	load_chunk(Vector2(1,1))
	load_chunk(Vector2(-1,0))
	load_chunk(Vector2(0,-1))
	load_chunk(Vector2(-1,-1))
	water.mesh = water_mesh
	
	add_child(water)

func _physics_process(delta):
	deltaTime += delta

	# Tick rate update. / Lazy Chunk loading.
	if next_update <= deltaTime:
		update_chunk()
		next_update = deltaTime + tickrate

	# Move the water plane to the current chunk position
	
	update_water()
	
	# Debuggin HUD
	lbl_current_chunk.text = str(current_chunk)
	lbl_fps.text = str(Engine.get_frames_per_second())
	
func generate_seed():
	# Generate random Seed
	randomize()
	
	var current_seed = randi() % 200000
	
	softnoise1 = preScript.SoftNoise.new()
	softnoise1 = preScript.SoftNoise.new(randi() % current_seed)
	softnoise2 = preScript.SoftNoise.new()
	softnoise2 = preScript.SoftNoise.new(randi() % current_seed / 2)
	softnoise3 = preScript.SoftNoise.new()
	softnoise3 = preScript.SoftNoise.new(randi() % current_seed / 3)
	softnoise4 = preScript.SoftNoise.new()
	softnoise4 = preScript.SoftNoise.new(randi() % current_seed / 4)

func update_chunk():
	current_chunk = Vector2(floor(camera.translation.x / chunk_size.x), floor(camera.translation.z / chunk_size.z))
	
	for y in range(-render_distance, render_distance):
		for x in range(-render_distance, render_distance):
			var chunk = Vector2(current_chunk.x + x,current_chunk.y + y)
			
			if chunk_is_viewable(chunk):
				load_chunk(chunk)
				yield(get_tree(), "idle_frame")
				
				
	for chunk in loaded_chunks:
		if !chunk_is_viewable(chunk):
			unload_chunk(chunk)

	
func preload_chunk(position, chunk_position):
	# Make an array that generates the chunk without rendering it yet.
	for x in chunk_size.x:
		for z in chunk_size.z:
			var height = noise2d(Vector2(chunk_position.x + x, chunk_position.y + z))
			var current_pos = Vector3(x , height, z )
			cubes_in_chunk.append(current_pos)
		
func load_chunk(position):
	if chunk_exists(position):
		return

	var chunk_position = Vector2(floor(position.x) * chunk_size.x, floor(position.y) * chunk_size.z)
	surfacetool = SurfaceTool.new() # Create the mesh
	
	surfacetool.begin(Mesh.PRIMITIVE_TRIANGLES) # Begin surfaceStream
	surfacetool.set_material(material)
	
	preload_chunk(position, chunk_position)

	for cube_position in cubes_in_chunk:
		create_faces(surfacetool, cube_position, chunk_position)
		

	var chunk = MeshInstance.new() 
	chunk.name = str(position)
	chunk.mesh = surfacetool.commit() 
	chunk.create_trimesh_collision()


	chunk.add_to_group("chunk") 
	add_child(chunk)
	
	loaded_chunks.append(position)
	
	# Cleaning up
	surfacetool.clear()
	cubes_in_chunk.clear()
	

func unload_chunk(position):
	if has_node(str(position)):
		loaded_chunks.remove(loaded_chunks.find(position, 0))
		get_node(str(position)).queue_free()

func update_water():
	# Adds the water
	water.translation = Vector3(current_chunk.x * chunk_size.x + (chunk_size.x / 2), 
								level_water + water_offset, 
								current_chunk.y * chunk_size.z + (chunk_size.z / 2) )

func chunk_is_viewable(position):
	#Returns true if the chunk is in "viewable" distance.
	if stepify(abs((current_chunk - position).length()),1) <= render_distance :
		return true
	return false

func chunk_exists(position):
	# Returns true if the chunk is already loaded.
	if has_node(str(position)):
		return true
	return false
	
func create_faces(surfacetool, voxel_position, chunk_pos):
	# Top
	var voxel_pos = Vector3(voxel_position.x + chunk_pos.x, 
			voxel_position.y, 
			voxel_position.z + chunk_pos.y)
	surfacetool.add_color(get_color(voxel_pos.y))
	
	# Top
	if canCreateFace(Vector3(voxel_position.x, voxel_position.y + 1, voxel_position.z)):
		surfacetool.add_normal(Vector3(0, 1, 0))
		surfacetool.add_vertex(vertices[4] + voxel_pos)
		surfacetool.add_vertex(vertices[5] + voxel_pos)
		surfacetool.add_vertex(vertices[7] + voxel_pos)
		surfacetool.add_vertex(vertices[5] + voxel_pos)
		surfacetool.add_vertex(vertices[6] + voxel_pos)
		surfacetool.add_vertex(vertices[7] + voxel_pos)
	
	# Side right	
	if canCreateFace(Vector3(voxel_position.x + 1, voxel_position.y, voxel_position.z)) and canCreateFace(Vector3(voxel_position.x + 1, voxel_position.y + 1, voxel_position.z)):
		surfacetool.add_normal(Vector3(1, 0, 0))
		surfacetool.add_vertex(vertices[2] + voxel_pos)
		surfacetool.add_vertex(vertices[5] + voxel_pos)
		surfacetool.add_vertex(vertices[1] + voxel_pos)
		surfacetool.add_vertex(vertices[2] + voxel_pos)
		surfacetool.add_vertex(vertices[6] + voxel_pos)
		surfacetool.add_vertex(vertices[5] + voxel_pos)
	
	# Side Left	
	if canCreateFace(Vector3(voxel_position.x - 1, voxel_position.y, voxel_position.z)) and canCreateFace(Vector3(voxel_position.x - 1, voxel_position.y + 1, voxel_position.z)):
		surfacetool.add_normal(Vector3(-1, 0, 0))
		surfacetool.add_vertex(vertices[0] + voxel_pos)
		surfacetool.add_vertex(vertices[7] + voxel_pos)
		surfacetool.add_vertex(vertices[3] + voxel_pos)
		surfacetool.add_vertex(vertices[0] + voxel_pos)
		surfacetool.add_vertex(vertices[4] + voxel_pos)
		surfacetool.add_vertex(vertices[7] + voxel_pos)
	
	# Front face
	if canCreateFace(Vector3(voxel_position.x, voxel_position.y, voxel_position.z + 1)) and canCreateFace(Vector3(voxel_position.x, voxel_position.y + 1, voxel_position.z + 1)):
		surfacetool.add_normal(Vector3(0, 0, 1))
		surfacetool.add_vertex(vertices[3] + voxel_pos)
		surfacetool.add_vertex(vertices[6] + voxel_pos)
		surfacetool.add_vertex(vertices[2] + voxel_pos)
		surfacetool.add_vertex(vertices[3] + voxel_pos)
		surfacetool.add_vertex(vertices[7] + voxel_pos)
		surfacetool.add_vertex(vertices[6] + voxel_pos)
	
	# Back Face
	if canCreateFace(Vector3(voxel_position.x, voxel_position.y, voxel_position.z - 1)) and canCreateFace(Vector3(voxel_position.x, voxel_position.y + 1, voxel_position.z - 1)):
		surfacetool.add_normal(Vector3(0, 0, -1))
		surfacetool.add_vertex(vertices[0] + voxel_pos)
		surfacetool.add_vertex(vertices[1] + voxel_pos)
		surfacetool.add_vertex(vertices[5] + voxel_pos)
		surfacetool.add_vertex(vertices[5] + voxel_pos)
		surfacetool.add_vertex(vertices[4] + voxel_pos)
		surfacetool.add_vertex(vertices[0] + voxel_pos)

func canCreateFace(position):
	# Returns true if there is no block at $position.
	if cubes_in_chunk.find(position, 0) == -1:
		return true
	return false
		
func noise2d(position):
	# Noise function 2D.
	var noise_var1 = softnoise1.openSimplex2D(position.x / resolution1, position.y / resolution1)#softnoise1.openSimplex2D(position.x  / resolution1, position.y / resolution1)
	var noise_var2 = softnoise2.openSimplex2D(position.x / resolution2, position.y / resolution2)
	var noise_var3 = softnoise3.openSimplex2D(position.x / resolution3, position.y / resolution3)
	var noise_var4 = softnoise4.openSimplex2D(position.x / resolution4, position.y / resolution4)

	var result = (noise_var1 * amplitude1) + (noise_var2 * amplitude2) + (noise_var3 * amplitude3) + (noise_var4 * amplitude4)
	
	return stepify(result * amplify, cube_size) 
	
func noise3d(position):
	var noise_var1 = softnoise1.openSimplex3D(position.x, position.y, position.z)
	
	return noise_var1
func get_color(height):
	# Returns the color of the voxel at specified Height.
	if height <= level_water:
		return color_sand.lightened(height / 50)
	
	else:
		return color_grass.lightened(height / 50)
