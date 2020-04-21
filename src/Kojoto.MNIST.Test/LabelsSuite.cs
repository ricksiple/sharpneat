using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

using Kojoto.MNIST.Database;

namespace Kojoto.MNIST.Test
{
    [TestClass]
    public class LabelsSuite
    {
        [TestMethod]
        public void GetEnumerator()
        {
            var fi = new FileInfo("t10k-labels.idx1-ubyte");

            var lbls = new Labels(fi);

            var en = ((IEnumerable<byte>)lbls).GetEnumerator();

            Assert.AreEqual(10000, lbls.Count);

            int count = 0;

            while (en.MoveNext())
            {
                count += 1;
                Assert.IsTrue(en.Current >= 0 && en.Current <= 9);
            }

            Assert.AreEqual(count, 10000);
        }
    }
}
