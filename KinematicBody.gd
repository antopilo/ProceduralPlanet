extends KinematicBody

var camera_angle = 0
var mouse_sensitivity = 1
var camera_change = Vector2()

var velocity = Vector3()
var direction = Vector3()

#fly variables
const FLY_SPEED = 20
const FLY_ACCEL = 4
var flying = false

#walk variables
var gravity = -9.8 * 3
const MAX_SPEED = 5
const MAX_RUNNING_SPEED = 7
const ACCEL = 15
const DEACCEL = 12

#jumping
var jump_height = 9
var in_air = 0
var has_contact = false

#slope variables
const MAX_SLOPE_ANGLE = 35

#stair variables
const MAX_STAIR_SLOPE = 20
const STAIR_JUMP_HEIGHT = 6

var mouse_captured = true

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	pass

func _physics_process(delta):
	aim()
	if flying:
		fly(delta)
	else:
		walk(delta)

func _input(event):
	if event is InputEventMouseMotion:
		camera_change = event.relative
		
func walk(delta):
	# reset the direction of the player
	direction = Vector3()
	
	# get the rotation of the camera
	var aim = get_node("Head").get_global_transform().basis
	# check input and change direction
	if Input.is_action_pressed("w"):
		direction -= aim.z
	if Input.is_action_pressed("s"):
		direction += aim.z
	if Input.is_action_pressed("a"):
		direction -= aim.x
	if Input.is_action_pressed("d"):
		direction += aim.x
	if Input.is_action_just_pressed("esc"):
		if mouse_captured:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	direction.y = 0
	direction = direction.normalized()
	if Input.is_action_pressed("q"):
		flying = !flying
	
	velocity.y += gravity * delta
	
	
	var temp_velocity = velocity
	temp_velocity.y = 0
	
	var speed
	if Input.is_action_pressed("shift"):
		speed = MAX_RUNNING_SPEED
	else:
		speed = MAX_SPEED
	
	
	# where would the player go at max speed
	var target = direction * speed
	
	var acceleration
	if direction.dot(temp_velocity) > 0:
		acceleration = ACCEL
	else:
		acceleration = DEACCEL
	
	# calculate a portion of the distance to go
	temp_velocity = temp_velocity.linear_interpolate(target, acceleration * delta)
	
	velocity.x = temp_velocity.x
	velocity.z = temp_velocity.z
	
	if Input.is_action_just_pressed("jump"):
		velocity.y = jump_height
		has_contact = false
	
	# move
	velocity = move_and_slide(velocity, Vector3(0, 1, 0))
	
	
func fly(delta):
	# reset the direction of the player
	direction = Vector3()
	
	# get the rotation of the camera
	var aim = get_node("Head/Camera").get_global_transform().basis
	
	# check input and change direction
	if Input.is_action_pressed("move_forward"):
		direction -= aim.z
	if Input.is_action_pressed("move_backward"):
		direction += aim.z
	if Input.is_action_pressed("move_left"):
		direction -= aim.x
	if Input.is_action_pressed("move_right"):
		direction += aim.x
	
	direction = direction.normalized()
	
	# where would the player go at max speed
	var target = direction * FLY_SPEED
	
	# calculate a portion of the distance to go
	velocity = velocity.linear_interpolate(target, FLY_ACCEL * delta)
	
	# move
	move_and_slide(velocity)
	
func aim():

	get_node("Head").rotate_y(deg2rad(-camera_change.x * mouse_sensitivity))

	var change = -camera_change.y * mouse_sensitivity
	if change + camera_angle < 90 and change + camera_angle > -90:
		get_node("Head/Camera").rotate_x(deg2rad(change))
		camera_angle += change
	camera_change = Vector2()


func _on_Area_body_entered( body ):
	if body.name == "Gary":
		flying = true


func _on_Area_body_exited( body ):
	if body.name == "Gary":
		flying = false