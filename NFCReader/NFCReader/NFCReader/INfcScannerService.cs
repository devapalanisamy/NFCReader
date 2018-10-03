using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NdefLibrary.Ndef;

namespace NFCReader
{
    public interface INfcScannerService
    {

        bool IsAvailable { get; }
        void WriteTag(NdefMessage message);
        event EventHandler<string> NewTag;
        event EventHandler<NfcTag> TagConnected;
        event EventHandler<NfcTag> TagDisconnected;
        ObservableCollection<string> ReadNdefMessage(NdefMessage message);
        Task ScanAsync();
    }
}