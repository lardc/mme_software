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

namespace SCME.ProfileBuilder.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProfilesPageVM
    {
        public List<string> MMECodes { get; set; }

        public string SelectedMMECode
        {
            get => Properties.Settings.Default.LastSelectedMMECode;
            set => Properties.Settings.Default.LastSelectedMMECode = value;
        }

        [DependsOn(nameof(IsEditModeEnabled))]
        public bool IsCancelSaveModeEnabled => !IsEditModeEnabled;
        public bool IsEditModeEnabled { get; set; } = true;

        public ObservableCollection<MyProfile> Profiles { get; set; }
        public MyProfile SelectedProfile { get; set; }
        public AddTestParametrUserControlVM AddTestParametrUserControlVM { get; set; } = new AddTestParametrUserControlVM();
    }
}
