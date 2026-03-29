extends Node


class_name VOIPChannel

@export var debug : bool = false
@export var input : VOIPInput
@export var output : VOIPOutput

func _ready() -> void:
	##print("peerid", get_parent().PeerID)
	var peerID : int = int(get_parent().PeerID)
	if(peerID != multiplayer.get_unique_id()):
		print("inputDevice skipped")
		return
	input = get_tree().root.get_node("Main/Core/MicIn")
	input.transmitAudioJsonPacket.connect(on_json_header)
	input.transmitAudioPacket.connect(on_audio_packet)

func on_json_header(audioStreamPacketHeader :Dictionary) -> void:
	if(!multiplayer.has_multiplayer_peer() or multiplayer.multiplayer_peer.get_connection_status() != MultiplayerPeer.CONNECTION_CONNECTED):
		return
	rpc("rpcSendHeader", audioStreamPacketHeader)

@rpc("any_peer", "reliable")
func rpcSendHeader(audioStreamPacketHeader : Dictionary) -> void:
	if(debug): print("Json Header/Footer received on peer -> ", multiplayer.get_unique_id())
	output.addAudioPacket(JSON.stringify(audioStreamPacketHeader).to_ascii_buffer())

func on_audio_packet(packet : PackedByteArray, opusFrameCount : int) -> void:
	if(!multiplayer.has_multiplayer_peer() or multiplayer.multiplayer_peer.get_connection_status() != MultiplayerPeer.CONNECTION_CONNECTED):
		return
	rpc("rpcSendPacket", packet, opusFrameCount)

@rpc("any_peer", "reliable")
func rpcSendPacket(packet : PackedByteArray, _opusFrameCount : int) -> void:
	if(debug): print("Packets received on peer -> ", multiplayer.get_unique_id())
	output.addAudioPacket(packet)
