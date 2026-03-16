extends Node

class_name VOIPRecord

@export var btnPlay : Button
@export var audioInput : VOIPInput
@export var audioOutput : Node

@export var frameCount : LineEdit
@export var bytesTotal : LineEdit
@export var bytesPerSec : LineEdit
@export var recordingLength : LineEdit

var recordedSamples = [ ]
const maxRecordedSamples = 10*50
var recordedOpusPackets = [ ]
var recordedOpusPacketsMemSize = 0
var recordedChunkMax = 0.0
var recordedReSampledPackets = null

var recordedHeader = { }
var recordedFooter = { }

func _ready() -> void:
	audioInput.transmitAudioPacket.connect(on_TransmitAudioPacket)
	audioInput.transmitAudioJsonPacket.connect(on_TransmitAudioJsonPacket)
	btnPlay.pressed.connect(_on_play_pressed);

func _on_play_pressed():
	recordedHeader.erase("opusframecount")
	audioOutput.replayRecording(1.0, recordedHeader, recordedOpusPackets, recordedFooter)


func on_TransmitAudioJsonPacket(audioStreamPacketHeader):
	print(audioStreamPacketHeader)
	if audioStreamPacketHeader.has("talkingTimeStart"):
		recordedSamples = [ ]
		recordedOpusPackets = [ ]
		recordedReSampledPackets = [ ]

		frameCount.text = str(0)
		recordingLength.text = str(0)
		recordedOpusPacketsMemSize = 0
		recordedChunkMax = 0.0
		bytesTotal.text = str(0)
		bytesPerSec.text = str(0)

		print("start talking")
		recordedHeader = audioStreamPacketHeader
		recordedFooter = { }
	else:
		recordedFooter = audioStreamPacketHeader
		assert(audioStreamPacketHeader.has("talkingtimeend"))
		print("recordedpacketsMemSize ", recordedOpusPacketsMemSize)
		print("Talked for ", audioStreamPacketHeader["talkingtimeduration"], " seconds")

func on_TransmitAudioPacket(opusPacket, _opusFrameCount):
	if len(recordedSamples) < maxRecordedSamples:
		recordOriginalChunks(audioInput.audio_chunk, audioInput.last_ChunkMax, opusPacket)

func recordOriginalChunks(audioSamples, chunkMax, opusPacket):
	recordedSamples.append(audioSamples)
	recordedOpusPackets.append(opusPacket)
	frameCount.text = str(len(recordedOpusPackets))
	recordedOpusPacketsMemSize += opusPacket.size()
	bytesTotal.text = str(recordedOpusPacketsMemSize)
	var tm = len(recordedOpusPackets)*audioInput.audio_chunk_size*1.0/AudioServer.get_input_mix_rate()
	recordingLength.text = str(tm)
	bytesPerSec.text = str(int(recordedOpusPacketsMemSize/tm))
	recordedChunkMax = max(recordedChunkMax, chunkMax)
	#$VBoxPlayback/HBoxStream/ChunkMax.text = str(recordedChunkMax)
