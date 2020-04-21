using System.Collections;
using System.Collections.Generic;

using System.IO;

namespace Kojoto.MNIST.Database
{
    public class Db : IEnumerable<IRecord>, IEnumerator<IRecord>
    {
        private FileInfo _ImageFile;
        private FileInfo _LabelFile;

        private Labels _Labels;
        private Images _Images;

        private IEnumerator<byte> _LabelEnum;
        private IEnumerator<byte[]> _ImageEnum;

        public Db(FileInfo imageFile, FileInfo labelFile)
        {
            _ImageFile = imageFile;
            _LabelFile = labelFile;

            _Labels = new Labels(_LabelFile);
            _Images = new Images(_ImageFile);

            _LabelEnum = ((IEnumerable<byte>)_Labels).GetEnumerator();
            _ImageEnum = ((IEnumerable<byte[]>)_Images).GetEnumerator();

            if (_Labels.Count != _Images.Count)
            {
                throw new InvalidDataException(string.Format("Label count {0} does not match image count {1}.", _Labels.Count, _Images.Count));
            }
        }

        public int ImageCount
        {
            get
            {
                return _Images.Count;
            }
        }

        public int PixelCount
        {
            get
            {
                return _Images.PixelCount;
            }
        }

        #region IEnumerable<IRecord>

        // IEnumerable<IRecord>

        IEnumerator<IRecord> IEnumerable<IRecord>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerator<IRecord>

        // IEnumerator<IRecord>

        IRecord IEnumerator<IRecord>.Current
        {
            get
            {
                return new Record(_LabelEnum.Current, _ImageEnum.Current);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return ((IEnumerator<IRecord>)this).Current;
            }
        }

        bool IEnumerator.MoveNext()
        {
            var labelEOF = _LabelEnum.MoveNext();
            var imageEOF = _ImageEnum.MoveNext();

            if (labelEOF != imageEOF)
            {
                if (labelEOF)
                {
                    throw new InvalidDataException("EOF in label file, but not in image file.");
                }
                else
                {
                    throw new InvalidDataException("EOF in iamge file, but not in label file.");
                }
            }

            return labelEOF;

        }

        void IEnumerator.Reset()
        {
            _LabelEnum.Reset();
            _ImageEnum.Reset();
        }

        #endregion

        private class Record : IRecord
        {

            private int _Label;
            public byte[] _Image;

            public Record(int label, byte[] image)
            {
                _Label = label;
                _Image = image;
            }

            int IRecord.Label
            {
                get { return _Label; }
            }

            byte[] IRecord.Image
            {
                get { return _Image; }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ImageEnum.Dispose();
                    _LabelEnum.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Database()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void System.IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public interface IRecord
    {
        int Label { get; }
        byte[] Image { get; }
    }
}
