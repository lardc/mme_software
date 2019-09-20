using PropertyChanged;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProfilesPageVM
    {
        public Dictionary<string, int> MmeCodes { get; set; }

        public string SelectedMMECode
        {
            get => Properties.Settings.Default.LastSelectedMMECode;
            set => Properties.Settings.Default.LastSelectedMMECode = value;
        }

        public string SearchingName { get; set; }

        [DependsOn(nameof(IsEditModeEnabled))] public bool IsCancelSaveModeEnabled => !IsEditModeEnabled;
        public bool IsEditModeEnabled { get; set; } = true;

        private ObservableCollection<MyProfile> _profiles;

        public ObservableCollection<MyProfile> Profiles
        {
            get => _profiles;
            set
            {
                if (_profiles != null)
                    _profiles.CollectionChanged -= Profiles_CollectionChanged;
                HideProfilesForSearch = new List<MyProfile>();
                _profiles = value;
                _profiles.CollectionChanged += Profiles_CollectionChanged;
            }
        }

        public ProfileDeepData ProfileDeepData { get; set; }


        public MyProfile SelectedProfile { get; set; }


        public List<MyProfile> HideProfilesForSearch { get; private set; } = new List<MyProfile>();

        private void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                HideProfilesForSearch.AddRange(e.NewItems.Cast<MyProfile>());
            if (e.OldItems == null)
                return;
            foreach (var i in e.OldItems.Cast<MyProfile>())
                HideProfilesForSearch.Remove(i);
        }
    }
}