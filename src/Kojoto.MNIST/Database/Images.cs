using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections;

namespace Kojoto.MNIST.Database
{
    public class Images : IEnumerable<byte[]>, IEnumerator<byte[]>
    {
        private FileInfo _file;
        private BinaryReader _reader;
        private byte[] _value;
        private int _rows;
        private int _cols;

        private const int _MAGIC_NUMBER = 2051;

        public Images(FileInfo imageFile)
        {
            _file = imageFile;
        }

        public int Count { get; private set; } = 0;
        public int PixelCount { get; private set; } = 0;
        
        #region IEnumerable<byte>

        // IEnumerable<byte> 

        IEnumerator<byte[]> IEnumerable<byte[]>.GetEnumerator()
        {
            // open iamges file
            _reader = new BinaryReader(_file.OpenRead());

            // read magic number
            var magicNumber = System.BitConverter.ToInt32(_reader.ReadBytes(4).Reverse<byte>().ToArray<byte>(), 0);
            if (_MAGIC_NUMBER != magicNumber)
            {
                throw new InvalidDataException(string.Format("Incorrect magic number for image file.  Expected {0}, Actual {1}", _MAGIC_NUMBER, magicNumber));
            }

            // read number of images
            Count = System.BitConverter.ToInt32(_reader.ReadBytes(4).Reverse<byte>().ToArray<byte>(), 0);

            // read number of rows
            _rows = System.BitConverter.ToInt32(_reader.ReadBytes(4).Reverse<byte>().ToArray<byte>(), 0);

            // read number of columns
            _cols = System.BitConverter.ToInt32(_reader.ReadBytes(4).Reverse<byte>().ToArray<byte>(), 0);

            // store pixel count per image
            PixelCount = _rows * _cols;

            // return enumerator
            return this;

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<byte>)this).GetEnumerator();
        }

        #endregion

        #region IEnumerator<byte>

        // IEnumerator<byte>

        bool IEnumerator.MoveNext()
        {
            _value = _reader.ReadBytes(PixelCount);
            return _value.Length == PixelCount;
        }

        void IEnumerator.Reset()
        {
            _reader.BaseStream.Seek(8, SeekOrigin.Begin);
        }

        byte[] IEnumerator<byte[]>.Current
        {
            get
            {
                return _value;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return _value;
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _reader.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Labels()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
