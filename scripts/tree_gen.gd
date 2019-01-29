extends Node

class tree_generator:
	const BARK_MAX_HEIGHT = 12
	const BARK_MAX_WIDTH = 5
	
	const LEAVES_MAX_HEIGHT = 12
	const LEAVES_MAX_WIDTH = 5
	
	var bark_height = 4
	var bark_width = 1
	
	var leaves_width = 5
	var leave_heigth = 2
	
	var voxels = []
	
	func _init():
		bark_height = 4
		bark_width = 1
		
	func new_tree():
		randomize()
		
		voxels = [Vector3(0,0,0),
				Vector3(0,-1,0),
				Vector3(0,-2,0),
				Vector3(0,-3,0),
				Vector3(0,-4,0),
				Vector3(0,-5,0),
				Vector3(0,-6,0)]
				
		#gen_bark()
	
		return voxels
		voxels.clear()

	func gen_bark():
		bark_height = randi() % BARK_MAX_HEIGHT
		clamp(bark_height, 1, BARK_MAX_HEIGHT)
		
		bark_width = randi() % BARK_MAX_WIDTH
		clamp(bark_width, 1, BARK_MAX_WIDTH)
		
		for x in bark_width:
			for y in bark_height:
				#voxels.append(Vector3(bark_width, -bark_height,bark_width))
				pass
	