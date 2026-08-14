shader_type canvas_item;

uniform sampler2D direction_gradient : hint_default_white;
uniform sampler2D noise_tex : hint_default_white;
uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform float edge_softness : hint_range(0.01, 1.0) = 0.1;

// 控制风的拉伸比例：X 越小，横向拉得越长；Y 越大，纵向压得越扁
uniform vec2 noise_stretch = vec2(0.1, 2.0); 

void fragment() {
    vec4 base_color = texture(TEXTURE, UV);
    float grad_val = texture(direction_gradient, UV).r;
    
    // 将 UV 乘以拉伸系数，制造拉长撕裂感
    float noise_val = texture(noise_tex, UV * noise_stretch).r;
    
    float threshold = grad_val + (noise_val * edge_softness);
    
    // 当混合值小于进度时，变透明
    if (threshold < progress * (1.0 + edge_softness)) {
        base_color.a = 0.0;
    }
    
    COLOR = base_color;
}