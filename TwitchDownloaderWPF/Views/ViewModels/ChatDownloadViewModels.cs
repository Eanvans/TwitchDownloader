using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwitchDownloaderCore.Models;

namespace TwitchDownloaderWPF.Views.ViewModels
{
    public class ChatDownloadViewModels : ObservableObject
    {
        private ObservableCollection<VodCommentData> _vodCommentsData = new();
        public ObservableCollection<VodCommentData> VodCommentsData { get => _vodCommentsData; set => SetProperty(ref _vodCommentsData, value); }

        public ChatDownloadViewModels()
        {
        }

        public void SetVodCommentsData(List<VodCommentData> l)
        {
            VodCommentsData = new(l);
        }
    }
}
