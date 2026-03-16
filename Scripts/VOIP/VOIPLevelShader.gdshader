shader_type canvas_item;

uniform vec4 backgroundcolor : source_color = vec4(0.1, 0.2, 0.8, 1.0);
uniform vec4 voxThresholdColor : source_color = vec4(0.602, 0.43, 0.508, 1.0);
uniform vec4 voxchunkmaxcolor : source_color = vec4(0.8, 0.8, 0.08, 0.2);
uniform vec4 voxwavecolor : source_color = vec4(0.8,0.8,0.9,1.0);

uniform float voxThresHold = 0.2;
uniform float chunkmax = 0.0;
uniform float chunkMaxPersist = 0.0;
uniform float speechNoiseProbability = 0.0;
uniform bool chunkTextEnabled = false;
uniform sampler2D chunkTexture : repeat_enable;

void fragment() {
	if (UV.x > voxThresHold)
		COLOR = backgroundcolor;
	else if ((chunkmax != 0.0) || chunkTextEnabled)
		COLOR = voxThresholdColor;

	if ((UV.x < chunkMaxPersist) && (UV.y > 0.1) && (UV.y < 0.9))
		COLOR.rgb = mix(COLOR.rgb, voxchunkmaxcolor.rgb, voxchunkmaxcolor.a);

	if ((speechNoiseProbability > 0.0) && (UV.y > 0.9) && (abs(UV.x - 0.5) < speechNoiseProbability*0.5)) {
		COLOR.rgb = vec3(1.0,1.0,0);
	}

	float d;
	if (chunkTextEnabled) {
		vec4 v = texture(chunkTexture, UV);
		d = UV.y*2.0 - v.r*4.0 - 1.0;
	} else {
		d = (UV.x < chunkmax ? UV.y - 0.5 : 1.0);
	}
	if (abs(d) < 0.1)
		COLOR = voxwavecolor;
}
