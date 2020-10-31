
namespace ProjNet.CoordinateSystems.Transformations
{
    using ProjNet.CoordinateSystems;
    using ProjNet.NTv2;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    public static class CoordinateTransformationFactoryExtensions
    {
        #region Grid transformation

        /// <summary>
        /// Creates a transformation between two coordinate systems.
        /// </summary>
        /// <param name="factory">The coordinate transformation factory.</param>
        /// <param name="sourceCS">Source coordinate system.</param>
        /// <param name="targetCS">Target coordinate system.</param>
        /// <param name="gridFile">The grid file path.</param>
        /// <param name="inverse">Indicates whether to use the inverse grid transform.</param>
        /// <returns></returns>		
        public static ICoordinateTransformation CreateFromCoordinateSystems(this CoordinateTransformationFactory factory,
            CoordinateSystem sourceCS, CoordinateSystem targetCS, string gridFile, bool inverse)
        {
            return CreateFromCoordinateSystems(factory, sourceCS, targetCS, GridFile.Open(gridFile), inverse);
        }

        /// <summary>
        /// Creates a transformation between two coordinate systems.
        /// </summary>
        /// <param name="factory">The coordinate transformation factory.</param>
        /// <param name="sourceCS">Source coordinate system.</param>
        /// <param name="targetCS">Target coordinate system.</param>
        /// <param name="grid">The grid file.</param>
        /// <param name="inverse">Indicates whether to use the inverse grid transform.</param>
        /// <returns></returns>		
        public static ICoordinateTransformation CreateFromCoordinateSystems(this CoordinateTransformationFactory factory,
            CoordinateSystem sourceCS, CoordinateSystem targetCS, GridFile grid, bool inverse)
        {
            if (sourceCS == null)
            {
                throw new ArgumentNullException(nameof(sourceCS));
            }

            if (targetCS == null)
            {
                throw new ArgumentNullException(nameof(targetCS));
            }

            var ct = factory.CreateFromCoordinateSystems(sourceCS, targetCS);

            if (sourceCS is ProjectedCoordinateSystem && targetCS is ProjectedCoordinateSystem)
            {
                var list = GetCoordinateTransformationList(ct);

                if (list.Count != 3)
                {
                    throw new NotSupportedException("No support for grid transformation.");
                }

                // Replace the geographic transform in the middle with our grid transformation.
                list[1] = CreateGridTransformation(list[1].SourceCS, list[1].TargetCS, grid, inverse);
            }
            else if (sourceCS is GeographicCoordinateSystem source && targetCS is GeographicCoordinateSystem target)
            {
                return CreateGridTransformation(source, target, grid, inverse);
            }
            else if (sourceCS is GeographicCoordinateSystem && targetCS is ProjectedCoordinateSystem)
            {
                var list = GetCoordinateTransformationList(ct);

                // list[0] = source geographic -> geocentric
                // list[1] =        geocentric -> target projected

                // Replace the geographic transform with our grid transformation.
                list[0] = CreateGridTransformation(list[0].SourceCS, list[0].TargetCS, grid, inverse);
            }
            else if (sourceCS is ProjectedCoordinateSystem && targetCS is GeographicCoordinateSystem)
            {
                var list = GetCoordinateTransformationList(ct);

                // list[0] = source projected -> geocentric
                // list[1] =        geocentric -> target geographic

                // Replace the geographic transform with our grid transformation.
                list[1] = CreateGridTransformation(list[1].SourceCS, list[1].TargetCS, grid, inverse);
            }
            else
            {
                throw new NotSupportedException("No support for grid transformation.");
            }

            return ct;
        }

        #endregion

        static IList<ICoordinateTransformationCore> GetCoordinateTransformationList(ICoordinateTransformation ct)
        {
            var assembly = ct.GetType().Assembly;
            var type = assembly.GetType("ProjNet.CoordinateSystems.Transformations.ConcatenatedTransform");

            var prop = type.GetProperty("CoordinateTransformationList");

            return (IList<ICoordinateTransformationCore>)prop.GetValue(ct.MathTransform);
        }

        static ICoordinateTransformation CreateGridTransformation(CoordinateSystem sourceCS, CoordinateSystem targetCS, GridFile grid, bool inverse)
        {
            var assembly = sourceCS.GetType().Assembly;
            var type = assembly.GetType("ProjNet.CoordinateSystems.Transformations.CoordinateTransformation");

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var instance = Activator.CreateInstance(type, flags, null, new object[]
                {
                    sourceCS,
                    targetCS,
                    TransformType.Other,
                    new GridTransformation(grid, inverse),
                    "", "", -1, "", ""
                }, CultureInfo.CurrentCulture);

            return (ICoordinateTransformation)instance;
        }
    }
}
