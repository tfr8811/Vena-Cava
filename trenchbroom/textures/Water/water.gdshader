shader_type spatial;

uniform vec3 water_color : source_color;
uniform sampler2D water_normal_noise;
uniform float time_scale : hint_range(0.0, 10.0, 0.25) = 1.0;

void fragment() {
	// Called for every pixel the material is visible on.

	vec2 _uv = UV;
	
	_uv.x += sin(TIME * time_scale + (_uv.x + _uv.y) * 25.0) * 0.01;
	_uv.y += cos(TIME + time_scale + (_uv.x + _uv.y) * 25.0) * 0.01;

	ALBEDO = water_color;
	NORMAL_MAP = texture(water_normal_noise, _uv).rgb;
	ROUGHNESS = .8;

	NORMAL *= .5;
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
