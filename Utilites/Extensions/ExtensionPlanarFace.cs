using Autodesk.Revit.DB;

namespace MS.Utilites
{
    public static class ExtensionPlanarFace
    {
        /// <summary>
        /// Возвращает самый длинный контур грани.
        /// </summary>
        /// <param name="face">Плоская грань.</param>
        /// <returns>Самый длинный контур грани.</returns>
        public static EdgeArray GetMaxLengthEdgeArray(this PlanarFace face)
        {
            EdgeArrayArray edgeLoopArray = face.EdgeLoops;
            double loopMaxLength = 0;
            EdgeArray outerEdgeArray = new EdgeArray();
            foreach (EdgeArray loop in edgeLoopArray)
            {
                double loopLength = 0;
                foreach (Edge edge in loop)
                {
                    loopLength += edge.AsCurve().Length;
                }
                if (loopLength > loopMaxLength)
                {
                    loopMaxLength = loopLength;
                    outerEdgeArray = loop;
                }
            }
            return outerEdgeArray;
        }
    }
}
