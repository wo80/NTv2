using NUnit.Framework;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.NTv2.Tests
{
    [DefaultFloatingPointTolerance(1e-2)]
    public class CoordinateTransformTest
    {
        CoordinateTransformationFactory TransformationFactory;
        GridFile Grid;

        [SetUp]
        public void Setup()
        {
            TransformationFactory = new CoordinateTransformationFactory();
            Grid = GridFile.Open(GetType().Assembly.GetManifestResourceStream("ProjNet.NTv2.Tests.BETA2007.gsb"), true);
        }

        [Test]
        public void TestForwardTransformation()
        {
            var GK = SRIDReader.GetCSbyID(31466); // DHDN (3-degree Gauss-Kruger zone 2)
            var ETRS89 = SRIDReader.GetCSbyID(25832); // ETRS89 (UTM Zone 32N)

            var ct = TransformationFactory.CreateFromCoordinateSystems(GK, ETRS89, Grid, false);

            double[] input = new[] { 2558093.045, 5746759.212 };
            double[] expected = new[] { 351405.315, 5746765.219 };

            var actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void TestInverseTransformation()
        {
            var ETRS89 = SRIDReader.GetCSbyID(25832); // ETRS89 (UTM Zone 32N)
            var GK = SRIDReader.GetCSbyID(31466); // DHDN (3-degree Gauss-Kruger zone 2)

            var ct = TransformationFactory.CreateFromCoordinateSystems(ETRS89, GK, Grid, true);

            double[] input = new[] { 351405.315, 5746765.219 };
            double[] expected = new[] { 2558093.045, 5746759.212 };

            var actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore("NAD conversion has large error.")]
        [DefaultFloatingPointTolerance(1e-6)]
        public void TestNAD()
        {
            var NAD27 = SRIDReader.GetCSbyID(4267);
            var NAD83 = SRIDReader.GetCSbyID(4269);

            // https://github.com/OSGeo/proj-datumgrid/tree/master/north-america
            var grid = GridFile.Open(@"ntv2_0.gsb");

            var ct = TransformationFactory.CreateFromCoordinateSystems(NAD27, NAD83, grid, false);

            // http://www.apsalin.com/nad-conversion.aspx
            // https://www.ngs.noaa.gov/NCAT/

            double[] input = new[] { -79.378243, 43.664087 };
            double[] expected = new[] { -79.3780316, 43.6641356 };

            var actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}