namespace ZMap.Style
{
    public class RasterStyle : Style
    {
        /// <summary>
        /// 绘制图片的不透明度。默认为 1.
        /// </summary>
        public Expression<float> Opacity { get; set; }

        /// <summary>
        /// 在色轮上旋转色相的角度。默认为 0.
        /// </summary>
        public Expression<float> HueRotate { get; set; }

        /// <summary>
        /// 增大或减少图片的亮度。此值是最小亮度。默认为 0.
        /// </summary>
        public Expression<float> BrightnessMin { get; set; }

        /// <summary>
        /// 增大或减少图片的亮度。此值是最大亮度。默认为 1.
        /// </summary>
        public Expression<float> BrightnessMax { get; set; }

        /// <summary>
        /// 增加或者减少图片的饱和度。默认为 0.
        /// </summary>
        public Expression<float> Saturation { get; set; }

        /// <summary>
        /// 增加或者减少图片的对比度。默认为 0.
        /// </summary>
        public Expression<float> Contrast { get; set; }
    }
}