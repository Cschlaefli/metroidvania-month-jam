extends Node2D

var spawn_points = []
var debug := true
export var spawn_at = 0


func _ready():
	add_to_group("area")
	for point in $Spawns.get_children() :
		spawn_points.append(point.global_position)

	if debug :
		var p = Globals.PLAYER.instance()
		p.global_position = spawn_points[spawn_at]
		add_child(p)

	Globals.current_area = self


func _save():
	var save_nodes = get_tree().get_nodes_in_group("persist")
	var dict = {}
	for node in save_nodes :
		dict[get_path_to(node)] = {"persist" : node.persist}

	return dict

func _load(dict := {}):
	for node_path in dict.keys() :
		var per = get_node(node_path)
		if per : per.set_persist(dict[node_path].persist)

