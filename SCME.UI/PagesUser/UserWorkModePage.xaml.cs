using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SCME.UI.IO;
using SCME.UI.Properties;
using System.Windows.Navigation;
using SCME.UI.CustomControl;
using SCME.Types;

namespace SCME.UI.PagesUser
{
    /// <summary>
    /// Interaction logic for UserWorkMode.xaml
    /// </summary>
    public partial class UserWorkModePage
    {
        public ViewModels.UserWorkModePageVM VM { get; set; } = new ViewModels.UserWorkModePageVM();
        public UserWorkModePage()
        {
            InitializeComponent();
            Cache.Net.SetSafetyMode(Types.SafetyMode.Disabled);
        }


        private void SelectOperatorMode(UserWorkMode userWorkMode)
        {
            switch (userWorkMode)
            {
                case UserWorkMode.Operator:
                    Cache.Net.SetSafetyMode(Types.SafetyMode.Internal);
                    break;
                case UserWorkMode.OperatorBuildMode:
                    Cache.Net.SetSafetyMode(Types.SafetyMode.External);
                    break;
                default:
                    throw new NotImplementedException();
            }
            //если мы были в режиме специальных измерений - надо перечитать содержимое профилей из базы данных чтобы откатить все изменения, сделанные в профилях в режиме специальных имерений
            if (Cache.WorkMode == UserWorkMode.SpecialMeasure)
                ProfilesDbLogic.ImportProfilesFromDb();

            //запоминаем выбранный пользователем режим работы
            Cache.WorkMode = UserWorkMode.Operator;

            NavigationService?.Navigate(Cache.Login);
        }

        private void SelectMode_Click(object Sender, RoutedEventArgs E)
        {
            //чтобы пользователь не смог нажать повторно на кнопку, чей обработчик уже исполняется или на любую другую кнопку этой формы
            VM.ButtonsModeIsEnabled = false;

            try
            {
                UserWorkMode userWorkMode = ((UserWorkMode)(Sender as Button).CommandParameter) ;

                Cache.Net.SetUserWorkMode(userWorkMode);
                switch (userWorkMode)
                {
                    case UserWorkMode.Operator:
                    case UserWorkMode.OperatorBuildMode:
                        SelectOperatorMode(userWorkMode);
                        break;

                    case UserWorkMode.ServiceMan:
                        //если мы были в режиме специальных измерений - надо перечитать содержимое профилей из базы данных чтобы откатить все изменения, сделанные в профилях в режиме специальных имерений
                        if (Cache.WorkMode == UserWorkMode.SpecialMeasure)
                            ProfilesDbLogic.ImportProfilesFromDb();

                        //запоминаем выбранный пользователем режим работы
                        Cache.WorkMode = UserWorkMode.ServiceMan;
                        Cache.Main.ServiceManLogin();
                        break;

                    case UserWorkMode.SpecialMeasure:
                        if (Settings.Default.IsTechPasswordEnabled)
                        {
                            //без ввода пароля наладчика не будем пускать в режим специальных измерений
                            Cache.Password.AfterOkRoutine += SpecialMeasureCallBack;
                            NavigationService?.Navigate(Cache.Password);
                        }
                        else SpecialMeasureCallBack(NavigationService);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            catch(Exception ex)
            {
                var dialog = new DialogWindow("Ошибка", ex.ToString());
                dialog.ButtonConfig(DialogWindow.EbConfig.OK);
                dialog.ShowDialog();
            }
            finally
            {
                VM.ButtonsModeIsEnabled = true;
            }
        }

        private void SpecialMeasureCallBack(NavigationService ns)
        {
            //запоминаем выбранный пользователем режим работы
            Cache.WorkMode = UserWorkMode.SpecialMeasure;

            Cache.ProfileSelection.ClearFilter();
            Cache.ProfileEdit.InitFilter();

            ns?.Navigate(Cache.ProfileEdit);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(Cache.Welcome);
        }
    }
}
