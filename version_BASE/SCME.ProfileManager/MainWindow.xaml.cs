using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SCME.ProfileManager.Properties;
using SCME.Types.Profiles;

namespace SCME.ProfileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ProfileDictionaryObject m_Root;

        public MainWindow()
        {
            InitializeComponent();
        }

        public ProfileDictionaryObject Root
        {
            get
            {
                return m_Root;
            }
            private set
            {
                m_Root = value;
                OnPropertyChanged("Root");
            }
        }

        private void Window_Loaded(object Sender, RoutedEventArgs E)
        {
            if(!SystemHost.Initialize())
                Application.Current.Shutdown();

            try
            {
                Root = SystemHost.ProfileClient.ReadProfileDictionary(Settings.Default.ManagerID);

                if(Root == null)
                    throw new ApplicationException("Server sent no data");
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Невозможно получить данные от сервера: {0}", ex.Message),
                    "Ошибка соединения",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
            }
        }

        private void Window_Unloaded(object Sender, RoutedEventArgs E)
        {
            SystemHost.Close();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(PropertyName)); 
        }

        #endregion

        private void TreeProfiles_OnPreviewMouseRightButtonDown(object Sender, MouseButtonEventArgs E)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(E.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                E.Handled = true;
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject Source)
        {
            while (Source != null && !(Source is TreeViewItem))
                Source = VisualTreeHelper.GetParent(Source);

            return Source as TreeViewItem;
        }

        private void CommandBindingSave_OnCanExecute(object Sender, CanExecuteRoutedEventArgs E)
        {
        }

        private void CommandBindingSave_OnExecuted(object Sender, ExecutedRoutedEventArgs E)
        {
        }

        private void CommandBindingEdit_OnCanExecute(object Sender, CanExecuteRoutedEventArgs E)
        {
            if (treeProfiles != null)
            {
                var node = treeProfiles.SelectedItem;

                if (node != null)
                {
                    E.CanExecute = ((node as ProfileFolder) != null) ||
                                   ((node as ProfileSet) != null);
                }
            }
        }

        private void CommandBindingEdit_OnExecuted(object Sender, ExecutedRoutedEventArgs E)
        {
            var node = treeProfiles.SelectedItem as ProfileDictionaryObject;

            if (node != null)
            {
                var renamer = new RenameForm(node.Name);
                var result = renamer.ShowDialog();

                if(result.HasValue && result.Value)
                    node.Name = renamer.TextValue;
            }
        }

        private void CommandBindingDelete_OnCanExecute(object Sender, CanExecuteRoutedEventArgs E)
        {
            if (treeProfiles != null)
            {
                var node = treeProfiles.SelectedItem;

                if (node != null)
                {
                    E.CanExecute = ((node as ProfileFolder) != null) ||
                                   ((node as ProfileSet) != null);
                }
            }
        }

        private void CommandBindingDelete_OnExecuted(object Sender, ExecutedRoutedEventArgs E)
        {
            var node = treeProfiles.SelectedItem as ProfileDictionaryObject;

            if (node != null)
            {
                if (node.Parent != null)
                    node.Parent.ChildrenList.Remove(node);
                else
                    Root.ChildrenList.Remove(node);
            }
        }
    }
}
