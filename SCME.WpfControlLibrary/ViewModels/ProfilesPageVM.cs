using PropertyChanged;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.ViewModels.ProfilesPage;
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
        public Dictionary<string, int> MMECodes { get; set; }
        public string SelectedMMECode { get => Properties.Settings.Default.LastSelectedMMECode; set => Properties.Settings.Default.LastSelectedMMECode = value; }

        public string SearchingName { get; set; }

        [DependsOn(nameof(IsEditModeEnabled))]
        public bool IsCancelSaveModeEnabled => !IsEditModeEnabled;
        public bool IsEditModeEnabled { get; set; } = true;

        public ObservableCollection<MyProfile> _Profiles { get; set; }
        public ObservableCollection<MyProfile> Profiles
        {
            get => _Profiles;
            set
            {
                if(_Profiles != null)
                    _Profiles.CollectionChanged -= Profiles_CollectionChanged;
                HideProfilesForSearch = new List<MyProfile>();
                _Profiles = value;
                _Profiles.CollectionChanged += Profiles_CollectionChanged;
            }
        }
        public AddTestParametrUserControlVM AddTestParametrUserControlVM { get; set; } = new AddTestParametrUserControlVM();



        public ProfileDeepData ProfileDeepData { get; set; }


        public MyProfile SelectedProfile { get; set; }


        public List<MyProfile> HideProfilesForSearch { get; private set; } = new List<MyProfile>();

        private void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            HideProfilesForSearch.AddRange(e.NewItems.Cast<MyProfile>());
            if(e.OldItems != null)
            foreach (var i in e.OldItems.Cast<MyProfile>())
                HideProfilesForSearch.Remove(i);
        }
    }
}
