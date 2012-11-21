﻿namespace Units.Tests.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using NUnit.Framework;

    [TestFixture]
    public class CsvFileTests
    {
        // Content of an invariant culture CSV file
        internal const string Test1Content = @"Text,Number,Length [km]
One,-3,1
Two,17,42.195";

        // Content of a culture-specific CSV file
        internal const string Test2Content = @"Text;Number;Length [km]
One;-3;1
Two;17;42,195";

        [Test]
        public void LoadFromString_Test1()
        {
            var input = Test1Content;
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var file = CsvFile.Load(inputStream);
            ValidateTest1(file);
            Assert.AreEqual(typeof(double), file.Columns[2].Type);

            var outputStream = new MemoryStream();
            file.Save(outputStream);
            Assert.AreEqual(input, Encoding.UTF8.GetString(outputStream.ToArray()));
        }

        [Test]
        public void LoadFromString_Test2()
        {
            var c = new CultureInfo("no");
            var input = Test2Content;
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var file = CsvFile.Load(inputStream, c);
            ValidateTest1(file);
            Assert.AreEqual(typeof(double), file.Columns[2].Type);

            var outputStream = new MemoryStream();
            file.Save(outputStream, c);
            Assert.AreEqual(input, Encoding.UTF8.GetString(outputStream.ToArray()));
        }

        [Test]
        public void LoadFromList_Test1()
        {
            UnitProvider.Default.TrySetDisplayUnit<Length>("km");
            var items = new List<TestObject>
                            {
                                new TestObject { Text = "One", Number = -3, Length = 1 * Length.Kilometre },
                                new TestObject { Text = "Two", Number = 17, Length = 42.195 * Length.Kilometre }
                            };

            var file = CsvFile.Load(items);
            ValidateTest1(file);
            Assert.AreEqual(typeof(Length), file.Columns[2].Type);

            var outputStream = new MemoryStream();
            file.Save(outputStream);
            Assert.AreEqual(Test1Content, Encoding.UTF8.GetString(outputStream.ToArray()));
        }

        [Test, ExpectedException]
        public void PropertyDescriptor()
        {
            var properties = TypeDescriptor.GetProperties(typeof(CsvFile.CsvRow));
            Assert.AreEqual(0, properties.Count);
        }

        private static void ValidateTest1(CsvFile file)
        {
            Assert.AreEqual(3, file.Columns.Count);
            Assert.AreEqual(typeof(string), file.Columns[0].Type);
            Assert.AreEqual(typeof(int), file.Columns[1].Type);
            Assert.AreEqual("km", file.Columns[2].Unit);
            Assert.AreEqual(2, file.Rows.Count);
            var properties = TypeDescriptor.GetProperties(file.Rows[0]);
            Assert.AreEqual(3, properties.Count);
            Assert.AreEqual("Text", properties[0].Name);
            Assert.AreEqual(typeof(int), properties[1].PropertyType);
            Assert.AreEqual(17, properties[1].GetValue(file.Rows[1]));
        }

        internal class TestObject
        {
            [CsvColumn(2)]
            public Length Length { get; set; }
        
            [CsvColumn(1)]
            public int Number { get; set; }

            [CsvColumn(0)]
            public string Text { get; set; }
        }
    }
}