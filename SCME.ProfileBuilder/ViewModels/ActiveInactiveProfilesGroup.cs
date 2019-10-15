using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using PropertyChanged;
using SCME.InterfaceImplementations.NewImplement.SQLite;
using SCME.Types.Database;
using SCME.Types.Profiles;

namespace SCME.ProfileBuilder.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ActiveInactiveProfilesGroup
    {
        public CollectionViewSource ProfilesSource { get; }
        public ObservableCollection<MyProfile> _profiles { get; set; }
        
        public ActiveInactiveProfilesGroup(IEnumerable<string> mmeCodes, IDbService dbService)
        {
            ProfilesSource.Source = _profiles = new ObservableCollection<MyProfile>(mmeCodes.SelectMany(m=>
                dbService.GetProfilesSuperficially(m)));
        }

    }
}