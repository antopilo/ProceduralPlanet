extends KinematicBody

var input_direction = Vector3()
var speed = 10
func _ready():
	# Called when the node is added to the scene for the first time.
	# Initialization here
	pass

func _physics_process(delta):
	update_input()
	move_and_slide(Vector3(input_direction.z * speed, 
						input_direction.x * speed, 
						input_direction.z))
						
func update_input():
	if Input.is_action_just_pressed("forward"):
		input_direction.z += 1
	elif Input.is_action_just_released("forward"):
		input_direction.z -= 1
	
	if Input.is_action_just_pressed("backward"):
		input_direction.y -= 1
	elif Input.is_action_just_released("backward"):
		input_direction.y += 1
	