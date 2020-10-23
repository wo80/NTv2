
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

            ICoordinateTransformation ct;

            if (sourceCS is ProjectedCoordinateSystem && targetCS is ProjectedCoordinateSystem)
            {
                ct = factory.CreateFromCoordinateSystems(sourceCS as ProjectedCoordinateSystem, targetCS as ProjectedCoordinateSystem);

                var list = GetCoordinateTransformationList(ct);

                // list[0] = source projected  -> source geographic
                // list[1] = source geographic -> target geographic
                // list[2] = target geographic -> target projected

                // Replace the geographic transform in the middle with our grid transformation.
                list[1] = CreateCoordinateTransformation((ICoordinateTransformation)list[1], grid, inverse);
            }
            else if (sourceCS is GeographicCoordinateSystem && targetCS is GeographicCoordinateSystem)
            {
                ct = factory.CreateFromCoordinateSystems(sourceCS as GeographicCoordinateSystem, targetCS as GeographicCoordinateSystem);

                var list = GetCoordinateTransformationList(ct);

                // list[0] = source geographic -> geocentric
                // list[1] =        geocentric -> target geographic

                // Replace the geographic transform in the middle with our grid transformation.
                list[0] = CreateCoordinateTransformation((ICoordinateTransformation)list[0], grid, inverse);

                list.RemoveAt(1);
            }
            else
            {
                throw new NotSupportedException("No support for transforming between the two specified coordinate systems");
            }


            return ct;
        }

        static IList<ICoordinateTransformationCore> GetCoordinateTransformationList(ICoordinateTransformation ct)
        {
            var assembly = ct.GetType().Assembly;
            var type = assembly.GetType("ProjNet.CoordinateSystems.Transformations.ConcatenatedTransform");

            var prop = type.GetProperty("CoordinateTransformationList");

            return (IList<ICoordinateTransformationCore>)prop.GetValue(ct.MathTransform);
        }

        static ICoordinateTransformation CreateCoordinateTransformation(ICoordinateTransformation ct, GridFile grid, bool inverse)
        {
            var assembly = ct.GetType().Assembly;
            var type = assembly.GetType("ProjNet.CoordinateSystems.Transformations.CoordinateTransformation");

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var instance = Activator.CreateInstance(type, flags, null, new object[]
                {
                    ct.SourceCS,
                    ct.TargetCS,
                    ct.TransformType,
                    new GridTransformation(grid, inverse),
                    ct.Name,
                    ct.Authority,
                    ct.AuthorityCode,
                    ct.AreaOfUse,
                    ct.Remarks
                }, CultureInfo.CurrentCulture);

            return (ICoordinateTransformation)instance;
        }
    }

    class GridTransformation : MathTransform
    {
        private bool inverse;

        private GridFile grid;

        public GridTransformation(GridFile grid, bool inverse)
        {
            this.inverse = inverse;
            this.grid = grid;
        }

        public override int DimSource => 2;

        public override int DimTarget => 2;

        public override string WKT => "";

        public override string XML => "";

        public override MathTransform Inverse()
        {
            throw new NotImplementedException();
        }

        public override void Invert()
        {
            throw new NotImplementedException();
        }

        public override void Transform(ref double x, ref double y, ref double z)
        {
            grid.Transform(ref x, ref y, inverse);
        }
    }
}
