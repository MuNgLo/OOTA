extends Node

class_name VOIPOutput

@export var audioPlayer : AudioStreamPlayer = null

var audioStreamOpus : AudioStreamOpus = null
var audioStreamPlaybackOpus : AudioStreamPlaybackOpus = null

var audioServerOutputLatency = AudioServer.get_output_latency()
@export var audioBufferLagTimeTarget = 0.6
@export var audioBufferLagTimeTargetTolerance = 0.35

const asciiopenbrace = 123 # "{".to_ascii_buffer()[0]
const asciiclosebrace = 125 # "}".to_ascii_buffer()[0]
var lenchunkprefix = -1
var opusstreamcount = 0
var opusframecount = 0
var opusframesize = 960
const Noutoforderqueue = 4
const Npacketinitialbatching = 2
var outoforderchunkqueue = [ ]
var opusframequeuecount = 0

var playbackPausedOnMark = false

var lastEmittedAudioBufferPitchScale = 1.0
var runningLagTimeMinimum = -1.0


func _ready():
	if audioPlayer.has_method("set_stream"):
		audioStreamOpus = AudioStreamOpus.new()
		audioPlayer.set_stream(audioStreamOpus)
	else:
		audioPlayer = null
		printerr("No audio stream player")


var playingRecording = false
var pauseReached = false
var prevSkips = 0

func _physics_process(_delta):
	if audioStreamPlaybackOpus == null:
		return
	if playingRecording:
		return
	var queueLengthFrames = audioStreamPlaybackOpus.queue_length_frames()
	if not pauseReached and queueLengthFrames == 0:
		pauseReached = true
		var currSkips = audioStreamPlaybackOpus.get_skips(false)
		print("Skips during playback: ", currSkips - prevSkips)
		prevSkips = currSkips
		
	var bufferLengthTime = audioServerOutputLatency + queueLengthFrames*1.0/audioStreamOpus.opus_sample_rate
	if not playbackPausedOnMark:
		runningLagTimeMinimum = bufferLengthTime
		if abs(bufferLengthTime - audioBufferLagTimeTarget) > audioBufferLagTimeTargetTolerance:
			setPitchScale(0.7 if (bufferLengthTime < audioBufferLagTimeTarget) else 1.4)
			print(" set lastEmittedAudioBufferPitchScale to ", lastEmittedAudioBufferPitchScale)


func setrecopusvalues(opus_sample_rate, opus_channels):
	if not audioPlayer.playing or audioStreamOpus.opus_sample_rate != opus_sample_rate or audioStreamOpus.opus_channels != opus_channels:
		prints(":newplay: ", audioPlayer.playing, audioStreamOpus.opus_sample_rate, opus_sample_rate, audioStreamOpus.opus_channels, opus_channels)
		audioStreamOpus.opus_sample_rate = opus_sample_rate
		audioStreamOpus.opus_channels = opus_channels
		audioPlayer.play()  # creates a new playback
		audioStreamPlaybackOpus = audioPlayer.get_stream_playback()
		set_sinewave_out(sinewaveoutmode)
		# begins in a paused state
		# audioStreamPlaybackOpus.mark_end_opus_stream(false)
		playbackPausedOnMark = true
		pauseReached = false

func unpausewhenbufferready():
	assert (playbackPausedOnMark)
	var bufferlengthtime = audioServerOutputLatency + audioStreamPlaybackOpus.queue_length_frames()*1.0/audioStreamOpus.opus_sample_rate
	if bufferlengthtime > audioBufferLagTimeTarget:
		audioStreamPlaybackOpus.mark_end_opus_stream(true)
		playbackPausedOnMark = false
		runningLagTimeMinimum = bufferlengthtime

func addAudioPacket(packet):
	if audioStreamOpus == null:
		return
	if len(packet) <= 3:
		print("Bad packet too short")
	elif packet[0] == asciiopenbrace and packet[-1] == asciiclosebrace:
		var h = JSON.parse_string(packet.get_string_from_ascii())
		if h != null:
			print("audio json packet ", h)

			if h.has("talkingTimeStart"): # checked
				setrecopusvalues(h["opusSampleRate"], h.get("opusChannels", 2)) # checked
				lenchunkprefix = int(h["lenchunkprefix"])
				opusstreamcount = int(h["opusStreamCount"]) # checked
				opusframesize = int(h["opusframesize"])
				opusframecount = 0
				if h.has("opusFrameCount"):
					prints("Mid speech header!!! ", h["opusFrameCount"])
					opusframecount = int(h["opusFrameCount"]) + 1
				outoforderchunkqueue.clear()
				for i in range(Noutoforderqueue):
					outoforderchunkqueue.push_back(null)
				opusframequeuecount = 0
				assert (Npacketinitialbatching < Noutoforderqueue)
				runningLagTimeMinimum = -1.0

			elif h.has("talkingtimeend"):
				if playbackPausedOnMark and audioStreamPlaybackOpus.queue_length_frames() == 0:
					audioStreamPlaybackOpus.mark_end_opus_stream(true)
				audioStreamPlaybackOpus.mark_end_opus_stream(false)
				playbackPausedOnMark = true
				pauseReached = false
				print("runningLagTimeMinimum: ", runningLagTimeMinimum, " (target: ", audioBufferLagTimeTarget, ")")

	elif lenchunkprefix == -1:
		pass

	elif lenchunkprefix == 0:
		audioStreamOpus.push_opus_packet(packet, lenchunkprefix, 0)
		opusframecount += 1
		if playbackPausedOnMark:
			unpausewhenbufferready()

	elif packet[1]&128 == (opusstreamcount%2)*128:
		assert (lenchunkprefix == 2)
		var opusframecountI = packet[0] + (packet[1]&127)*256
		var opusframecountR = opusframecountI - opusframecount
		if opusframecountR < 0:
			if opusframecountR < -30000:
				print("framecount Wrapround 10mins? ", opusframecount, " ", opusframecountI)
				opusframecount = opusframecountI
				opusframecountR = 0
			else:
				print("late arriving frame ignored ", opusframecountR)
			
		if opusframecountR >= 0:
			while opusframecountR >= Noutoforderqueue:
				print("shifting outoforderqueue ", opusframecountI, " ", ("null" if outoforderchunkqueue[0] == null else len(outoforderchunkqueue[0])))
				if outoforderchunkqueue[0] != null:
					audioStreamPlaybackOpus.push_opus_packet(outoforderchunkqueue[0], lenchunkprefix, 0)
					opusframequeuecount -= 1
				else:
					var nextvalidpacketforfec = packet
					for i in range(1, Noutoforderqueue):
						if outoforderchunkqueue[i] != null:
							nextvalidpacketforfec = outoforderchunkqueue[i]
							break
					audioStreamPlaybackOpus.push_opus_packet(nextvalidpacketforfec, lenchunkprefix, 1)
				outoforderchunkqueue.pop_front()
				outoforderchunkqueue.push_back(null)
				opusframecountR -= 1
				opusframecount += 1
				assert (opusframequeuecount >= 0)

			outoforderchunkqueue[opusframecountR] = packet
			opusframequeuecount += 1
			while outoforderchunkqueue[0] != null and opusframecount + opusframequeuecount >= Npacketinitialbatching:
				if opusframesize > audioStreamPlaybackOpus.available_space_frames():
					print("!!! segment space filled up")
					break
				audioStreamPlaybackOpus.push_opus_packet(outoforderchunkqueue.pop_front(), lenchunkprefix, 0)
				outoforderchunkqueue.push_back(null)
				opusframecount += 1
				opusframequeuecount -= 1
				assert (opusframequeuecount >= 0)

		if playbackPausedOnMark:
			unpausewhenbufferready()

	else:
		prints("dropping frame with opusstream number mismatch", opusstreamcount, packet[0], packet[1])

func setPitchScale(pitchscale):
	if pitchscale != lastEmittedAudioBufferPitchScale:
		#audioPlayer.pitch_scale = pitchscale
		lastEmittedAudioBufferPitchScale = pitchscale



func replayRecording(speedup, recordedheader, recordedopuspackets, recordedfooter):
	playingRecording = true
	addAudioPacket(JSON.stringify(recordedheader).to_ascii_buffer())
	setPitchScale(speedup)
	for x in recordedopuspackets:
		if recordedheader["opusframesize"] > audioStreamPlaybackOpus.available_space_frames():
			var tmm = audioStreamPlaybackOpus.queue_length_frames()*0.5/audioStreamOpus.opus_sample_rate
			await get_tree().create_timer(tmm).timeout
		addAudioPacket(x)
	addAudioPacket(JSON.stringify(recordedfooter).to_ascii_buffer())
	playingRecording = false

var sinewaveoutmode = false
func set_sinewave_out(toggled_on):
	sinewaveoutmode = toggled_on
	if audioStreamPlaybackOpus:
		audioStreamPlaybackOpus.set_sinewave_frames(audioStreamOpus.opus_sample_rate/440 if toggled_on else 0, 0.05)
