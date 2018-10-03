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
using Xamarin.Forms;

namespace NFCReader.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INfcScannerService _nfcScannerService;
        private readonly IPageDialogService _pageDialogService;
        public MainPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
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
            var text = _nfcScannerService.ReadNdefMessage(e.NdefMessage);
            await _pageDialogService.DisplayAlertAsync("Tag Content", text[0], null, "ok");
        }

        private async void OnTagConnected(object sender, NfcTag e)
        {
            var text = _nfcScannerService.ReadNdefMessage(e.NdefMessage);
            await _pageDialogService.DisplayAlertAsync("Tag Content", text[0], null, "ok");
        }

        private async void HandleNewTag(object sender, string e)
        {
           // var text = _nfcScannerService.ReadNdefMessage(e.NdefMessage);
           await _pageDialogService.DisplayAlertAsync("Tag Content", e, null, "ok");
        }


        private DelegateCommand<ItemTappedEventArgs> _scanCommand;

        public DelegateCommand<ItemTappedEventArgs> ScanCommand
        {
            get
            {
                if (_scanCommand == null)
                {
                    _scanCommand = new DelegateCommand<ItemTappedEventArgs>(ScanNfc);
                }
                return _scanCommand;
            }
        }

        private async void ScanNfc(ItemTappedEventArgs obj)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                await _nfcScannerService.ScanAsync();
            }
        }
    }
}
