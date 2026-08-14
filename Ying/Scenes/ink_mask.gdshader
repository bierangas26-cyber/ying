shader_type canvas_item;

// 你的黑白水墨遮罩图 (ink_mask.png)
uniform sampler2D ink_texture; 

// 动画进度 (从 0.0 到 1.0)
uniform float progress : hint_range(0.0, 1.0) = 0.0;

// 【核心参数】控制边缘的“燃烧/侵蚀”范围
uniform float burn_size : hint_range(0.0, 0.5) = 0.15;

// 【核心参数】前锋墨迹的颜色（纯黑，或者带点深蓝/深褐的水墨色）
uniform vec4 ink_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);

void fragment() {
    // 1. 获取新季节的原始图片颜色
    vec4 base_color = texture(TEXTURE, UV);
    
    // 2. 获取水墨遮罩图的灰度值 (0.0纯黑 -> 1.0纯白)
    // 你的图是从右到左渐白的，这天然就带有方向性！
    float mask_val = texture(ink_texture, UV).r;
    
    // --- 魔法开始：双层阈值法 ---
    
    // 3. 计算“前锋墨迹”层 (比主画面稍微超前一点)
    // 这里使用 step 函数，产生极其锐利、不规则的边缘！这是侵蚀感的来源！
    // 当 progress 推进时，mask_val 较小的（较黑的，也就是右边）区域会先变成 1.0
    float ink_layer = step(mask_val, progress + burn_size);
    
    // 4. 计算“主画面”层 (比墨迹层稍微落后一点)
    float image_layer = step(mask_val, progress);
    
    // --- 混合输出 ---
    
    // 5. 扣除主画面，只保留边缘那一圈“燃烧”的墨迹
    float edge_only = ink_layer - image_layer;
    
    // 6. 将墨迹的颜色和主画面混合
    // 如果是边缘 (edge_only = 1)，就显示纯黑水墨色；
    // 如果是内部 (image_layer = 1)，就显示新季节原本的颜色。
    vec3 final_rgb = mix(base_color.rgb, ink_color.rgb, edge_only);
    
    // 7. 最终的透明度：只要是墨迹层或者主画面层覆盖到的地方，都显示 (alpha=1)
    // 其他地方全透明 (alpha=0)，露出底下的旧季节。
    COLOR = vec4(final_rgb, base_color.a * ink_layer);
}