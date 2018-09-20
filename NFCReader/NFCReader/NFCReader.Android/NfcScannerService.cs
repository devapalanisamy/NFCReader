using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using NdefLibrary.Ndef;
using NFCReader.Droid;
using NdefMessage = NdefLibrary.Ndef.NdefMessage;
using NdefRecord = NdefLibrary.Ndef.NdefRecord;

[assembly: Xamarin.Forms.Dependency(typeof(NfcScannerService))]
namespace NFCReader.Droid
{
    public class NfcScannerService : INfcScannerService
    {
            #region Private Variables

            private NfcAdapter _nfcDevice;
            private NfcTag _nfcTag;
            private Tag _droidTag;

            #endregion

            #region Properties

            public bool IsAvailable
            {
                get
                {
                    return _nfcDevice.IsEnabled;
                }
            }

            #endregion

            #region Constructors

            public NfcScannerService()
            {
                using (NfcManager NfcManager = (NfcManager) Android.App.Application.Context.GetSystemService(Context.NfcService))
                {
                    _nfcDevice = NfcManager.DefaultAdapter;
                }
                _nfcTag = new NfcTag();
            }

            #endregion

            #region Private Methods

            private Ndef GetNdef(Tag tag)
            {
                Ndef ndef = Ndef.Get(tag);
                if (ndef == null)
                    return null;
                else
                    return ndef;
            }

            private NdefLibrary.Ndef.NdefMessage ReadNdef(Ndef ndef)
            {
                if (ndef?.CachedNdefMessage == null)
                {
                    return null;
                }

                var bytes = ndef.CachedNdefMessage.ToByteArray();
                var message = NdefLibrary.Ndef.NdefMessage.FromByteArray(bytes);

                return message;
            }

            #endregion

            #region Public Methods

            public void OnNewIntent(object sender, Intent e)
            {
                _droidTag = e.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
                if (_droidTag != null)
                {
                    _nfcTag.TechList = new List<string>(_droidTag.GetTechList());
                    _nfcTag.Id = _droidTag.GetId();

                    if (GetNdef(_droidTag) == null)
                    {
                        _nfcTag.IsNdefSupported = false;
                    }
                    else
                    {
                        _nfcTag.IsNdefSupported = true;
                        Ndef ndef = GetNdef(_droidTag);
                        _nfcTag.NdefMessage = ReadNdef(ndef);
                        _nfcTag.IsWriteable = ndef.IsWritable;
                        _nfcTag.MaxSize = ndef.MaxSize;
                    }

                    RaiseNewTag(_nfcTag);
                }
            }

            public void WriteTag(NdefLibrary.Ndef.NdefMessage message)
            {
                if (_droidTag == null)
                {
                    throw new Exception("Tag Error: No Tag to write, register to NewTag event before calling WriteTag()");
                }

                Ndef ndef = GetNdef(_droidTag);

                if (ndef == null)
                {
                    throw new Exception("Tag Error: NDEF not supported");
                }


                try
                {
                    ndef.Connect();
                    RaiseTagConnected(_nfcTag);
                }

                catch
                {
                    throw new Exception("Tag Error: No Tag nearby");
                }

                if (!ndef.IsWritable)
                {
                    ndef.Close();
                    throw new Exception("Tag Error: Tag is write locked");
                }

                int size = message.ToByteArray().Length;

                if (ndef.MaxSize < size)
                {
                    ndef.Close();
                    throw new Exception("Tag Error: Tag is too small");
                }

                try
                {
                    List<Android.Nfc.NdefRecord> records = new List<Android.Nfc.NdefRecord>();
                    for (int i = 0; i < message.Count; i++)
                    {
                        if (message[i].CheckIfValid())
                            records.Add(new Android.Nfc.NdefRecord(Android.Nfc.NdefRecord.TnfWellKnown, message[i].Type, message[i].Id, message[i].Payload));
                        else
                        {
                            throw new Exception("NDEFRecord number " + i + "is not valid");
                        }
                    };
                    Android.Nfc.NdefMessage msg = new Android.Nfc.NdefMessage(records.ToArray());
                    ndef.WriteNdefMessage(msg);
                }

                catch (TagLostException tle)
                {
                    throw new Exception("Tag Lost Error: " + tle.Message);
                }

                catch (IOException ioe)
                {
                    throw new Exception("Tag IO Error: " + ioe.ToString());
                }

                catch (Android.Nfc.FormatException fe)
                {
                    throw new Exception("Tag Format Error: " + fe.Message);
                }

                catch (Exception e)
                {
                    throw new Exception("Tag Error: " + e.ToString());
                }

                finally
                {
                    ndef.Close();
                    RaiseTagTagDisconnected(_nfcTag);
                }

            }

            #endregion

            #region Events

            public event EventHandler<NfcTag> TagConnected;

            public void RaiseTagConnected(NfcTag tag)
            {
                _nfcTag.IsConnected = true;

                if (TagConnected != null)
                {
                    TagConnected(this, tag);
                }
            }

            public event EventHandler<NfcTag> TagDisconnected;

            public void RaiseTagTagDisconnected(NfcTag tag)
            {
                _nfcTag.IsConnected = false;

                if (TagDisconnected != null)
                {
                    TagDisconnected(this, tag);
                }
            }


            public event EventHandler<NfcTag> NewTag;

            public void RaiseNewTag(NfcTag tag)
            {
                if (NewTag != null)
                {
                    NewTag(this, tag);
                }
            }

        public ObservableCollection<string> ReadNdefMessage(NdefMessage message)
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            if (message == null)
            {
                return collection;
            }

            foreach (NdefRecord record in message)
            {
                if (record.CheckSpecializedType(false) == typeof(NdefTextRecord))
                {

                    var textRecord = new NdefTextRecord(record);
                    collection.Add("Plain Text: " + textRecord.Text);
                }
            }
            return collection;
        }
        #endregion
    }
}