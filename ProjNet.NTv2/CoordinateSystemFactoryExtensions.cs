
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
		/// <remarks>
		/// This method will examine the coordinate systems in order to construct
		/// a transformation between them. This method may fail if no path between 
		/// the coordinate systems is found, using the normal failing behavior of 
		/// the DCP (e.g. throwing an exception).</remarks>
		/// <param name="factory">The coordinate transformation factory.</param>
		/// <param name="sourceCS">Source coordinate system.</param>
		/// <param name="targetCS">Target coordinate system.</param>
		/// <param name="gridFile">The grid file.</param>
		/// <param name="inverse">Indicates whether to use the inverse grid transform.</param>
		/// <returns></returns>		
		public static ICoordinateTransformation CreateFromCoordinateSystems(this CoordinateTransformationFactory factory,
			CoordinateSystem sourceCS, CoordinateSystem targetCS, string gridFile, bool inverse)
		{
            if (sourceCS == null)
            {
				throw new ArgumentNullException(nameof(sourceCS));
            }

			if (targetCS == null)
			{
				throw new ArgumentNullException(nameof(targetCS));
			}

			if (sourceCS is ProjectedCoordinateSystem && targetCS is ProjectedCoordinateSystem)
			{
				var ct = factory.CreateFromCoordinateSystems((sourceCS as ProjectedCoordinateSystem), (targetCS as ProjectedCoordinateSystem));

				var list = GetCoordinateTransformationList(ct);

                if (list.Count != 3)
				{
					// Sanity check (should never happen).
					throw new NotSupportedException("No support for transforming between the two specified coordinate systems");
				}

				// list[0] = source projected  -> source geographic
				// list[1] = source geographic -> target geographic
				// list[2] = target geographic -> target projected

				// Replace the geographic transform in the middle with our grid transformation.
				list[1] = CreateCoordinateTransformation((ICoordinateTransformation)list[1], gridFile, inverse);

				return ct;
			}

			throw new NotSupportedException("No support for transforming between the two specified coordinate systems");
		}

		static IList<ICoordinateTransformationCore> GetCoordinateTransformationList(ICoordinateTransformation ct)
		{
			var assembly = ct.GetType().Assembly;
			var type = assembly.GetType("ProjNet.CoordinateSystems.Transformations.ConcatenatedTransform");

			var prop = type.GetProperty("CoordinateTransformationList");

			return (IList<ICoordinateTransformationCore>)prop.GetValue(ct.MathTransform);
		}

		static ICoordinateTransformation CreateCoordinateTransformation(ICoordinateTransformation ct, string file, bool inverse)
		{
			var assembly = ct.GetType().Assembly;
			var type = assembly.GetType("ProjNet.CoordinateSystems.Transformations.CoordinateTransformation");

			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

			var instance = Activator.CreateInstance(type, flags, null, new object[]
				{
					ct.SourceCS,
					ct.TargetCS,
					ct.TransformType,
					new GridTransformation(file, inverse),
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

        public GridTransformation(string file, bool inverse)
		{
			this.inverse = inverse;

			grid = GridFile.Open(file);
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
