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
    }
}