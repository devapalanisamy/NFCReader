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
    public class NfcScannerService : NSObject, INfcScannerService
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

        public void RaiseNewTag(string tag)
        {
            NewTag?.Invoke(this, tag);
        }

        private NFCNdefReaderSession _session;
        private TaskCompletionSource<string> _tcs;

        public async Task ScanAsync()
        {
            var reader = new NfcReader();
            var message = await reader.ScanAsync();
            RaiseNewTag(message);
        }
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