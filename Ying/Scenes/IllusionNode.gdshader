shader_type canvas_item;

// 🌟 琉璃白配方
uniform vec4 core_color : source_color = vec4(1.0, 1.0, 1.0, 1.0); // 核心纯白
uniform vec4 glow_color : source_color = vec4(0.85, 0.92, 1.0, 1.0); // 边缘极淡的冰蓝

uniform float spread : hint_range(0.0, 50.0) = 15.0;
uniform float outer_intensity : hint_range(0.0, 5.0) = 1.5;
uniform float inner_intensity : hint_range(0.0, 5.0) = 1.2;

uniform float pulse_speed : hint_range(0.0, 10.0) = 2.0;
uniform float pulse_boost : hint_range(0.0, 2.0) = 0.5;

void fragment() {
    vec4 original = texture(TEXTURE, UV);
    vec2 ps = TEXTURE_PIXEL_SIZE;

    // --- 1. 光晕衰减 ---
    float alpha_sum = 0.0;
    float weight_sum = 0.0;
    for(float x = -3.0; x <= 3.0; x += 1.0) {
        for(float y = -3.0; y <= 3.0; y += 1.0) {
            float dist = length(vec2(x, y));
            if(dist > 0.0) {
                float w = exp(-dist * 0.8);
                alpha_sum += texture(TEXTURE, UV + vec2(x, y) * spread * ps).a * w;
                weight_sum += w;
            }
        }
    }
    float blurred_alpha = pow(alpha_sum / weight_sum, 1.5);
    
    // --- 2. 呼吸计算 ---
    float breath = (sin(TIME * pulse_speed) + 1.0) / 2.0;
    float current_boost = breath * pulse_boost;
    
    // --- 3. 外层光晕 ---
    vec4 outer_layer = glow_color;
    outer_layer.a = clamp(blurred_alpha * (outer_intensity + current_boost), 0.0, 1.0);
    
    // --- 4. 本体层 (🌟 绝对不发黄的修复核心！) ---
    // 强行提取亮度
    float luma = dot(original.rgb, vec3(0.299, 0.587, 0.114));
    
    // 🌟 将原图彻底转换为只有黑白灰的变量，彻底抛弃 original.rgb 的色彩！
    vec3 pure_gray = vec3(luma); 
    
    vec3 mapped_color = mix(glow_color.rgb, core_color.rgb, luma);
    float current_inner = inner_intensity + current_boost;
    
    // 🌟 使用 pure_gray 参与合成，原图的任何暖色、杂色都在这里被彻底抹杀
    vec3 final_inner = 1.0 - (1.0 - pure_gray) * (1.0 - mapped_color * current_inner * luma);
    
    vec4 inner_layer = vec4(final_inner, original.a);
    
    // --- 5. 合成输出 ---
    COLOR = mix(outer_layer, inner_layer, inner_layer.a);
}