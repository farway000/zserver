using System;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;
using ZMap.Renderer.SkiaSharp;
using ZMap.Style;

namespace ZServer.Tests
{
    public class SymbolStyleTests : BaseTests
    {
        [Fact]
        public void ImageSymbol()
        {
            var data = GetFeatures();

            var style = new SymbolStyle
            {
                Uri = Expression<Uri>.New(new Uri("file://108.png", UriKind.Absolute)),
                Size = Expression<int>.New(30)
            };

            var width = 512;
            var height = 512;
            var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
            var graphicsService =
                new SkiaGraphicsService(Guid.NewGuid().ToString(), width, height, Extent, memoryCache);

            foreach (var feature in data)
            {
                graphicsService.Render(style, feature);
            }

            var bytes = graphicsService.GetImage("image/png");
            File.WriteAllBytes($"images/{GetType().Name}_ImageSymbol.png", bytes);
        }
    }
}