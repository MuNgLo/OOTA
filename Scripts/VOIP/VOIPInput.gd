extends Node

class_name VOIPInput

@export_group("Settings")
@export var microphone_gain: float = 1.0
@export var deNoise: bool: set = SetDeNoise
@export var voiceCapture: bool = false
@export var voiceThreshold: float = 0.07
@export var opusFrameDurationMS: int = 20

static var micLevel: VOIPMicLevel

var opusEncoder: TwovoipOpusEncoder = TwovoipOpusEncoder.new()
var chunkPrefix: PackedByteArray = PackedByteArray([0, 0])

# when true this input is recording packets
var talking: bool: set = SetTalking
var currentlyTalking : bool = false
var opusFrameCount: int = 0
var opusStreamCount: int = 0

func SetTalking(value : bool) -> void:
	if(talking == value): return
	talking = value;
	#print("Talking flag is now -> ", talking)

const rootMeanSquareMaxMeasurement: bool = true

var microphoneAudioSamplesCountSeconds: float = 0.0
var microphoneAudioSamplesCount: int = 0
var microphoneAudioSamplesCountSecondsSampleWindow: float = 10.0

@export_group("Runtime set")
@export var opus_chunk_size: int = 960
@export var frameTimeSecs: float = 0.02

var talkingTimeStart: float = 0
var audio_chunk_size: int = 882
var opusBitRate: int = 12000
var opusComplexity: int = 5
var opusSampleRate: int = 48000
var opusChannels: int = 2

var hangFrames = 25
var hangFramesCountUp = 0

var audio_chunk: PackedVector2Array = []
var last_ChunkMax: float = 0.0

signal transmitAudioPacket(packet: PackedByteArray, opusFrameCount: int)
signal transmitAudioJsonPacket(audioStreamPacketHeader: Dictionary)

func _ready() -> void:
	#print("AudioServer.get_mix_rate()=", AudioServer.get_mix_rate())
	#print("ProjectSettings.get_setting_with_override(\"audio/driver/mix_rate\")=", ProjectSettings.get_setting_with_override("audio/driver/mix_rate"))
	opusEncoder.create_sampler(int(AudioServer.get_input_mix_rate()), opusSampleRate, opusChannels, deNoise)
	opusEncoder.create_opus_encoder(opusBitRate, opusComplexity, deNoise)
	
	opus_chunk_size = int(opusSampleRate * opusFrameDurationMS / 1000.0)
	audio_chunk_size = opusEncoder.calc_audio_chunk_size(opus_chunk_size)
	frameTimeSecs = opusFrameDurationMS / 1000.0

func _process(delta):
	microphoneAudioSamplesCountSeconds += delta
	startAndEndStream() # adds header and footer
	while true:
		audio_chunk = AudioServer.get_input_frames(opusEncoder.calc_audio_chunk_size(opus_chunk_size))
		if len(audio_chunk) == 0:
			break
		# get peak volume of the chunk
		last_ChunkMax = opusEncoder.process_pre_encoded_chunk(audio_chunk, opus_chunk_size, deNoise, rootMeanSquareMaxMeasurement)# TODO SPEACH prob???
		microphoneAudioSamplesCount += len(audio_chunk)
		if microphoneAudioSamplesCountSeconds > microphoneAudioSamplesCountSecondsSampleWindow:
			print("measured mic audio samples rate ", microphoneAudioSamplesCount / microphoneAudioSamplesCountSeconds)
			microphoneAudioSamplesCount = 0
			microphoneAudioSamplesCountSeconds = 0.0
			microphoneAudioSamplesCountSecondsSampleWindow *= 1.5
		# Detect voice and start stream
		if(voiceCapture):
			if(last_ChunkMax >= voiceThreshold):
				if( voiceCapture and !talking):
					talking = true
					hangFramesCountUp = 0
			else:
				hangFramesCountUp += 1
				if(hangFramesCountUp > hangFrames):
					talking = false
					hangFramesCountUp = 0
		# Send data to the image rect for visual feedback
		if(micLevel != null): micLevel.process_vox(last_ChunkMax, audio_chunk, voiceThreshold, deNoise)
		if currentlyTalking:
			process_opus_chunk()
	audio_chunk = []



func startAndEndStream():
	# Start the stream with a header
	if talking and not currentlyTalking:
		talkingTimeStart = Time.get_ticks_msec() * 0.001
		#hangFrames = ceil(hangTime / frameTimeSecs)
		var audioStreamPacketHeader: Dictionary = {
			"opusframesize": opus_chunk_size,
			"opusSampleRate": opusSampleRate,
			"opusChannels": opusChannels,
			"lenchunkprefix": len(chunkPrefix),
			"opusStreamCount": opusStreamCount,
			"talkingTimeStart": talkingTimeStart
		}
		opusEncoder.reset_opus_encoder()
		transmitAudioJsonPacket.emit(audioStreamPacketHeader)
		opusFrameCount = 0
		currentlyTalking = true
	#end stream with a footer
	elif not talking and currentlyTalking:
		currentlyTalking = false
		var talkingTimeEnd = Time.get_ticks_msec() * 0.001
		var talkingTimeDuration = talkingTimeEnd - talkingTimeStart
		var audioStreamPacketFooter = {
			"opusStreamCount": opusStreamCount,
			"opusFrameCount": opusFrameCount,
			"talkingtimeduration": talkingTimeDuration,
			"talkingtimeend": talkingTimeEnd
		}
		#print("My voice chunktime=", talkingTimeDuration / opusFrameCount, " over ", talkingTimeDuration, " seconds")
		transmitAudioJsonPacket.emit(audioStreamPacketFooter)
		opusStreamCount += 1


func process_opus_chunk():
	#print("Processing opus chunk")
	assert(currentlyTalking)
	if len(chunkPrefix) == 2:
		chunkPrefix.set(0, (opusFrameCount % 256)) # 32768 frames is 10 minutes
		@warning_ignore("integer_division")
		chunkPrefix.set(1, (int(opusFrameCount / 256) & 127) + (opusStreamCount % 2) * 128)
	else:
		assert(len(chunkPrefix) == 0)
	var oPacket: PackedByteArray = opusEncoder.encode_chunk(chunkPrefix, microphone_gain)
	transmitAudioPacket.emit(oPacket, opusFrameCount)
	opusFrameCount += 1

func SetDeNoise(value : bool) -> void:
	if(deNoise == value): return
	deNoise = value
	opusEncoder.create_sampler(int(AudioServer.get_input_mix_rate()), opusSampleRate, opusChannels, deNoise)
	opusEncoder.create_opus_encoder(opusBitRate, opusComplexity, deNoise)
	#print("Opus encoder rebuilt!")