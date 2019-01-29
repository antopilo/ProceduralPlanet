extends Camera

const MOVE_SPEED = 1
const MOUSE_SENSITIVITY = 0.002
onready var speed = 0
onready var velocity = Vector3()
onready var initial_rotation = PI/2

func _enter_tree():
	# Capture the mouse (can be toggled by pressing F10)
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _input(event):
	# Horizontal mouse look
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		rotation.y -= event.relative.x*MOUSE_SENSITIVITY

	# Vertical mouse look, clamped to -90..90 degrees
	if event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		rotation.x = clamp(rotation.x - event.relative.y*MOUSE_SENSITIVITY, deg2rad(-90), deg2rad(90))

	# Toggle mouse capture
	if event.is_action_pressed("esc"):
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _physics_process(delta):
	# Speed modifier
	if Input.is_action_just_released("mwheelup"):
		speed *= 1.25
	elif Input.is_action_just_released("mwheeldown"):
		speed /= 1.25
	clamp(speed, 0.5, 5)
	# Movement

	if Input.is_action_pressed("w"):
		velocity.x -= MOVE_SPEED*speed*delta

	if Input.is_action_pressed("s"):
		velocity.x += MOVE_SPEED*speed*delta

	if Input.is_action_pressed("a"):
		velocity.z += MOVE_SPEED*speed*delta

	if Input.is_action_pressed("d"):
		velocity.z -= MOVE_SPEED*speed*delta

	if Input.is_action_pressed("e"):
		velocity.y += MOVE_SPEED*speed*delta

	if Input.is_action_pressed("q"):
		velocity.y -= MOVE_SPEED*speed*delta

	# Friction
	velocity *= 0.875

	# Apply velocity
	translation += velocity \
	.rotated(Vector3(0, 1, 0), rotation.y - initial_rotation) \
	.rotated(Vector3(1, 0, 0), cos(rotation.y)*rotation.x) \
	.rotated(Vector3(0, 0, 1), -sin(rotation.y)*rotation.x)

func _exit_tree():
	# Restore the mouse cursor upon quitting
	Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)