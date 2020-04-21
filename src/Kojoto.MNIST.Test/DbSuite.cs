using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

using Kojoto.MNIST.Database;

namespace Kojoto.MNIST.Test
{
    [TestClass]
    public class DatabaseSuite
    {
        [TestMethod]
        public void GetEnumerator()
        {
            var db = new Db(new FileInfo("t10k-images.idx3-ubyte"), new FileInfo("t10k-labels.idx1-ubyte"));

            var en = ((IEnumerable<IRecord>)db).GetEnumerator();

            Assert.AreEqual(10000, db.ImageCount);
            Assert.AreEqual(784, db.PixelCount);

            var count = 0;

            using (en)
            {
                while (en.MoveNext())
                {
                    count += 1;
                    var record = en.Current;
                    Assert.IsTrue(record.Label >= 0 && record.Label <= 9);
                    Assert.IsTrue(record.Image.Length == 784);
                }
            }

            Assert.AreEqual(10000, count);

        }
    }
}
