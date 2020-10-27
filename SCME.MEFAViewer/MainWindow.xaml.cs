using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using SCME.MEFADB;
using SCME.MEFADB.Tables;

namespace SCME.MEFAViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindowVm Vm { get; set; } = new MainWindowVm();
        private readonly MonitoringContext _db;
        private readonly DispatcherTimer _dispatcherTimer;

        public MainWindow()
        {
            _db = new MonitoringContext(new DbContextOptionsBuilder<MonitoringContext>().UseSqlServer(App.AppSettings.ConnectionString).Options);
            var q = _db.MmeCodesTry.ToList();
            Vm.MmeTiles = new ObservableCollection<MmeTile>(q.Select(m => new MmeTile()
            {
                Id = m.MmeCodeId,
                Name = m.Name
            }));
            InitializeComponent();
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Tick += RefreshMmeOnTick;
            _dispatcherTimer.Start();
            /*_db.MonitoringEvents.Add(new MonitoringEvent()
            {
                Timestamp = DateTime.Now,
                MmeCode = "A",
                MonitoringEventType = _db.MonitoringEventTypes.Single(m=> m.EventName == MonitoringEventType.HEART_BEAT_EVENT_NAME)
            });
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                Timestamp = DateTime.Now,
                MmeCode = "MME009",
                MonitoringEventType = _db.MonitoringEventTypes.Single(m=> m.EventName == MonitoringEventType.HEART_BEAT_EVENT_NAME)
            });*/
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                Timestamp = DateTime.Now,
                MmeCode = "MME009",
                MonitoringEventType = _db.MonitoringEventTypes.Single(m=> m.EventName == MonitoringEventType.TEST_EVENT_NAME)
            });
            _db.SaveChanges();
        }

        private object _lock = 0;
        private void RefreshMmeOnTick(object sender, EventArgs e)
        {
            lock (_lock)
            {
                var fiveMinutes = new TimeSpan(0,5,0);
                var limitDateTime = DateTime.Now - fiveMinutes;
                foreach (var i in Vm.MmeTiles)
                {
                    Brush brush = Brushes.Gray;
                    var q = _db.MonitoringEvents.ToList();
                    /*if (_db.MonitoringEvents.FirstOrDefault(m => m.MonitoringEventType.EventName == MonitoringEventType.HEART_BEAT_EVENT_NAME && m.MmeCode == i.Name &&
                                                                  m.Timestamp > limitDateTime) != null)
                    {
                        brush = _db.MonitoringEvents.FirstOrDefault(m => m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && m.MmeCode == i.Name &&
                                                                         m.Timestamp > limitDateTime) != null ? Brushes.Orange : Brushes.Green;
                    }*/

                    i.Color = brush;
                }
            }
        }

        private void MmeTile_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime d1, d2;
            var mmeTile = Vm.SelectedMmeTile;
            var monitoringEventsStart = _db.MonitoringEvents.Where(m=> m.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME);

            mmeTile.LastStartTimestamp = _db.MonitoringEvents.Where(m => m.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Timestamp;
            
            mmeTile.SWVersionAtLastStart= _db.MonitoringEvents.Where(m => m.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Data4;

            if(mmeTile.LastStartTimestamp != null)
                mmeTile.TestCounterSinceLastStart = _db.MonitoringEvents.Count(m => m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && m.Timestamp >= mmeTile.LastStartTimestamp.Value);
            mmeTile.TestCounterTotal = _db.MonitoringEvents.Count(m => m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME);
            mmeTile.TestCounter = _db.MonitoringEvents.Count(m => m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && m.Timestamp > mmeTile.TestCounterBeginDateTime && m.Timestamp < mmeTile.TestCounterEndDateTime);

            mmeTile.WorkingHoursSinceLastStart = _db.MonitoringStats.Single(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS).ValueData / 60;
            mmeTile.WorkingHoursTotal = _db.MonitoringStats.Single(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.TOTAL_HOURS).ValueData / 60;
            d1 = mmeTile.WorkingHoursBeginDateTime.Date;
            d2 = mmeTile.WorkingHoursEndDateTime.Date;
            mmeTile.WorkingHours = _db.MonitoringStats.Where(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.DAY_HOURS 
                                                                                            && m.KeyData.Date >= d1.Date && m.KeyData.Date <= d2.Date).Sum(m => m.ValueData) / 60;
            
            mmeTile.HardwareErrorCounterTotal = _db.MonitoringEvents.Count(m => m.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME);
            mmeTile.HardwareErrorCounter = _db.MonitoringEvents.Count(m => m.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME 
                                                                           && m.Timestamp.Date >= mmeTile.HardwareErrorCounterBeginDateTime.Date && m.Timestamp.Date <= mmeTile.HardwareErrorCounterEndDateTime.Date);
            
            mmeTile.LastTestTimestamp = _db.MonitoringEvents.Where(m => m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Timestamp;
            mmeTile.LastState = _db.MonitoringStats.Single(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS).KeyData;
            
            mmeTile.ActiveProfilesCount = _db.MonitoringEvents.Where(m => m.MonitoringEventType.EventName == MonitoringEventType.SYNC_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Data1;
            
            var dateTime = _db.MonitoringEvents.Where(m => m.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Data1;
            if(dateTime != null)
                mmeTile.LastSwUpdateTimestamp = DateTime.FromBinary(dateTime.Value);
            
                
          
            
            
          

           
        }
    }
}