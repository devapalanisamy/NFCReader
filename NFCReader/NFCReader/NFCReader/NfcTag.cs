using System.Collections;
using NdefLibrary.Ndef;

namespace NFCReader
{
    public class NfcTag
    {
        public NdefMessage NdefMessage;
        public IList TechList;
        public bool IsNdefSupported;
        public bool IsWriteable;
        public bool IsConnected;
        public byte[] Id;
        public int MaxSize;
    }
}