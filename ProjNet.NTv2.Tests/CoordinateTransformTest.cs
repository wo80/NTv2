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

        // Data taken from BETA2007testdaten.csv (http://www.crs-geo.eu/BeTA2007)

        [Test]
        public void TestGridTransformation_ProjCS_NTv2()
        {
            var GK = SRIDReader.GetCSbyID(31466); // DHDN (3-degree Gauss-Kruger zone 2)
            var ETRS89 = SRIDReader.GetCSbyID(25832); // ETRS89 (UTM Zone 32N)

            var ctf = new CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(GK, ETRS89, Grid, false);

            double[] input = new[] { 2598417.333192, 5930677.980308 };
            double[] expected = new[] { 399340.601863, 5928794.177992 };

            double[] actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGridTransformationInverse_ProjCS_NTv2()
        {
            var ETRS89 = SRIDReader.GetCSbyID(25832); // ETRS89 (UTM Zone 32N)
            var GK = SRIDReader.GetCSbyID(31466); // DHDN (3-degree Gauss-Kruger zone 2)

            var ctf = new CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(ETRS89, GK, Grid, true);

            double[] input = new[] { 399340.601863, 5928794.177992 };
            double[] expected = new[] { 2598417.333192, 5930677.980308 };

            double[] actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [DefaultFloatingPointTolerance(1e-8)]
        public void TestGridTransformation_GeogCS_NTv2()
        {
            var DHDN = SRIDReader.GetCSbyID(4314); // DE_DHDN_Lat-Lon
            var ETRS89 = SRIDReader.GetCSbyID(4326); // ETRS89_Lat-Lon

            var ctf = new CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(DHDN, ETRS89, Grid, false);

            double[] input = new[] { 7.483333333333, 53.500000000000 };
            double[] expected = new[] { 7.482506019176, 53.498461143331 };

            double[] actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [DefaultFloatingPointTolerance(1e-8)]
        public void TestGridTransformationInverse_GeogCS_NTv2()
        {
            var ETRS89 = SRIDReader.GetCSbyID(4326); // ETRS89_Lat-Lon
            var DHDN = SRIDReader.GetCSbyID(4314); // DE_DHDN_Lat-Lon

            var ctf = new CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(ETRS89, DHDN, Grid, true);

            double[] input = new[] { 7.482506019176, 53.498461143331 };
            double[] expected = new[] { 7.483333333333, 53.500000000000 };

            double[] actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        [DefaultFloatingPointTolerance(1e-8)]
        public void TestGridTransformation_Proj2GeogCS_NTv2()
        {
            var GK = SRIDReader.GetCSbyID(31466); // DHDN (3-degree Gauss-Kruger zone 2)
            var ETRS89 = SRIDReader.GetCSbyID(4326); // ETRS89_Lat-Lon

            var ctf = new CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(GK, ETRS89, Grid, false);

            double[] input = new[] { 2598417.333192, 5930677.980308 };
            double[] expected = new[] { 7.482506019176, 53.498461143331 };

            double[] actual = ct.MathTransform.Transform(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGridTransformation_Geog2ProjCS_NTv2()
        {
            var ETRS89 = SRIDReader.GetCSbyID(4326); // ETRS89_Lat-Lon
            var GK = SRIDReader.GetCSbyID(31466); // DHDN (3-degree Gauss-Kruger zone 2)

            var ctf = new CoordinateTransformationFactory();
            var ct = ctf.CreateFromCoordinateSystems(ETRS89, GK, Grid, true);

            double[] input = new[] { 7.482506019176, 53.498461143331 };
            double[] expected = new[] { 2598417.333192, 5930677.980308 };

            double[] actual = ct.MathTransform.Transform(input);

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