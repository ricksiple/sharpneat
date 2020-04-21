using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Collections;

namespace Kojoto.MNIST.Database
{
    public class Labels : IEnumerable<byte>, IEnumerator<byte>
    {
        private FileInfo _file;
        private BinaryReader _reader;
        private byte _value;
        private const int _MAGIC_NUMBER = 2049;

        public Labels(FileInfo labelFile)
        {
            _file = labelFile;
        }

        public int Count { get; private set; } = 0;

        #region IEnumerable<byte>

        // IEnumerable<byte> 

        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            // open labels
            _reader = new BinaryReader(_file.OpenRead());

            // read magic number
            var magicNumber = System.BitConverter.ToInt32(_reader.ReadBytes(4).Reverse<byte>().ToArray<byte>(), 0);
            if (_MAGIC_NUMBER != magicNumber)
            {
                throw new InvalidDataException(string.Format("Incorrect magic number for label file.  Expected {0}, Actual {1}", _MAGIC_NUMBER, magicNumber));
            }

            // read number of labels
            Count = System.BitConverter.ToInt32(_reader.ReadBytes(4).Reverse<byte>().ToArray<byte>(), 0);

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
            try
            {
                _value = _reader.ReadByte();
                return true;
            }
            catch (EndOfStreamException)
            {
                return false;
            }
        }

        void IEnumerator.Reset()
        {
            _reader.BaseStream.Seek(8, SeekOrigin.Begin);
        }

        byte IEnumerator<byte>.Current
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
