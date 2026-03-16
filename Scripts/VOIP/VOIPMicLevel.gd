extends ColorRect

class_name VOIPMicLevel

@export var debug : bool = false
@export var hangFrames : int = 25
@export var chunkTextEnabled : bool = false

var audioChunkSize = 882 # also declared and used in VOIPInput

var frameImage : Image
var frameTexture : ImageTexture
var hangFramesCountup = 0
var chunkMaxPersist = 0.0


func _ready() -> void:
	var frameData : PackedVector2Array = PackedVector2Array()
	frameData.resize(audioChunkSize)
	for j in range(audioChunkSize):
		frameData.set(j, Vector2(-0.5,0.9) if (j%10)<5 else Vector2(0.6,0.1))
	frameImage = Image.create_from_data(audioChunkSize, 1, false, Image.FORMAT_RGF, frameData.to_byte_array())
	frameTexture = ImageTexture.create_from_image(frameImage)
	material = material.duplicate();
	material.set_shader_parameter("chunkTexture", frameTexture) # checked


func process_vox(chunkMax : float, audioChunk : PackedVector2Array, voxThresHold : float, withDeNoise : bool):
	if(debug): print("Processing chunk for visuals: chunkMax[", chunkMax, "] voxThresHold[", voxThresHold, "] audioChunk.Size[", audioChunk.size(), "] withDeNoise[", withDeNoise, "]")
	material.set_shader_parameter("voxThresHold", voxThresHold) # checked
	if withDeNoise:
		material.set_shader_parameter("speechNoiseProbability", chunkMax) # checked
	material.set_shader_parameter("chunkMax", chunkMax)
	if chunkMax >= voxThresHold:
		hangFramesCountup = 0
		if chunkMax > chunkMaxPersist:
			chunkMaxPersist = chunkMax
			material.set_shader_parameter("chunkMaxPersist", chunkMaxPersist) # checked
	else:
		if hangFramesCountup == hangFrames:
			chunkMaxPersist = 0.0
			material.set_shader_parameter("chunkMaxPersist", chunkMaxPersist) # checked
		hangFramesCountup += 1

	if chunkTextEnabled:
		material.set_shader_parameter("chunkTextEnabled", true) # checked
		frameImage.set_data(audioChunkSize, 1, false, Image.FORMAT_RGF, audioChunk.to_byte_array())
		frameTexture.update(frameImage)
	else:
		material.set_shader_parameter("chunkTextEnabled", false) # checked

