using NetTopologySuite.Geometries;

namespace ZMap;

public interface IGraphicsServiceProvider
{
    IGraphicsService Create(string mapId, int width, int height, Envelope envelope);
}