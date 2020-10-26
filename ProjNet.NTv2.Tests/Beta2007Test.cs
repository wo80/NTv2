using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ProjNet.NTv2.Tests
{
    public class Beta2007Test
    {
        CoordinateTransformationFactory TransformationFactory;
        GridFile Grid;
        List<DataPoint> Data;

        [SetUp]
        public void Setup()
        {
            TransformationFactory = new CoordinateTransformationFactory();
            Grid = GridFile.Open(GetType().Assembly.GetManifestResourceStream("ProjNet.NTv2.Tests.BETA2007.gsb"), true);

            Data = LoadData();
        }

        [Test]
        public void TestBeta2007()
        {
            var cs = new Dictionary<string, CoordinateSystem>();

            cs.Add("DE_DHDN_Lat-Lon", SRIDReader.GetCSbyID(4314));
            cs.Add("DE_DHDN_3GK2", SRIDReader.GetCSbyID(31466));
            cs.Add("DE_DHDN_3GK3", SRIDReader.GetCSbyID(31467));
            cs.Add("DE_DHDN_3GK4", SRIDReader.GetCSbyID(31468));
            cs.Add("DE_DHDN_3GK5", SRIDReader.GetCSbyID(31469));
            cs.Add("ETRS89_Lat-Lon", SRIDReader.GetCSbyID(4326));
            cs.Add("ETRS89_UTM32", SRIDReader.GetCSbyID(25832));
            cs.Add("ETRS89_UTM33", SRIDReader.GetCSbyID(25833));

            CoordinateSystem source, target;

            int success = 0;

            foreach (var item in Data)
            {
                if (cs.TryGetValue(item.qname, out source) && cs.TryGetValue(item.zname, out target))
                {
                    var ct = TransformationFactory.CreateFromCoordinateSystems(source, target, Grid, false);

                    double[] input = new[] { item.qx, item.qy };
                    double[] expected = new[] { item.zx, item.zy };

                    var actual = ct.MathTransform.Transform(input);

                    if (Validate(expected, actual))
                    {
                        success++;
                    }
                }
            }

            Assert.AreEqual(success, Data.Count);
        }

        private bool Validate(double[] expected, double[] actual)
        {
            return AlmostEqual(expected[0], actual[0]) && AlmostEqual(expected[1], actual[1]);
        }

        private static bool AlmostEqual(double a, double b)
        {
            const double EPSILON = 1e-10;

            return (a == b) || Math.Abs(a - b) <= EPSILON * (1 + (Math.Abs(a) + Math.Abs(b)) / 2);
        }

        private List<DataPoint> LoadData()
        {
            // Available for download at http://www.crs-geo.eu/BeTA2007
            string path = @"BETA2007testdaten.csv";

            var data = new List<DataPoint>();

            if (!File.Exists(path))
            {
                return data;
            }

            using (var reader = new StreamReader(path))
            {
                string line = reader.ReadLine();

                var format = CultureInfo.InvariantCulture.NumberFormat;

                while ((line = reader.ReadLine()) != null)
                {
                    var a = line.Split(',');

                    data.Add(new DataPoint()
                    {
                        qname = a[2],
                        qx = double.Parse(a[3], format),
                        qy = double.Parse(a[4], format),
                        zname = a[5],
                        zx = double.Parse(a[6], format),
                        zy = double.Parse(a[7], format)
                    });
                }
            }

            return data;
        }
    }

    class DataPoint
    {
        // Quelle (source cs)
        public string qname;
        public double qx, qy;
        // Ziel (target cs)
        public string zname;
        public double zx, zy;
    }
}
