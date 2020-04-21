using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

using Kojoto.MNIST.Database;

namespace Kojoto.MNIST.Test
{
    [TestClass]
    public class ImagesSuite
    {
        [TestMethod]
        public void GetEnumerator()
        {
            var fi = new FileInfo("t10k-images.idx3-ubyte");

            var imgs = new Images(fi);

            var en = ((IEnumerable<byte[]>)imgs).GetEnumerator();

            Assert.AreEqual(10000, imgs.Count);
            Assert.AreEqual(784, imgs.PixelCount);

            int count = 0;

            while (en.MoveNext())
            {
                count += 1;
                Assert.AreEqual<int>(784, en.Current.Length,string.Format("Incorrect length for image {0}.", count));
                Assert.IsTrue(count <= 10000);
            }

            Assert.AreEqual(count, 10000);
        }
    }
}
