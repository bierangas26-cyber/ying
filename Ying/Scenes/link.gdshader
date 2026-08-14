shader_type canvas_item;

// 【关键修复】：加上 repeat_enable，允许贴图像滚轮一样无限循环滚动！
uniform sampler2D noise_texture : repeat_enable;

// 墨水颜色 (深黑，带一点透明度)
uniform vec4 ink_color : source_color = vec4(0.0, 0.0, 0.0, 0.9);

// 沸腾/流动的速度
uniform float flow_speed : hint_range(0.0, 5.0) = 1.0;

// 墨迹的粗细扩散
uniform float spread : hint_range(0.0, 1.0) = 0.5;

void fragment() {
    // 1. 让 UV 坐标随着时间 (TIME) 往下流动
    vec2 flow_uv = UV + vec2(0.0, TIME * flow_speed * 0.5);

    // 2. 读取水墨图，产生流动的噪点
    float noise_val = texture(noise_texture, flow_uv).r;

    // 3. 制造刷子形状
    float shape = abs(UV.x - 0.5) * 2.0;

    // 4. 加噪点产生毛边
    float final_shape = shape + (noise_val - 0.5) * spread;

    // 5. 裁剪透明度
    float alpha = smoothstep(0.6, 0.2, final_shape);

    COLOR = vec4(ink_color.rgb, ink_color.a * alpha);
}