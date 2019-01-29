extends Node

onready var generator = get_node("VoxelWorld")

var thread1 = Thread.new()
var thread2 = Thread.new()
var thread3 = Thread.new()

						#func _ready():
							#thread1.start(generator, "update_chunk")

