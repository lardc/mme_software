using SCME.Linker.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SCME.Types;
using System.Windows.Controls.Primitives;

namespace SCME.Linker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IInputElement LastInputElement = null;

        //табельный номер, идентификатор аутентифицированного в данном приложении пользователя и битовая маска его разрешений
        public string FTabNum = null;
        public long FUserID = -1;
        public ulong FPermissionsLo = 0;

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Localization);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Localization error");
            }

            InitializeComponent();
        }

        static void DispatcherUnhandledException(object Sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs E)
        {
            MessageBox.Show(E.Exception.ToString(), "Unhandled exception");
        }

        private static void CurrentDomainOnUnhandledException(object Sender, UnhandledExceptionEventArgs Args)
        {
            MessageBox.Show(Args.ExceptionObject.ToString(), "Unhandled exception");
        }

        private void btAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            //проверяем имеет ли пользователь возможность работы с данной системой
            this.DialogResult = this.IsUserOK(tb_User.Text, pbPassword.Password, out ((MainWindow)this.Owner).FTabNum, out ((MainWindow)this.Owner).FUserID, out ((MainWindow)this.Owner).FPermissionsLo);

            if ((this.DialogResult == null) && (this.LastInputElement != null))
                FocusManager.SetFocusedElement(this, this.LastInputElement);
        }

        private bool? IsUserOK(string name, string userPassword, out string tabNum, out long userID, out ulong permissionsLo)
        {
            //проверяем имеет ли пользователь регистрацию в системе DC
            long dcUserID = DbRoutines.CheckDCUserExist(name, userPassword);

            switch (dcUserID)
            {
                case -1:
                    //введённый пароль неверен, либо пользователя с именем userName не существует;
                    MessageBox.Show(string.Format(Properties.Resources.PasswordIsIncorrect, name), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    tabNum = null;
                    userID = -1;
                    permissionsLo = 0;

                    return null;

                case -2:
                    MessageBox.Show(Properties.Resources.PasswordIncorrect, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    tabNum = null;
                    userID = -1;
                    permissionsLo = 0;

                    return null;

                default:
                    if (dcUserID > 0)
                    {
                        //если больше нуля - пользователь userName является пользователем DC. проверяем является ли пользователь DC пользователем данного приложения
                        switch (DbRoutines.UserPermissions(dcUserID, out permissionsLo))
                        {
                            case false:
                                //пользователь userID не является пользователем приложения
                                MessageBox.Show(string.Format(Properties.Resources.UserIisNotAnApplicationUser, name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                                tabNum = null;
                                userID = -1;
                                permissionsLo = 0;

                                return false;

                            default:
                                tabNum = name;
                                userID = dcUserID;

                                return true;
                        }
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.PasswordIncorrect, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                        tabNum = null;
                        userID = -1;
                        permissionsLo = 0;

                        return null;
                    }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.DialogResult = false;
                    break;

                case Key.Enter:
                    this.btAuthenticate.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    break;
            }
        }

        private void tb_User_LostFocus(object sender, RoutedEventArgs e)
        {
            this.LastInputElement = sender as IInputElement;
        }

        private void pbPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            this.LastInputElement = sender as IInputElement;
        }

    }
}
