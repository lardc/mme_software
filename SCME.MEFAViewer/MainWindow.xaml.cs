using Microsoft.EntityFrameworkCore;
using SCME.MEFADB;
using SCME.MEFADB.Tables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SCME.MEFAViewer
{
    public partial class MainWindow : Window
    {
        //Контекст данных
        private readonly MonitoringContext DbContext;
        //Таймер опроса комплексов
        private readonly DispatcherTimer PollTimer;

        /// <summary>Инициализирует новый экземпляр класса MainWindow</summary>
        public MainWindow()
        {
            InitializeComponent();
            DbContext = new MonitoringContext(new DbContextOptionsBuilder<MonitoringContext>().UseSqlServer(App.AppSettings.ConnectionString).Options);
            try
            {
                //Запрос списка комплексов
                List<MmeCode> MmeList = DbContext.MmeCodesTry.ToList();
                ViewModel.MmeTiles = new ObservableCollection<MmeTile>(MmeList.Select(mme => new MmeTile()
                {
                    Id = mme.MmeCodeId,
                    Name = mme.Name
                }));
            }
            catch
            {
                ConnectionStatus_Show(false, true);
                return;
            }
            ConnectionStatus_Show(true, true);
            //Обновление данных комплексов
            MmeData_Refresh();
            //Запуск таймера опроса
            PollTimer = new DispatcherTimer(TimeSpan.FromMinutes(1), DispatcherPriority.DataBind, PollTimer_Tick, Dispatcher);
            PollTimer.Start();
        }

        /// <summary>Модель представления</summary>
        public MainWindowVm ViewModel
        {
            get; set;
        } = new MainWindowVm();

        private void ConnectionStatus_Show(bool result, bool initialStart = false) //Отображение состояния связи с базой данных
        {
            DbConnection.Text = string.Format("Состояние связи с БД: {0}", result ? "подключено" : "нет связи");
            DbConnection.Foreground = new SolidColorBrush(result ? Colors.Green : Colors.Red);
            if (!result && initialStart)
                MessageBox.Show("Не удалось получить данные из БД", "Ошибка");
        }

        private void PollTimer_Tick(object sender, EventArgs e) //Таймер опроса комплексов
        {
            MmeData_Refresh();
        }

        private async void MmeData_Refresh() //Обновление данных комплексов
        {
            //Результат выполнения операции
            bool Result = false;
            await Task.Run(() =>
            {
                //5-минутный интервал
                TimeSpan FiveMinutes = new TimeSpan(0, 5, 0);
                DateTime LimitDateTime = DateTime.Now - FiveMinutes;
                try
                {
                    //Перебор всех комплексов
                    foreach (MmeTile Mme in ViewModel.MmeTiles)
                    {
                        //Цвет плитки на форме
                        Brush BackgroundColor = Brushes.WhiteSmoke;
                        //Данные последнего запуска комплекса
                        MonitoringEvent MonitoringEventsStart = DbContext.MonitoringEvents.Where(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME).OrderByDescending(mme => mme.Timestamp).FirstOrDefault();
                        //Были измерения
                        if (DbContext.MonitoringEvents.FirstOrDefault(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && mme.Timestamp > LimitDateTime) != null)
                        {
                            MmeTile_Color(Mme, Brushes.Orange, MonitoringEventsStart?.Data4);
                            continue;
                        }
                        //Комплекс простаивает
                        if (MonitoringEventsStart != null)
                        {
                            //Аптайм
                            DateTime Uptime = DbContext.MonitoringStats.Single(mme => mme.MmeCode == Mme.Name && mme.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS).KeyData;
                            //Проверка простоя более 5 минут
                            BackgroundColor = DateTime.Now - Uptime < FiveMinutes ? Brushes.LightGreen : Brushes.WhiteSmoke;
                        }
                        MmeTile_Color(Mme, BackgroundColor, MonitoringEventsStart?.Data4);
                        //Обновление таблицы данных комплекса
                        MmeTableData_Refresh(Mme);
                    }
                    Result = true;
                }
                catch { }
            });
            ConnectionStatus_Show(Result);
        }

        private void MmeTile_Color(MmeTile mme, Brush backgroundColor, string version) //Раскрашивание плиток комплексов
        {
            mme.Color = backgroundColor;
            mme.SWVersionAtLastStart = version;

        }

        private void MmeTableData_Refresh(MmeTile mmeTile) //Обновление таблицы данных комплекса
        {
            try
            {
                MmeTile Mme = mmeTile;
                DateTime PeriodStart, PeriodEnd;
                //Данные последнего запуска комплекса
                MonitoringEvent MonitoringEventsStart = DbContext.MonitoringEvents.Where(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.START_EVENT_NAME).OrderByDescending(mme => mme.Timestamp).FirstOrDefault();
                //Установленная версия
                Mme.SWVersionAtLastStart = MonitoringEventsStart?.Data4;
                //Дата обновления
                long? Timestamp = MonitoringEventsStart?.Data1;
                if (Timestamp != null)
                    Mme.LastSwUpdateTimestamp = DateTime.FromBinary(Timestamp.Value);
                //Последний запуск
                Mme.LastStartTimestamp = MonitoringEventsStart?.Timestamp;
                //Последнее состояние
                Mme.LastState = DbContext.MonitoringEventTypes.Where(mme => mme.Id == DbContext.MonitoringEvents.Where(mme => mme.MmeCode == Mme.Name).OrderByDescending(mme => mme.Timestamp).FirstOrDefault().MonitoringEventTypeId).FirstOrDefault()?.EventName;
                //Аптайм
                Mme.WorkingHoursSinceLastStart = TimeSpan.FromMinutes(DbContext.MonitoringStats.Single(mme => mme.MmeCode == Mme.Name && mme.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS).ValueData);
                //Суммарный аптайм
                Mme.WorkingHoursTotal = TimeSpan.FromMinutes(DbContext.MonitoringStats.Single(mme => mme.MmeCode == Mme.Name && mme.MonitoringStatType.StatName == MonitoringStatType.TOTAL_HOURS).ValueData);
                //Аптайм за период
                PeriodStart = Mme.WorkingHoursBeginDateTime.Date;
                PeriodEnd = Mme.WorkingHoursEndDateTime.Date;
                Mme.WorkingHours = TimeSpan.FromMinutes(DbContext.MonitoringStats.Where(mme => mme.MmeCode == Mme.Name && mme.MonitoringStatType.StatName == MonitoringStatType.DAY_HOURS && mme.KeyData.Date >= PeriodStart.Date && mme.KeyData.Date <= PeriodEnd.Date).Sum(mme => mme.ValueData));
                //Число измерений за сессию
                if (Mme.LastStartTimestamp != null)
                    Mme.TestCounterSinceLastStart = DbContext.MonitoringEvents.Count(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && mme.Timestamp >= Mme.LastStartTimestamp.Value);
                //Суммарное число измерений
                Mme.TestCounterTotal = DbContext.MonitoringEvents.Count(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME);
                //Число измерений за период
                PeriodStart = Mme.TestCounterBeginDateTime.Date;
                PeriodEnd = Mme.TestCounterEndDateTime.Date;
                Mme.TestCounter = DbContext.MonitoringEvents.Count(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && mme.Timestamp > PeriodStart && mme.Timestamp < PeriodEnd);
                //Суммарное число ошибок
                Mme.HardwareErrorCounterTotal = DbContext.MonitoringEvents.Count(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME);
                //Число ошибок за период
                PeriodStart = Mme.HardwareErrorCounterBeginDateTime.Date;
                PeriodEnd = Mme.HardwareErrorCounterEndDateTime.Date;
                Mme.HardwareErrorCounter = DbContext.MonitoringEvents.Count(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME && mme.Timestamp.Date >= PeriodStart && mme.Timestamp.Date <= PeriodEnd);
                //Дата последнего измерения
                Mme.LastTestTimestamp = DbContext.MonitoringEvents.Where(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME).OrderByDescending(mme => mme.Timestamp).FirstOrDefault()?.Timestamp;
                //Число активных профилей
                Mme.ActiveProfilesCount = DbContext.MonitoringEvents.Where(mme => mme.MonitoringEventType.EventName == MonitoringEventType.SYNC_EVENT_NAME).OrderByDescending(mme => mme.Timestamp).FirstOrDefault()?.Data1;
            }
            catch { }
        }

        private void UptimePeriod_Changed(object sender, SelectionChangedEventArgs e) //Выбор периода аптайма
        {
            try
            {
                DateTime PeriodStart, PeriodEnd;
                MmeTile Mme = ViewModel.SelectedMmeTile;
                if (Mme == null)
                    return;
                //Аптайм за период
                PeriodStart = Mme.WorkingHoursBeginDateTime.Date;
                PeriodEnd = Mme.WorkingHoursEndDateTime.Date;
                Mme.WorkingHours = TimeSpan.FromMinutes(DbContext.MonitoringStats.Where(mme => mme.MmeCode == Mme.Name && mme.MonitoringStatType.StatName == MonitoringStatType.DAY_HOURS && mme.KeyData.Date >= PeriodStart.Date && mme.KeyData.Date <= PeriodEnd.Date).Sum(mme => mme.ValueData));
            }
            catch { }
        }

        private void TestCountPeriod_Changed(object sender, SelectionChangedEventArgs e) //Выбор периода измерений
        {
            try
            {
                DateTime PeriodStart, PeriodEnd;
                MmeTile Mme = ViewModel.SelectedMmeTile;
                if (Mme == null)
                    return;
                //Число измерений за период
                PeriodStart = Mme.TestCounterBeginDateTime.Date;
                PeriodEnd = Mme.TestCounterEndDateTime.Date;
                Mme.TestCounter = DbContext.MonitoringEvents.Count(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.TEST_EVENT_NAME && mme.Timestamp > PeriodStart && mme.Timestamp < PeriodEnd);
            }
            catch { }
        }

        private void HWErrorCountPeriod_Changed(object sender, SelectionChangedEventArgs e) //Выбор периода ошибок
        {
            try
            {
                DateTime PeriodStart, PeriodEnd;
                MmeTile Mme = ViewModel.SelectedMmeTile;
                if (Mme == null)
                    return;
                //Число ошибок за период
                PeriodStart = Mme.HardwareErrorCounterBeginDateTime.Date;
                PeriodEnd = Mme.HardwareErrorCounterEndDateTime.Date;
                Mme.HardwareErrorCounter = DbContext.MonitoringEvents.Count(mme => mme.MmeCode == Mme.Name && mme.MonitoringEventType.EventName == MonitoringEventType.ERROR_EVENT_NAME && mme.Timestamp.Date >= PeriodStart && mme.Timestamp.Date <= PeriodEnd);
            }
            catch { }
        }
    }
}