using PropertyChanged;
using SCME.Types.Profiles;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class EditProfileVm
    {
        public MyProfile SelectedProfile { get; set; }

        [DependsOn(nameof(SelectedProfile), nameof(IsEditModeActive))]
        public bool IsEditModeEnabled => IsEditModeActive == false && SelectedProfile != null;

        public bool IsEditModeActive { get; set; }

        [DependsOn(nameof(IsEditModeActive))] public bool IsEditModeInActive => !IsEditModeActive;
    }
}