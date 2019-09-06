using SCME.Types.DatabaseServer;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SCME.ProfileBuilder.PagesTech
{
    /// <summary>
    /// Логика взаимодействия для ProfilesPage.xaml
    /// </summary>
    public partial class ProfilesPage : Page
    {
        public ProfilesPageVM VM { get; set; } = new ProfilesPageVM();
        private readonly IProfilesService _ProfilesService;

        public ProfilesPage(IProfilesService profilesService)
        {
            InitializeComponent();
            _ProfilesService = profilesService;

            VM.Profiles = new List<Profile>(_ProfilesService.GetProfileItemsWithChildSuperficially(null).Select(m=> m.ToProfileWithChildSuperficially()));
            //qwe.ItemsSource = new List<Types.Gate.TestParameters>()
            //{
            //    new Types.Gate.TestParameters()
            //    {
            //        IGT = 1,
            //        IH = 2
            //    }
            //};
        }


        private void ProfilesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            VM.SelectedProfile = _ProfilesService.GetProfileDeep((e.NewValue as Profile).Key);
        }

        private void ButtonAddTestParameters_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
