extends OptionButton

class_name  VOIPInputDevice

func _ready() -> void:
	connect("item_selected", whenItemSelected)
	clear()
	for d in AudioServer.get_input_device_list():
		add_item(d)
	##btnMicEnabled.connect("toggled", setMicState) TODO fix fix it
func _process(_delta: float) -> void:
	if(Input.is_key_pressed(Key.KEY_F8)):
		visible = !visible
func whenItemSelected(index: int) -> void:
	# NOTE might have to stop recording from mic before swapping
	var input_device: String = get_item_text(index)
	print("Set input device: ", input_device)
	AudioServer.set_input_device(input_device)
