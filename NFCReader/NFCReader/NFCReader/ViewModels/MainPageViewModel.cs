using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NdefLibrary.Ndef;
using Prism.Services;

namespace NFCReader.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INfcScannerService _nfcScannerService;
        private readonly IPageDialogService _pageDialogService;
        public MainPageViewModel(INavigationService navigationService, INfcScannerService nfcScannerService, IPageDialogService dialogService)
            : base(navigationService)
        {
            Title = "Main Page";
            _nfcScannerService = Xamarin.Forms.DependencyService.Get<INfcScannerService>();
            _pageDialogService = dialogService;
            _nfcScannerService.NewTag += HandleNewTag;
            _nfcScannerService.TagConnected += OnTagConnected;
            _nfcScannerService.TagDisconnected += OnTagDisconnected;
        }

        private async void OnTagDisconnected(object sender, NfcTag e)
        {
            var text = readNDEFMEssage(e.NdefMessage);
            await _pageDialogService.DisplayAlertAsync("Tag Content", text[0], null, "ok");
        }

        private async void OnTagConnected(object sender, NfcTag e)
        {
            var text = readNDEFMEssage(e.NdefMessage);
            await _pageDialogService.DisplayAlertAsync("Tag Content", text[0], null, "ok");
        }

        private async void HandleNewTag(object sender, NfcTag e)
        {
            var text = readNDEFMEssage(e.NdefMessage);
           await _pageDialogService.DisplayAlertAsync("Tag Content", text[0], null, "ok");
        }

        private ObservableCollection<string> readNDEFMEssage(NdefMessage message)
        {
            ObservableCollection<string> collection = new ObservableCollection<string>();

            if (message == null)
            {
                return collection;
            }

            foreach (NdefRecord record in message)
            {
                // Go through each record, check if it's a Smart Poster
                if (record.CheckSpecializedType(false) == typeof(NdefSpRecord))
                {
                    // Convert and extract Smart Poster info
                    var spRecord = new NdefSpRecord(record);
                    collection.Add("URI: " + spRecord.Uri);
                    collection.Add("Titles: " + spRecord.TitleCount());
                    collection.Add("1. Title: " + spRecord.Titles[0].Text);
                    collection.Add("Action set: " + spRecord.ActionInUse());
                }

                if (record.CheckSpecializedType(false) == typeof(NdefUriRecord))
                {
                    // Convert and extract Smart Poster info
                    var spRecord = new NdefUriRecord(record);
                    collection.Add("Text: " + spRecord.Uri);
                }
            }
            return collection;
        }
    }
}
