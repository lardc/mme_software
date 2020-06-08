using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SCME.Types;

namespace SCME.dbViewer
{
    /// <summary>
    /// Interaction logic for AuthenticationWindow.xaml
    /// </summary>
    public partial class AuthenticationWindow : Window
    {
        private IInputElement LastInputElement = null;

        public AuthenticationWindow()
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
        }

        public bool? ShowModal(out string tabNum, out long userID, out ulong permissionsLo)
        {
            //если раньше пользователь успел аутентифицироваться в данном приложении - будем использовать введённый им табельный номер
            tb_User.Text = (((MainWindow)this.Owner).FTabNum == null) ? System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1].ToString() : ((MainWindow)this.Owner).FTabNum;

            bool? result = this.ShowDialog();

            if (result ?? false)
            {
                tabNum = this.tb_User.Text;
                userID = ((MainWindow)this.Owner).FUserID;
                permissionsLo = ((MainWindow)this.Owner).FPermissionsLo;
            }
            else
            {
                tabNum = null;
                userID = -1;
                permissionsLo = 0;
            }

            return result;
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
                        MessageBox.Show("Введённый пароль неверен.", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);

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
