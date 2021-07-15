using Microsoft.EntityFrameworkCore;
using SCME.MEFADB;
using SCME.MEFADB.Tables;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 30);
            _dispatcherTimer.Tick += RefreshMmeOnTick;
            _dispatcherTimer.Start();
            RefreshMme();
        }

        private void RefreshMmeOnTick(object sender, EventArgs e)
        {
            RefreshMme();
        }

        private void RefreshMme()
        {
            RefreshMmeTable();
            var fiveMinutes = new TimeSpan(0, 5, 0);
            var limitDateTime = DateTime.Now - fiveMinutes;
            foreach (MmeTile i in Vm.MmeTiles)
            {
                Brush brush = Brushes.WhiteSmoke;
                MonitoringEvent monitoringEventsStart = _db.MonitoringEvents.Where(m => m.MmeCode == i.Name && m.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault();
                //Были измерения
                if (_db.MonitoringEvents.FirstOrDefault(m => m.MmeCode == i.Name && m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && m.Timestamp > limitDateTime) != null)
                {
                    brush = Brushes.Orange;
                    i.Color = brush;
                    i.SWVersionAtLastStart = monitoringEventsStart?.Data4;
                    continue;
                }
                //Комплекс простаивает
                if (monitoringEventsStart != null)
                {
                    //Аптайм
                    DateTime Uptime = _db.MonitoringStats.Single(m => m.MmeCode == i.Name && m.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS).KeyData;
                    brush = DateTime.Now - Uptime < fiveMinutes ? Brushes.LightGreen : Brushes.WhiteSmoke;
                }
                i.Color = brush;
                i.SWVersionAtLastStart = monitoringEventsStart?.Data4;
            }
        }

        private void RefreshMmeTable()
        {
            try
            {
                MmeTile mmeTile = Vm.SelectedMmeTile;
                if (mmeTile == null)
                    return;
                DateTime d1, d2;
                MonitoringEvent monitoringEventsStart = _db.MonitoringEvents.Where(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault();
                //Установленная версия
                mmeTile.SWVersionAtLastStart = monitoringEventsStart?.Data4;
                //Дата обновления
                long? dateTime = monitoringEventsStart?.Data1;
                if (dateTime != null)
                    mmeTile.LastSwUpdateTimestamp = DateTime.FromBinary(dateTime.Value);
                //Последний запуск
                mmeTile.LastStartTimestamp = monitoringEventsStart?.Timestamp;
                //Последнее состояние
                mmeTile.LastState = _db.MonitoringEventTypes.Single(m => m.Id == _db.MonitoringEvents.Where(m => m.MmeCode == mmeTile.Name).OrderByDescending(m => m.Timestamp).FirstOrDefault().MonitoringEventTypeId).EventName;
                //Аптайм
                mmeTile.WorkingHoursSinceLastStart = TimeSpan.FromMinutes(_db.MonitoringStats.Single(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS).ValueData);
                //Суммарный аптайм
                mmeTile.WorkingHoursTotal = TimeSpan.FromMinutes(_db.MonitoringStats.Single(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.TOTAL_HOURS).ValueData);
                //Аптайм за период
                d1 = mmeTile.WorkingHoursBeginDateTime.Date;
                d2 = mmeTile.WorkingHoursEndDateTime.Date;
                mmeTile.WorkingHours = TimeSpan.FromMinutes(_db.MonitoringStats.Where(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.DAY_HOURS && m.KeyData.Date >= d1.Date && m.KeyData.Date <= d2.Date).Sum(m => m.ValueData));
                //Число измерений за сессию
                if (mmeTile.LastStartTimestamp != null)
                    mmeTile.TestCounterSinceLastStart = _db.MonitoringEvents.Count(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && m.Timestamp >= mmeTile.LastStartTimestamp.Value);
                //Суммарное число измерений
                mmeTile.TestCounterTotal = _db.MonitoringEvents.Count(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME);
                //Число измерений за период
                mmeTile.TestCounter = _db.MonitoringEvents.Count(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && m.Timestamp > mmeTile.TestCounterBeginDateTime && m.Timestamp < mmeTile.TestCounterEndDateTime);
                //Суммарное число ошибок
                mmeTile.HardwareErrorCounterTotal = _db.MonitoringEvents.Count(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME);
                //Число ошибок за период
                mmeTile.HardwareErrorCounter = _db.MonitoringEvents.Count(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME && m.Timestamp.Date >= mmeTile.HardwareErrorCounterBeginDateTime.Date && m.Timestamp.Date <= mmeTile.HardwareErrorCounterEndDateTime.Date);
                //Дата последнего измерения
                mmeTile.LastTestTimestamp = _db.MonitoringEvents.Where(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Timestamp;
                //Число активных профилей
                mmeTile.ActiveProfilesCount = _db.MonitoringEvents.Where(m => m.MonitoringEventType.EventName == MonitoringEventType.SYNC_EVENT_NAME).OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Data1;
            }
            catch { }
        }

        private void MmeTile_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshMme();
        }

        private void UptimePeriod_Changed(object sender, SelectionChangedEventArgs e)
        {
            DateTime d1, d2;
            MmeTile mmeTile = Vm.SelectedMmeTile;
            if (mmeTile == null)
                return;
            //Аптайм за период
            d1 = mmeTile.WorkingHoursBeginDateTime.Date;
            d2 = mmeTile.WorkingHoursEndDateTime.Date;
            mmeTile.WorkingHours = TimeSpan.FromMinutes(_db.MonitoringStats.Where(m => m.MmeCode == mmeTile.Name && m.MonitoringStatType.StatName == MonitoringStatType.DAY_HOURS && m.KeyData.Date >= d1.Date && m.KeyData.Date <= d2.Date).Sum(m => m.ValueData));
        }

        private void TestCountPeriod_Changed(object sender, SelectionChangedEventArgs e)
        {
            MmeTile mmeTile = Vm.SelectedMmeTile;
            if (mmeTile == null)
                return;
            //Число измерений за период
            mmeTile.TestCounter = _db.MonitoringEvents.Count(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && m.Timestamp > mmeTile.TestCounterBeginDateTime && m.Timestamp < mmeTile.TestCounterEndDateTime);
        }

        private void HWErrorCountPeriod_Changed(object sender, SelectionChangedEventArgs e)
        {
            MmeTile mmeTile = Vm.SelectedMmeTile;
            if (mmeTile == null)
                return;
            //Число ошибок за период
            mmeTile.HardwareErrorCounter = _db.MonitoringEvents.Count(m => m.MmeCode == mmeTile.Name && m.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME && m.Timestamp.Date >= mmeTile.HardwareErrorCounterBeginDateTime.Date && m.Timestamp.Date <= mmeTile.HardwareErrorCounterEndDateTime.Date);
        }
    }
}