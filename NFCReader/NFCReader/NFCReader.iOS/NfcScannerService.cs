using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreFoundation;
using CoreNFC;
using Foundation;
using NdefLibrary.Ndef;
using NFCReader.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(NfcScannerService))]
namespace NFCReader.iOS
{
    public class NfcScannerService : NSObject, INfcScannerService, INFCNdefReaderSessionDelegate
    {
        public bool IsAvailable { get; }
        private NfcTag _nfcTag;
        public void WriteTag(NdefMessage message)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<string> NewTag;
        public event EventHandler<NfcTag> TagConnected;
        public event EventHandler<NfcTag> TagDisconnected;
        public ObservableCollection<string> ReadNdefMessage(NdefMessage message)
        {
            throw new NotImplementedException();
        }

        public NfcScannerService()
        {
            _nfcTag = new NfcTag();
        }

        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            var readerError = (NFCReaderError)(long)error.Code;
            if (readerError != NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead &&
                readerError != NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
            {
                // some error handling
            }
        }

        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            _nfcTag.TechList = new List<string>();
            foreach (NFCNdefMessage msg in messages)
            {
                
            }
            RaiseNewTag(null);
        }

        public IntPtr Handle { get; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void RaiseNewTag(string tag)
        {
            if (NewTag != null)
            {
                NewTag(this, tag);
            }
        }

        private NFCNdefReaderSession _session;
        private TaskCompletionSource<string> _tcs;

        public async Task ScanAsync()
        {
            var reader = new NfcReader();
            var message = await reader.ScanAsync();
            //var nfcTag = new NfcTag();
            //nfcTag.NdefMessage = NdefMessage.FromByteArray(Encoding.ASCII.GetBytes(message));
            RaiseNewTag(message);
        }

        //private NdefLibrary.Ndef.NdefMessage ReadNdef(Ndef ndef)
        //{
        //    if (ndef?.CachedNdefMessage == null)
        //    {
        //        return null;
        //    }

        //    var bytes = ndef.CachedNdefMessage.ToByteArray();
        //    var message = NdefLibrary.Ndef.NdefMessage.FromByteArray(bytes);

        //    return message;
        //}
    }

    public class NfcReader : NSObject, INFCNdefReaderSessionDelegate
    {
        private NFCNdefReaderSession _session;
        private TaskCompletionSource<string> _tcs;
        private NfcTag _nfcTag;

        public NfcReader()
        {
            _nfcTag = new NfcTag();
        }
        public Task<string> ScanAsync()
        {
            if (!NFCNdefReaderSession.ReadingAvailable)
            {
                throw new InvalidOperationException("Reading NDEF is not available");
            }

            _tcs = new TaskCompletionSource<string>();
            _session = new NFCNdefReaderSession(this, DispatchQueue.CurrentQueue, true);
            _session.BeginSession();

            return _tcs.Task;
        }

        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            _tcs.TrySetException(new Exception(error?.LocalizedFailureReason));
        }

        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            var bytes = messages[0].Records[0].Payload.Skip(3).ToArray();
            var message = Encoding.UTF8.GetString(bytes);
            _tcs.SetResult(message);
        }
    }
}