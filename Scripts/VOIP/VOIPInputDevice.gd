extends OptionButton

class_name  VOIPInputDevice

@export var btnMicEnabled: CheckButton

func _ready() -> void:
	btnMicEnabled.toggle_mode = true
	btnMicEnabled.button_pressed = true
	connect("item_selected", whenItemSelected)
	clear()
	for d in AudioServer.get_input_device_list():
		add_item(d)
	btnMicEnabled.connect("toggled", setMicState)
	setMicState(true);

func _process(_delta: float) -> void:
	if(Input.is_key_pressed(Key.KEY_F8)):
		visible = !visible

func whenItemSelected(index: int) -> void:
	# NOTE might have to stop recording from mic before swapping
	var input_device: String = get_item_text(index)
	print("Set input device: ", input_device)
	AudioServer.set_input_device(input_device)


	# Turn the input device on/off
func setMicState(recording: bool):
	if recording:
		var err = AudioServer.set_input_device_active(true)
		if err != OK:
			print("Mic input err: ", err)
			btnMicEnabled.set_pressed_no_signal(false)
	else:
		AudioServer.set_input_device_active(false)
