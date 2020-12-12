using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.DataTemplates.TestParameters;
using SCME.WpfControlLibrary.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
using HtmlAgilityPack;
using SCME.WpfControlLibrary.DataProviders;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Threading;

namespace SCME.WpfControlLibrary.Pages
{
    /// <summary>
    /// Логика взаимодействия для SSRTUResultPage.xaml
    /// </summary>
    public partial class SSRTUResultPage : Page
    {

        Func<bool> NeedStartFunc;
        private Action _start;
        public Action Stop { get; set; }

        private readonly string _userName;
        private readonly string _mme;
        private Profile _profile;
        private DispatcherTimer _dispatcherTimerNeedStart = new DispatcherTimer();

        public SSRTUResultVM VM { get; set; } = new SSRTUResultVM();
        public SSRTUResultComponentVM VMPosition1 { get; set; } = new SSRTUResultComponentVM() { Positition = 1};
        public SSRTUResultComponentVM VMPosition2 { get; set; } = new SSRTUResultComponentVM() { Positition = 2 };
        public SSRTUResultComponentVM VMPosition3 { get; set; } = new SSRTUResultComponentVM() { Positition = 3 };
        public SSRTUResultComponentVM VMPosition4 { get; set; } = new SSRTUResultComponentVM() { Positition = 4 };

        public Dictionary<int, SSRTUResultComponentVM> VMByPosition{ get; set; }
        public SSRTUResultPage()
        {
            InitializeComponent();
        }

        public SSRTUResultPage(string userName,string mme, Profile profile, Action start, Action stop, Func<bool> needStartFunc)
        {
            NeedStartFunc = needStartFunc;
            _dispatcherTimerNeedStart.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimerNeedStart.Tick += Dt_Tick;
            _dispatcherTimerNeedStart.Start();
            InitializeComponent();

            reportFolder = SCME.UIServiceConfig.Properties.Settings.Default.ReportFolder;
            if (Directory.Exists(reportFolder) == false)
                reportFolder = Directory.GetCurrentDirectory();    
            
            _start = start;
            _userName = userName;
            _mme = mme;
            _profile = profile;
            Stop = stop;
            VMByPosition = new Dictionary<int, SSRTUResultComponentVM>();
            VMByPosition[1] = VMPosition1;
            VMByPosition[2] = VMPosition2;
            VMByPosition[3] = VMPosition3;
            VMByPosition[4] = VMPosition4;
            foreach (var i in VMByPosition)
                i.Value.DutPackageType = profile.DutPackageType;

            foreach (var i in profile.TestParametersAndNormatives)
            {
                var sSRTUResultComponentVM = VMByPosition[i.NumberPosition];
                switch (i)
                {
                    case SCME.Types.InputOptions.TestParameters j:
                        if (j.TypeManagement == Types.TypeManagement.DCAmperage)
                        {
                            var inputOptions = sSRTUResultComponentVM.InputVoltages[i.Index-1];
                            inputOptions.Min = j.InputVoltageMinimum;
                            inputOptions.Max = j.InputVoltageMaximum;
                            inputOptions.Value = double.Epsilon;
                            foreach (var t in VMByPosition.Values)
                                t.ShowInputAmperage = false;
                        }
                        else
                        {
                            var inputOptions = sSRTUResultComponentVM.InputAmperages[i.Index-1];
                            inputOptions.Min = j.InputCurrentMinimum;
                            inputOptions.Max = j.InputCurrentMaximum;
                            inputOptions.Value = double.Epsilon;
                            foreach (var t in VMByPosition.Values)
                                t.ShowInputAmperage = true;
                        }
                        break;
                    case SCME.Types.AuxiliaryPower.TestParameters j:
                        if (j.ShowAuxiliaryVoltagePowerSupply1)
                        {
                            VMPosition4.AuxiliaryCurrentPowerSupplyMin1 = j.AuxiliaryCurrentPowerSupplyMinimum1;
                            VMPosition4.AuxiliaryCurrentPowerSupplyMax1 = j.AuxiliaryCurrentPowerSupplyMaximum1;
                            VMPosition4.AuxiliaryCurrentPowerSupply1 = double.Epsilon;
                        }
                        if (j.ShowAuxiliaryVoltagePowerSupply2)
                        {
                            VMPosition4.AuxiliaryCurrentPowerSupplyMin2 = j.AuxiliaryCurrentPowerSupplyMinimum2;
                            VMPosition4.AuxiliaryCurrentPowerSupplyMax2 = j.AuxiliaryCurrentPowerSupplyMaximum2;
                            VMPosition4.AuxiliaryCurrentPowerSupply2 = double.Epsilon;
                        }
                        break;
                    case SCME.Types.OutputLeakageCurrent.TestParameters j:
                        var leakageCurrent = sSRTUResultComponentVM.LeakageCurrents[i.Index-1];
                        leakageCurrent.Min = j.LeakageCurrentMinimum;
                        leakageCurrent.Max = j.LeakageCurrentMaximum;
                        leakageCurrent.Value = double.Epsilon;
                        break;
                    case SCME.Types.OutputResidualVoltage.TestParameters j:
                        var residualVoltage = sSRTUResultComponentVM.ResidualVoltages[i.Index-1];
                        if (j.OpenState)
                        {
                            residualVoltage.MinEx = j.OpenResistanceMinimum;
                            residualVoltage.MaxEx = j.OpenResistanceMaximum;
                            residualVoltage.ValueEx = double.Epsilon;
                        }
                        else
                        {
                            residualVoltage.Min = j.OutputResidualVoltageMinimum;
                            residualVoltage.Max = j.OutputResidualVoltageMaximum;
                            residualVoltage.Value = double.Epsilon;
                        }
                        break;
                    default:
                        break;
                }
            }
            
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            if (VM.CanStart && NeedStartFunc())
            {
                _dispatcherTimerNeedStart.Stop();
                Start_Click(null, null);
            }
        }

        public void PostSSRTUNotificationEvent(string message, ushort problem, ushort warning, ushort fault, ushort disable)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogWindow = new DialogWindow("Ошибка оборудования", $"{message}\r\n problem {problem}, warning {warning}, fault {fault}, disable {disable}"); ;
                dialogWindow.ShowDialog();
                VM.CanStart = true;
                try
                {
                    VMPosition4.ErrorCode = problem.ToString();  
                }
                catch(Exception ex)
                {

                }
                
            }));
        }
    

        public void PostAlarmEvent()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogWindow = new DialogWindow("Внимание", "Нарушен периметр безопасности");
                dialogWindow.ShowDialog();
                VM.CanStart = true;
            }));
        }

        private int countEndingTests;
        public void SSRTUHandler(DeviceState deviceState, Types.SSRTU.TestResults testResults)
        {
            var q = VMByPosition[testResults.NumberPosition];

            switch (testResults.TestParametersType)
            {
                case TestParametersType.AuxiliaryPower:
                    if (VMPosition4.ShowAuxiliaryCurrentPowerSupply1)
                        VMPosition4.AuxiliaryCurrentPowerSupply1 = testResults.AuxiliaryCurrentPowerSupply1;
                    if (VMPosition4.ShowAuxiliaryCurrentPowerSupply2)
                        VMPosition4.AuxiliaryCurrentPowerSupply2 = testResults.AuxiliaryCurrentPowerSupply2;
                    break;
                case TestParametersType.InputOptions:
                    if (testResults.InputOptionsIsAmperage)
                        q.InputAmperages[testResults.Index - 1].Value = testResults.Value;
                    else
                        q.InputVoltages[testResults.Index - 1].Value = testResults.Value;
                    break;
                case TestParametersType.OutputLeakageCurrent:
                    q.LeakageCurrents[testResults.Index-1].Value = testResults.Value;
                    break;
                case TestParametersType.OutputResidualVoltage:
                    var residualVoltage = q.ResidualVoltages[testResults.Index - 1];
                    if(residualVoltage.MinEx != null)
                        residualVoltage.ValueEx = testResults.OpenResistance;
                    else
                        residualVoltage.Value = testResults.Value;
                    break;
                default:
                    break;
            }

            if (countEndingTests == 0)
                results.Add(new Dictionary<int, SSRTUResultComponentVM>());

            if (++countEndingTests == _profile.TestParametersAndNormatives.Count)
            {
                VM.CanStart = true;
                var result = results.Last();
                result[0] = VMPosition1.IsEmpty ? null : VMPosition1.Copy();
                result[1] = VMPosition2.IsEmpty ? null : VMPosition2.Copy();
                result[2] = VMPosition3.IsEmpty ? null : VMPosition3.Copy();
                result[3] = VMPosition4.IsEmpty ? null : VMPosition4.Copy();
                result.First(m => m.Value != null).Value.SerialNumber = VM.SerialNumber;
                if (string.IsNullOrEmpty(VMPosition4.ErrorCode) == false)
                    result.Values.First(m => m != null).ErrorCode = VMPosition4.ErrorCode;
                CreateReport();
                _dispatcherTimerNeedStart.Start();
                VM.SerialNumber++;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            VM.CanStart = true;


        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            foreach (var i in _profile.TestParametersAndNormatives)
            {
                var sSRTUResultComponentVM = VMByPosition[i.NumberPosition];

                foreach (var j in sSRTUResultComponentVM.LeakageCurrents.Where(j => j.Min != null))
                    j.Value = double.Epsilon;
                
                foreach (var j in sSRTUResultComponentVM.InputAmperages.Where(j => j.Min != null))
                    j.Value = double.Epsilon;
                
                foreach (var j in sSRTUResultComponentVM.InputVoltages.Where(j => j.Min != null))
                    j.Value = double.Epsilon;

                foreach (var j in sSRTUResultComponentVM.ResidualVoltages.Where(j => j.Min != null))
                    j.Value = double.Epsilon;
                
                foreach (var j in sSRTUResultComponentVM.ResidualVoltages.Where(j => j.MinEx != null))
                    j.ValueEx = double.Epsilon;

                if (sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMin1 != null)
                    sSRTUResultComponentVM.AuxiliaryCurrentPowerSupply1 = double.Epsilon;

                if (sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMin2 != null)
                    sSRTUResultComponentVM.AuxiliaryCurrentPowerSupply2 = double.Epsilon;

            }

                VM.CanStart = false;
            countEndingTests = 0;
            _start();
            return;
            /*VM.CanStart = false;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);

                VM.CanStart = true;
            });
            return;
            Random random = new Random(DateTime.Now.Millisecond);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                VMPosition1.InputAmperage = random.Next(0, 6);
                VMPosition2.InputVoltage = random.Next(0, 6);
                VMPosition3.LeakageCurrent = random.Next(0, 6);
                VM.CanStart = true;
            });*/
        }


        private HtmlDocument _doc = new HtmlDocument();
        private List<Dictionary<int, SSRTUResultComponentVM>> results = new List<Dictionary<int, SSRTUResultComponentVM>>();
        private DateTime _dateTimeBeginMeasurement;
        private string reportFolder;
        

        private void CreateReport()
        {
            //Верхняя подпись
            _dateTimeBeginMeasurement = DateTime.Now;
            HtmlNode tr;
            var body = _doc.CreateElement("body");

            var tmpHtmlBody = _doc.CreateElement("h1");
            tmpHtmlBody.InnerHtml = "Протокол испытаний";
            body.AppendChild(tmpHtmlBody);

            tmpHtmlBody = _doc.CreateElement("p");
            tmpHtmlBody.InnerHtml = _dateTimeBeginMeasurement.ToShortDateString();
            body.AppendChild(tmpHtmlBody);

            tmpHtmlBody = _doc.CreateElement("p");
            tmpHtmlBody.InnerHtml = $"Профиль испытания: Профиль {_profile.Name}";
            body.AppendChild(tmpHtmlBody);

            tmpHtmlBody = _doc.CreateElement("p");
            tmpHtmlBody.InnerHtml = $"Номер партии: {VM.BatchNumber}";
            body.AppendChild(tmpHtmlBody);

            tmpHtmlBody = _doc.CreateElement("p");
            tmpHtmlBody.InnerHtml = $"Оператор: {_userName}";
            body.AppendChild(tmpHtmlBody);

            tmpHtmlBody = _doc.CreateElement("p");
            tmpHtmlBody.InnerHtml = $"Оборудование: {_mme}";
            body.AppendChild(tmpHtmlBody);

            //Параметры
            var table = _doc.CreateElement("table");
            table.SetAttributeValue("id", "table1");
            table.SetAttributeValue("class", "center");
            table.SetAttributeValue("style", "width:96%;");

            var tbody = _doc.CreateElement("tbody");
            table.AppendChild(tbody);

            AddParameters(tbody);

            body.AppendChild(table);

            //Результаты
            var h2 = _doc.CreateElement("h2");
            h2.InnerHtml = "Результаты";
            body.AppendChild(h2);

            table = _doc.CreateElement("table");
            table.SetAttributeValue("id", "table1");
            table.SetAttributeValue("class", "center");
            table.SetAttributeValue("style", "width:96%;");

            tbody = _doc.CreateElement("tbody");
            table.AppendChild(tbody);

            body.AppendChild(table);

            AddHeadersResult(results.First().Values, tbody);
            foreach (var i in results)
                AddLineValues(i.Values, results.IndexOf(i) + 1, tbody);

            //Нижняя подпись
            table = _doc.CreateElement("table");
            table.SetAttributeValue("id", "table2");
            table.SetAttributeValue("class", "center");
            table.SetAttributeValue("style", "width:96%;border:null");
            body.AppendChild(table);

            tbody = _doc.CreateElement("tbody");
            table.AppendChild(tbody);

            tr = _doc.CreateElement("tr");
            tbody.AppendChild(tr);

            AddCellTdString(Assembly.GetExecutingAssembly().GetName().Version.ToString(), tr, new Dictionary<string, string>()
            {
                {"style","width:33%" },
                {"align","left" }
            });
            AddCellTdString("АО \"ПРОТОН - ЭЛЕКТРОТЕКС\"©", tr, new Dictionary<string, string>()
            {
                {"style","width:34%" },
                {"align","center" }
            });
            AddCellTdString("HTML Report Generator", tr, new Dictionary<string, string>()
            {
                {"style","width:33%" },
                {"align","right" }
            });



            //string fileName = $@"{_dateTimeBeginMeasurement.ToString("yyyy-MM-dd-HH-mm")}-{(string.IsNullOrEmpty(VM.BatchNumber) ? "NoBatchNumber" : VM.BatchNumber)}.html";
            string fileName = $@"{_profile.Name}-{_dateTimeBeginMeasurement:yyyy-MM-dd-HH-mm}.html";
            File.WriteAllText(System.IO.Path.Combine(reportFolder, fileName), File.ReadAllText("ReportTemplate.html").Replace("body", body.OuterHtml));




        }

        private HtmlNode AddNumberPositions(BaseTestParametersAndNormatives[] parameters)
        {
            var tr = _doc.CreateElement("tr");
            for (var i = 0; i < parameters.Length; i++)
            {
                var td = _doc.CreateElement("td");
                td.InnerHtml = $"Канал {parameters[i].NumberPosition}";
                td.SetAttributeValue("align", "center"); 
                td.SetAttributeValue("colspan", "2");
                tr.AppendChild(td);
            }
            return tr;
        }

       

        private void AddCellTdString(double? value, HtmlNode tr)
        {
            var td = _doc.CreateElement("td");

            if (value == null)
                td.InnerHtml = "-";
            else
                td.InnerHtml = Math.Round(value.Value,6).ToString();

            tr.AppendChild(td);
        }
        private void AddCellTdString(string value, HtmlNode tr, Dictionary<string, string> attributes = null)
        {
            var td = _doc.CreateElement("td");
            td.InnerHtml = value;
            tr.AppendChild(td);
            if (attributes != null)
                foreach (var i in attributes)
                    td.SetAttributeValue(i.Key, i.Value);
        }
        private void AddCellThString(string value, HtmlNode tr)
        {
            var th = _doc.CreateElement("th");
            th.InnerHtml = value;
            tr.AppendChild(th);
        }

        private HtmlNode AddLineValues(string name, IEnumerable<string> valuesI)
        {
            var values = valuesI.ToArray();
            var tr = _doc.CreateElement("tr");
            for (var i = 0; i < values.Length; i++)
            {
                var td = _doc.CreateElement("td");
                td.InnerHtml = name;
                tr.AppendChild(td);
                td = _doc.CreateElement("td");
                td.InnerHtml = values[i];
                tr.AppendChild(td);
            }
            return tr;
        }

        private HtmlNode AddLineValues2(IEnumerable<string> namesI , IEnumerable<string> valuesI)
        {
            var names = namesI.ToArray();
            var values = valuesI.ToArray();
            var tr = _doc.CreateElement("tr");
            for (var i = 0; i < values.Length; i++)
            {
                var td = _doc.CreateElement("td");
                td.InnerHtml = names[i];
                tr.AppendChild(td);
                td = _doc.CreateElement("td");
                td.InnerHtml = values[i];
                tr.AppendChild(td);
            }
            return tr;
        }

        private void AddLineValues(IEnumerable<SSRTUResultComponentVM> valuesI, int number, HtmlNode tbody)
        {
            var values = valuesI.Where(m => m != null && m.Positition != 4).OrderBy(m=> m.Positition).ToList();
            var tr = _doc.CreateElement("tr");


            var td = _doc.CreateElement("td");
            td.InnerHtml = valuesI.Where(m => m != null).First().SerialNumber.ToString();
            if (values.Count(m => m != null) == 0)
                td.SetAttributeValue("rowspan", "1");
            else
                td.SetAttributeValue("rowspan", values.Count(m => m != null).ToString());
            tr.AppendChild(td);

            var auxiliarPower = valuesI.SingleOrDefault(m => m?.Positition == 4);
            if (values.Count == 0)
            {
                td = _doc.CreateElement("td");
                td.InnerHtml = auxiliarPower.Positition.ToString();
                tr.AppendChild(td);

                if (auxiliarPower != null)
                {
                    if (values.IndexOf(auxiliarPower) == 0)
                    {
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                            AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupply1, tr);
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                            AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupply2, tr);
                    }
                    else
                    {
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                            AddCellTdString("-", tr);
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                            AddCellTdString("-", tr);
                    }
                }

                td = _doc.CreateElement("td");
                if (string.IsNullOrEmpty(valuesI.First(m => m != null).ErrorCode) == false)
                {
                    td.InnerHtml = valuesI.First(m => m != null).ErrorCode;
                    td.SetAttributeValue("style", "background-color:#eb3434");
                }
                if (auxiliarPower.IsGood)
                {
                    td.InnerHtml = "годен";
                    td.SetAttributeValue("style", "background-color:#71fa2d");
                }
                else
                {
                    td.InnerHtml = "не годен";
                    td.SetAttributeValue("style", "background-color:#eb3434");
                }
                tr.AppendChild(td);
                tbody.AppendChild(tr);

                tr = _doc.CreateElement("tr");
            }
            else
                foreach (var i in values)
                {
                    td = _doc.CreateElement("td");
                    td.InnerHtml = i.Positition.ToString();
                    tr.AppendChild(td);

                    AddMinMaxHeader(values.SelectMany(m => m.LeakageCurrents).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.LeakageCurrents, tr, SelectorMinMaxValue.Value);
                    AddMinMaxHeader(values.SelectMany(m => m.InputAmperages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.InputAmperages, tr, SelectorMinMaxValue.Value);
                    AddMinMaxHeader(values.SelectMany(m => m.InputVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.InputVoltages, tr, SelectorMinMaxValue.Value);
                    AddMinMaxHeader(values.SelectMany(m => m.ResidualVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.ResidualVoltages.Where(m => m.Min != null), tr, SelectorMinMaxValue.Value);
                    AddMinMaxExHeader(values.SelectMany(m => m.ResidualVoltages).Where(m => m.MinEx != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.ResidualVoltages.Where(m => m.MinEx != null), tr, SelectorMinMaxValue.Value);

                    if (auxiliarPower != null)
                    {
                        if (values.IndexOf(i) == 0)
                        {
                            if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                                AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupply1, tr);
                            if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                                AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupply2, tr);
                        }
                        else
                        {
                            if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                                AddCellTdString("-", tr);
                            if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                                AddCellTdString("-", tr);
                        }
                    }

                    td = _doc.CreateElement("td");

                    if (string.IsNullOrEmpty(valuesI.First(m => m != null).ErrorCode) == false)
                    {
                        td.InnerHtml = valuesI.First(m => m != null).ErrorCode;
                        td.SetAttributeValue("style", "background-color:#eb3434");
                    }
                    else if (i.IsGood)
                    {
                        td.InnerHtml = "годен";
                        td.SetAttributeValue("style", "background-color:#71fa2d");
                    }
                    else
                    {
                        td.InnerHtml = "не годен";
                        td.SetAttributeValue("style", "background-color:#eb3434");
                    }
                    tr.AppendChild(td);
                    tbody.AppendChild(tr);

                    tr = _doc.CreateElement("tr");

                }
        }


        enum SelectorMinMaxValue
        {
            Min,Max,Value
        }

        private void AddMinMaxHeader(int count, IEnumerable<SSRTUResultComponentVM.Result> result, HtmlNode tr, SelectorMinMaxValue selectorMinMaxValue = SelectorMinMaxValue.Min)
        {
            if (count == 0)
                return;
            int n = 0;
            var resultArray = result.ToArray();
            for (int i = 1; i <= count; i++ )
            {
                if (n < result.Count())
                {
                    var res = resultArray[n];
                    if (res.Index == i)
                    {
                        switch (selectorMinMaxValue)
                        {
                            case SelectorMinMaxValue.Min:
                                AddCellTdString(res.Min, tr);
                                break;
                            case SelectorMinMaxValue.Max:
                                AddCellTdString(res.Max, tr);
                                break;
                            case SelectorMinMaxValue.Value:
                                AddCellTdString(res.Value, tr);
                                break;
                        }
                        n++;
                    }
                    else
                        AddCellTdString("-", tr);
                }
                else
                    AddCellTdString("-", tr);
            }
            //foreach (var j in result.Where(m => m.Min != null))
            //{
            //    switch (selectorMinMaxValue)
            //    {
            //        case SelectorMinMaxValue.Min:
            //            AddCellTdString(j.Min, tr);
            //            break;
            //        case SelectorMinMaxValue.Max:
            //            AddCellTdString(j.Max, tr);
            //            break;
            //        case SelectorMinMaxValue.Value:
            //            AddCellTdString(j.Value, tr);
            //            break;
            //    }
            //    n++;
            //}

            //while (n++ < count)
            //    AddCellTdString("-", tr);
        }

        private void AddMinMaxExHeader(int count, IEnumerable<SSRTUResultComponentVM.ResultResidualVoltage> result, HtmlNode tr, SelectorMinMaxValue selectorMinMaxValue = SelectorMinMaxValue.Min)
        {
            if (count == 0)
                return;
            int n = 0;
            var resultArray = result.ToArray();
            for (int i = 1; i <= count; i++)
            {
                if (n < result.Count())
                {
                    var res = resultArray[n];
                    if (res.Index == i)
                    {
                        switch (selectorMinMaxValue)
                        {
                            case SelectorMinMaxValue.Min:
                                AddCellTdString(res.MinEx, tr);
                                break;
                            case SelectorMinMaxValue.Max:
                                AddCellTdString(res.MaxEx, tr);
                                break;
                            case SelectorMinMaxValue.Value:
                                AddCellTdString(res.ValueEx, tr);
                                break;
                        }
                        n++;
                    }
                    else
                        AddCellTdString("-", tr);
                }
                else
                    AddCellTdString("-", tr);
            }
        }


        private void AddAuxiliarPower(SSRTUResultComponentVM result, HtmlNode tr, bool useMax)
        {

        }

        private void AddHeadersResult(IEnumerable<SSRTUResultComponentVM> valuesI, HtmlNode tbody)
        {
            var values = valuesI.Where(m=> m!= null && m.Positition != 4).OrderBy(m => m.Positition).ToList();
            var tr = _doc.CreateElement("tr");

            var td = _doc.CreateElement("td");
            td.InnerHtml = "Нормы";
            td.SetAttributeValue("rowspan", (values.Count * 2).ToString());
            tr.AppendChild(td);

            var auxiliarPower = valuesI.SingleOrDefault(m => m?.Positition == 4);
            foreach (var i in values)
            {
                AddCellTdString($"Мин{i.Positition}", tr);

                AddMinMaxHeader(values.SelectMany(m => m.LeakageCurrents).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.LeakageCurrents, tr, SelectorMinMaxValue.Min);
                AddMinMaxHeader(values.SelectMany(m => m.InputAmperages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.InputAmperages, tr, SelectorMinMaxValue.Min);
                AddMinMaxHeader(values.SelectMany(m => m.InputVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.InputVoltages, tr, SelectorMinMaxValue.Min);
                AddMinMaxHeader(values.SelectMany(m => m.ResidualVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.ResidualVoltages, tr, SelectorMinMaxValue.Min);
                AddMinMaxExHeader(values.SelectMany(m => m.ResidualVoltages).Where(m => m.MinEx != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.ResidualVoltages, tr, SelectorMinMaxValue.Min);

                if (auxiliarPower != null)
                {
                    if (values.IndexOf(i) == 0)
                    {
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                            AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupplyMin1, tr);
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                            AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupplyMin2, tr);
                    }
                    else
                    {
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                            AddCellTdString("-", tr);
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                            AddCellTdString("-", tr);
                    }
                }

                AddCellTdString("", tr);
                tbody.AppendChild(tr);

                tr = _doc.CreateElement("tr");
                AddCellTdString($"Макс{i.Positition}", tr);

                AddMinMaxHeader(values.SelectMany(m => m.LeakageCurrents).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.LeakageCurrents, tr, SelectorMinMaxValue.Max);
                AddMinMaxHeader(values.SelectMany(m => m.InputAmperages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.InputAmperages, tr, SelectorMinMaxValue.Max);
                AddMinMaxHeader(values.SelectMany(m => m.InputVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.InputVoltages, tr, SelectorMinMaxValue.Max);
                AddMinMaxHeader(values.SelectMany(m => m.ResidualVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.ResidualVoltages, tr, SelectorMinMaxValue.Max);
                AddMinMaxExHeader(values.SelectMany(m => m.ResidualVoltages).Where(m => m.MinEx != null).OrderBy(m => m.Index).GroupBy(m => m.Index).Count(), i.ResidualVoltages, tr, SelectorMinMaxValue.Max);

                if (auxiliarPower != null)
                {
                    if (values.IndexOf(i) == 0)
                    {
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                            AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupplyMax1, tr);
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                            AddCellTdString(auxiliarPower.AuxiliaryCurrentPowerSupplyMax2, tr);
                    }
                    else
                    {
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin1 != null)
                            AddCellTdString("-", tr);
                        if (auxiliarPower.AuxiliaryCurrentPowerSupplyMin2 != null)
                            AddCellTdString("-", tr);
                    }
                }

                AddCellTdString("", tr);
                tbody.AppendChild(tr);
                tr = _doc.CreateElement("tr");
            }

            AddCellThString("Номер", tr);
            AddCellThString("Канал", tr);


            foreach(var i in values.SelectMany(m => m.LeakageCurrents).Where(m=> m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index))
                AddCellThString($"Ток утечки {i.Key}", tr);

            foreach (var i in values.SelectMany(m => m.InputAmperages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index))
                AddCellThString($"Ток входа {i.Key}", tr);

            foreach (var i in values.SelectMany(m => m.InputVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index))
                AddCellThString($"Напряжение входа {i.Key}", tr);

            foreach (var i in values.SelectMany(m => m.ResidualVoltages).Where(m => m.Min != null).OrderBy(m => m.Index).GroupBy(m => m.Index))
                AddCellThString($"Выходное ост. напр. {i.Key}", tr);

            foreach (var i in values.SelectMany(m => m.ResidualVoltages).Where(m => m.MinEx != null).OrderBy(m => m.Index).GroupBy(m => m.Index))
                AddCellThString($"Сопр. в откр. сост. {i.Key}", tr);

            if (auxiliarPower?.AuxiliaryCurrentPowerSupplyMin1 != null)
                AddCellThString($"Ток вспом. пит. 1", tr);

            if (auxiliarPower?.AuxiliaryCurrentPowerSupplyMin2 != null)
                AddCellThString($"Ток вспом. пит. 2", tr);

            AddCellThString("Статус", tr);
            
            tbody.AppendChild(tr);
        }
       
        private void AddParameters(HtmlNode tbody)
        {
            foreach (var i in _profile.TestParametersAndNormatives.GroupBy(m => m.GetType()))
            {
                foreach (var j in i.OrderBy(m => m.NumberPosition).GroupBy(m => m.Index))
                {
                    var t = j.First();

                    var tr = _doc.CreateElement("tr");
                    tbody.AppendChild(tr);

                    var th = _doc.CreateElement("th");
                    th.SetAttributeValue("colspan", "6");
                    var type = Types.TestParameterTypeMeasurement.GetAllList().Single(m => m.Type == t.GetType());
                    if (type.TestParametersType == TestParametersType.AuxiliaryPower)
                        th.InnerHtml = type.Name;
                    else
                        th.InnerHtml = $"{type.Name} {t.Index}";
                    tr.AppendChild(th);

                    tbody.AppendChild(AddNumberPositions(j.ToArray()));

                    switch (t)
                    {
                        case SCME.Types.InputOptions.TestParameters ioType:
                            var io = j.Cast<SCME.Types.InputOptions.TestParameters>();
                            tbody.AppendChild(AddLineValues("Тип управления:", io.Select(m => TestTypeEnumDictionary.GetTypeManagementToString()[m.TypeManagement])));
                            tbody.AppendChild(AddLineValues2(io.Select(m => m.ShowVoltage ? "Напряжение управления, В:" : "Ток управления, мА:"),
                                io.Select(m => m.ShowVoltage ? m.ControlVoltage.ToString() : m.ControlCurrent.ToString())));
                            if (io.First().ShowAuxiliaryVoltagePowerSupply1)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 1, В:", io.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                            if (io.First().ShowAuxiliaryVoltagePowerSupply2)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 2, В:", io.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                            break;

                        case SCME.Types.OutputResidualVoltage.TestParameters rvType:
                            var rv = j.Cast<SCME.Types.OutputResidualVoltage.TestParameters>();
                            tbody.AppendChild(AddLineValues("Тип управления:", rv.Select(m => TestTypeEnumDictionary.GetTypeManagementToString()[m.TypeManagement].ToString())));
                            tbody.AppendChild(AddLineValues2(rv.Select(m => m.ShowVoltage ? "Напряжение управления, В:" : "Ток управления, мА:"), 
                                rv.Select(m => m.ShowVoltage ? m.ControlVoltage.ToString() : m.ControlCurrent.ToString())));
                            tbody.AppendChild(AddLineValues2(rv.Select(m => m.ShowVoltage ? "Ограничение по току управления, мА:" : "Ограничение по напр. управления, В"), 
                                rv.Select(m => m.ShowVoltage ? m.ControlCurrentMaximum.ToString() : m.ControlVoltageMaximum.ToString())));

                            //tbody.AppendChild(AddLineValues("Полярность прил. пост. ком. напр.:", rv.Select(m =>
                            //TestTypeEnumDictionary.GetPolarityDCSwitchingVoltageApplication().ToDictionary(n => n.Value, n => n.Key)[m.PolarityDCSwitchingVoltageApplication])));
                            tbody.AppendChild(AddLineValues("Ком. ток. ампл. знач., мА:", rv.Select(m => m.SwitchedAmperage.ToString())));
                            //tbody.AppendChild(AddLineValues("Ком. напр, В:", rv.Select(m => m.SwitchedVoltage.ToString())));
                            if (rv.First().ShowAuxiliaryVoltagePowerSupply1)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 1, В:", rv.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                            if (rv.First().ShowAuxiliaryVoltagePowerSupply2)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 2, В:", rv.Select(m => m.AuxiliaryVoltagePowerSupply2.ToString())));
                            //tbody.AppendChild(AddLineValues("Форма импульса коммутируемого тока:", rv.Select(m =>
                            //TestTypeEnumDictionary.GetSwitchingCurrentPulseShape().ToDictionary(n => n.Value, n => n.Key)[m.SwitchingCurrentPulseShape])));
                            //tbody.AppendChild(AddLineValues("Длительность имп. ком.тока., мкс:", rv.Select(m => m.SwitchingCurrentPulseDuration.ToString())));
                            break;

                        case SCME.Types.OutputLeakageCurrent.TestParameters lcType:
                            var lc = j.Cast<SCME.Types.OutputLeakageCurrent.TestParameters>();
                            tbody.AppendChild(AddLineValues("Тип управления:", lc.Select(m => TestTypeEnumDictionary.GetTypeManagementToString()[m.TypeManagement])));
                            tbody.AppendChild(AddLineValues2(lc.Select(m => m.ShowVoltage ? "Напряжение управления, В:" : "Ток управления, мА:"),
                                lc.Select(m => m.ShowVoltage ? m.ControlVoltage.ToString() : m.ControlCurrent.ToString())));
                            tbody.AppendChild(AddLineValues2(lc.Select(m => m.ShowVoltage ? "Ограничение по току управления, мА:" : "Ограничение по напр. управления, В"),
                                lc.Select(m => m.ShowVoltage ? m.ControlCurrentMaximum.ToString() : m.ControlVoltageMaximum.ToString())));
                            tbody.AppendChild(AddLineValues("Тип ком. напр. при измер. утечки:.:", lc.Select(m =>
                            TestTypeEnumDictionary.GetApplicationPolarityConstantSwitchingVoltage().ToDictionary(n => n.Value, n => n.Key)[m.ApplicationPolarityConstantSwitchingVoltage])));
                            tbody.AppendChild(AddLineValues("Комм. напряжение, В", lc.Select(m => m.SwitchedVoltage.ToString())));
                            //tbody.AppendChild(AddLineValues("Полярность прил. пост. ком. напр.:", lc.Select(m =>
                            //TestTypeEnumDictionary.GetPolarityDCSwitchingVoltageApplication().ToDictionary(n => n.Value, n => n.Key)[m.PolarityDCSwitchingVoltageApplication])));
                            //tbody.AppendChild(AddLineValues("Ком.ток, мА:", lc.Select(m => m.SwitchedAmperage.ToString())));
                            //tbody.AppendChild(AddLineValues("Ком. напр, В:", lc.Select(m => m.SwitchedVoltage.ToString())));
                            if (lc.First().ShowAuxiliaryVoltagePowerSupply1)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 1, В:", lc.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                            if (lc.First().ShowAuxiliaryVoltagePowerSupply2)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 2, В:", lc.Select(m => m.AuxiliaryVoltagePowerSupply2.ToString())));
                            break;

                        case SCME.Types.AuxiliaryPower.TestParameters apType:
                            var ap = j.Cast<SCME.Types.AuxiliaryPower.TestParameters>();
                            if (ap.First().ShowAuxiliaryVoltagePowerSupply1)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 1, В:", ap.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                            if (ap.First().ShowAuxiliaryVoltagePowerSupply2)
                                tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 2, В:", ap.Select(m => m.AuxiliaryVoltagePowerSupply2.ToString())));
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private void OpenFodlerResult_Click(object sender, RoutedEventArgs e)
        {
                Process.Start("explorer.exe", reportFolder);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _dispatcherTimerNeedStart.Stop();
            _dispatcherTimerNeedStart.Tick -= Dt_Tick;
            _dispatcherTimerNeedStart = null;
        }
    }
}
