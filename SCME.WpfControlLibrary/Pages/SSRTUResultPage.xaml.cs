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

namespace SCME.WpfControlLibrary.Pages
{
    /// <summary>
    /// Логика взаимодействия для SSRTUResultPage.xaml
    /// </summary>
    public partial class SSRTUResultPage : Page
    {


        private Action _start;
        public Action Stop { get; set; }
        private Profile _profile;

        public SSRTUResultVM VM { get; set; } = new SSRTUResultVM();
        public SSRTUResultComponentVM VMPosition1 { get; set; } = new SSRTUResultComponentVM() { Positition = 1};
        public SSRTUResultComponentVM VMPosition2 { get; set; } = new SSRTUResultComponentVM() { Positition = 2 };
        public SSRTUResultComponentVM VMPosition3 { get; set; } = new SSRTUResultComponentVM() { Positition = 3 };

        public Dictionary<int, SSRTUResultComponentVM> VMByPosition{ get; set; }
        public SSRTUResultPage()
        {
            InitializeComponent();
        }

        public SSRTUResultPage(Profile profile, Action start, Action stop)
        {
            InitializeComponent();

            reportFolder = SCME.UIServiceConfig.Properties.Settings.Default.ReportFolder;
            if (Directory.Exists(reportFolder) == false)
                reportFolder = Directory.GetCurrentDirectory();    
            
            _start = start;
            _profile = profile;
            Stop = stop;
            VMByPosition = new Dictionary<int, SSRTUResultComponentVM>();
            VMByPosition[1] = VMPosition1;
            VMByPosition[2] = VMPosition2;
            VMByPosition[3] = VMPosition3;
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
                            sSRTUResultComponentVM.InputVoltageMin = j.InputVoltageMinimum;
                            sSRTUResultComponentVM.InputVoltageMax = j.InputVoltageMaximum;
                            sSRTUResultComponentVM.InputVoltage = 0;
                            foreach (var t in VMByPosition.Values)
                                t.ShowInputAmperage = false;
                        }
                        else
                        {
                            sSRTUResultComponentVM.InputAmperageMin = j.InputCurrentMinimum;
                            sSRTUResultComponentVM.InputAmperageMax = j.InputCurrentMaximum;
                            sSRTUResultComponentVM.InputAmperage = 0;
                            foreach (var t in VMByPosition.Values)
                                t.ShowInputAmperage = true;
                        }
                        if(j.ShowAuxiliaryVoltagePowerSupply1)
                        {
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMin1 = j.AuxiliaryCurrentPowerSupplyMinimum1;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMax1 = j.AuxiliaryCurrentPowerSupplyMaximum1;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupply1 = 0;
                        }
                        if (j.ShowAuxiliaryVoltagePowerSupply2)
                        {
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMin2 = j.AuxiliaryCurrentPowerSupplyMinimum2;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupplyMax2 = j.AuxiliaryCurrentPowerSupplyMaximum2;
                            sSRTUResultComponentVM.AuxiliaryCurrentPowerSupply2 = 0;
                        }
                        break;
                    case SCME.Types.OutputLeakageCurrent.TestParameters j:
                        sSRTUResultComponentVM.LeakageCurrentMin = j.LeakageCurrentMinimum;
                        sSRTUResultComponentVM.LeakageCurrentMax = j.LeakageCurrentMaximum;
                        sSRTUResultComponentVM.LeakageCurrent = 0;
                        break;
                    case SCME.Types.OutputResidualVoltage.TestParameters j:
                        sSRTUResultComponentVM.ResidualVoltageMin = j.OutputResidualVoltageMinimum;
                        sSRTUResultComponentVM.ResidualVoltageMax = j.OutputResidualVoltageMaximum;
                        sSRTUResultComponentVM.ResidualVoltage = 0;
                        if (j.OpenState)
                        {
                            sSRTUResultComponentVM.OpenResistanceMin = j.OpenResistanceMinimum;
                            sSRTUResultComponentVM.OpenResistanceMax = j.OpenResistanceMaximum;
                            sSRTUResultComponentVM.OpenResistance = 0;
                        }
                        break;
                    case SCME.Types.ProhibitionVoltage.TestParameters j:
                        sSRTUResultComponentVM.ProhibitionVoltageMin = j.ProhibitionVoltageMinimum;
                        sSRTUResultComponentVM.ProhibitionVoltageMax = j.ProhibitionVoltageMaximum;
                        sSRTUResultComponentVM.ProhibitionVoltage = 0;
                        break;
                    default:
                        break;
                }
            }
            
        }

        public void PostSSRTUNotificationEvent(string message, ushort problem, ushort warning, ushort fault, ushort disable)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogWindow = new DialogWindow("Ошибка оборудования", $"{message}\r\n problem {problem}, warning {warning}, fault {fault}, disable {disable}"); ;
                dialogWindow.ShowDialog();
                VM.CanStart = true;
            }));
        }
    

        public void PostAlarmEvent()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogWindow = new DialogWindow("Внимание", "Нарушен периметр безопасности");
                dialogWindow.ShowDialog();
            }));
        }

        private int countEndingTests;
        public void SSRTUHandler(DeviceState deviceState, Types.SSRTU.TestResults testResults)
        {
            var q = VMByPosition[testResults.NumberPosition];

            switch (testResults.TestParametersType)
            {
                case TestParametersType.InputOptions:
                    if (testResults.InputOptionsIsAmperage)
                        q.InputAmperage = testResults.Value;
                    else
                        q.InputVoltage = testResults.Value;
                    if(q.ShowAuxiliaryCurrentPowerSupply1)
                        q.AuxiliaryCurrentPowerSupply1 = testResults.AuxiliaryCurrentPowerSupply1;
                    if(q.ShowAuxiliaryCurrentPowerSupply2)
                        q.AuxiliaryCurrentPowerSupply2 = testResults.AuxiliaryCurrentPowerSupply2;
                    break;
                case TestParametersType.OutputLeakageCurrent:
                    q.LeakageCurrent = testResults.Value;
                    break;
                case TestParametersType.OutputResidualVoltage:
                    q.ResidualVoltage = testResults.Value;
                    if(q.OpenResistanceMin != null)
                        q.OpenResistance = testResults.OpenResistance;
                    break;
                case TestParametersType.ProhibitionVoltage:
                    q.ProhibitionVoltage = testResults.Value;
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
                result.First(m => m.Value != null).Value.SerialNumber = VM.SerialNumber;
                CreateReport();
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
            VM.CanStart = false;
            countEndingTests = 0;
            _start();
            return;
            VM.CanStart = false;
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
            });
        }


        private HtmlDocument _doc = new HtmlDocument();
        private List<Dictionary<int, SSRTUResultComponentVM>> results = new List<Dictionary<int, SSRTUResultComponentVM>>();
        private DateTime _dateTimeBeginMeasurement;
        private string reportFolder;

        private void CreateReport()
        {
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

            var table = _doc.CreateElement("table");
            table.SetAttributeValue("class", "center");
            table.SetAttributeValue("style", "width:96%;");

            var tbody = _doc.CreateElement("tbody");
            table.AppendChild(tbody);

            AddParameters(tbody);

            body.AppendChild(table);

            var h2 = _doc.CreateElement("h2");
            h2.InnerHtml = "Результаты";
            body.AppendChild(h2);

            table = _doc.CreateElement("table");
            table.SetAttributeValue("class", "center");
            table.SetAttributeValue("style", "width:96%;");

            tbody = _doc.CreateElement("tbody");
            table.AppendChild(tbody);

            body.AppendChild(table);

            AddHeadersResult(results.First().Values, tbody);
            foreach (var i in results)
                AddLineValues(i.Values, results.IndexOf(i) + 1, tbody);

            
            string fileName = $@"{_dateTimeBeginMeasurement.ToString("yyyy-MM-dd-hh-mm")}-{(string.IsNullOrEmpty(VM.BatchNumber) ? "NoBatchNumber" : VM.BatchNumber)}.html";
            File.WriteAllText(System.IO.Path.Combine(reportFolder, fileName), File.ReadAllText("ReportTemplate.html").Replace("body", body.OuterHtml));




        }

        private HtmlNode AddNumberPositions(BaseTestParametersAndNormatives[] parameters)
        {
            var tr = _doc.CreateElement("tr");
            for (var i = 0; i < parameters.Length; i++)
            {
                var td = _doc.CreateElement("td");
                td.InnerHtml = $"Позиция {parameters[i].NumberPosition}";
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
                td.InnerHtml = value.ToString();

            tr.AppendChild(td);
        }
        private void AddCellTdString(string value, HtmlNode tr)
        {
            var td = _doc.CreateElement("td");
            td.InnerHtml = value;
            tr.AppendChild(td);
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

        private void AddLineValues(IEnumerable<SSRTUResultComponentVM> valuesI, int number, HtmlNode tbody)
        {
            var values = valuesI.Where(m=> m != null).ToArray();
            var tr = _doc.CreateElement("tr");
            tbody.AppendChild(tr);

            var td = _doc.CreateElement("td");
            td.InnerHtml = values.First().SerialNumber.ToString();
            td.SetAttributeValue("rowspan", "3");
            tr.AppendChild(td);

            foreach (var i in values)
            {
                td = _doc.CreateElement("td");
                td.InnerHtml = i.Positition.ToString();
                tr.AppendChild(td);
                
                if (values.Count(m => m.LeakageCurrentMin != null) > 0)
                    AddCellTdString(i.LeakageCurrent, tr);

                if (values.Count(m => m.InputAmperageMin != null) > 0)
                    AddCellTdString(i.InputAmperage, tr);

                if (values.Count(m => m.InputVoltageMin != null) > 0)
                    AddCellTdString(i.InputVoltage, tr);

                if (values.Count(m => m.OpenResistanceMin != null) > 0)
                    AddCellTdString(i.OpenResistance, tr);

                if (values.Count(m => m.ResidualVoltageMin != null) > 0)
                    AddCellTdString(i.ResidualVoltage, tr);

                if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMin1 != null) > 0)
                    AddCellTdString(i.AuxiliaryCurrentPowerSupply1, tr);

                if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMin2 != null) > 0)
                    AddCellTdString(i.AuxiliaryCurrentPowerSupply2, tr);

                td = _doc.CreateElement("td");
                if (i.IsGood)
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

                tr = _doc.CreateElement("tr");
                tbody.AppendChild(tr);
            }
        }

        private void AddHeadersResult(IEnumerable<SSRTUResultComponentVM> valuesI, HtmlNode tbody)
        {
            var values = valuesI.Where(m=> m!= null).ToArray();
            var tr = _doc.CreateElement("tr");

            var td = _doc.CreateElement("td");
            td.InnerHtml = "Нормы";
            td.SetAttributeValue("rowspan", (values.Length * 2).ToString());
            tr.AppendChild(td);

            foreach (var i in values)
            {
                AddCellTdString($"Мин{i.Positition}", tr);
                
                if (values.Count(m => m.LeakageCurrentMin != null) > 0)
                    AddCellTdString(i.LeakageCurrentMin, tr);

                if (values.Count(m => m.InputAmperageMin != null) > 0)
                    AddCellTdString(i.InputAmperageMin, tr);

                if (values.Count(m => m.InputVoltageMin != null) > 0)
                    AddCellTdString(i.InputVoltageMin, tr);

                if (values.Count(m => m.OpenResistanceMin != null) > 0)
                    AddCellTdString(i.OpenResistanceMin, tr);

                if (values.Count(m => m.ResidualVoltageMin != null) > 0)
                    AddCellTdString(i.ResidualVoltageMin, tr);

                if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMin1 != null) > 0)
                    AddCellTdString(i.AuxiliaryCurrentPowerSupplyMin1, tr);

                if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMin2 != null) > 0)
                    AddCellTdString(i.AuxiliaryCurrentPowerSupplyMin2, tr);

                AddCellTdString("", tr);
                tbody.AppendChild(tr);
                
                tr = _doc.CreateElement("tr");
                AddCellTdString($"Макс{i.Positition}", tr);
                
                if (values.Count(m => m.LeakageCurrentMax != null) > 0)
                    AddCellTdString(i.LeakageCurrentMax , tr);

                if (values.Count(m => m.InputAmperageMax != null) > 0)
                    AddCellTdString(i.InputAmperageMax, tr);

                if (values.Count(m => m.InputVoltageMax != null) > 0)
                    AddCellTdString(i.InputVoltageMax, tr);

                if (values.Count(m => m.OpenResistanceMax != null) > 0)
                    AddCellTdString(i.OpenResistanceMax, tr);

                if (values.Count(m => m.ResidualVoltageMax != null) > 0)
                    AddCellTdString(i.ResidualVoltageMax, tr);

                if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMax1 != null) > 0)
                    AddCellTdString(i.AuxiliaryCurrentPowerSupplyMax1, tr);

                if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMax2 != null) > 0)
                    AddCellTdString(i.AuxiliaryCurrentPowerSupplyMax2, tr);

                AddCellTdString("", tr);
                tbody.AppendChild(tr);
                tr = _doc.CreateElement("tr");
            }

            AddCellThString("Номер", tr);
            AddCellThString("Позиция", tr);
            
            if (values.Count(m => m.LeakageCurrentMin != null) > 0)
                AddCellThString("Ток утечки", tr);

            if (values.Count(m => m.InputAmperageMin != null) > 0)
                AddCellThString("Ток входа", tr);

            if (values.Count(m => m.InputVoltageMin != null) > 0)
                AddCellThString("Напряжение входа", tr);

            if (values.Count(m => m.OpenResistanceMin != null) > 0)
                AddCellThString("Сопр. в откр. сост.", tr);

            if (values.Count(m => m.ResidualVoltageMin != null) > 0)
                AddCellThString("Выходное ост. напр.	", tr);

            if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMin1 != null) > 0)
                AddCellThString("Ток вспом. пит. 1", tr);

            if (values.Count(m => m.AuxiliaryCurrentPowerSupplyMin2 != null) > 0)
                AddCellThString("Ток вспом. пит. 2", tr);

            AddCellThString("Статус", tr);
            
            tbody.AppendChild(tr);
        }
       
        private void AddParameters(HtmlNode tbody)
        {
            foreach (var i in _profile.TestParametersAndNormatives.GroupBy(m => m.GetType()))
            {
                var t = i.First();

                var tr = _doc.CreateElement("tr");
                tbody.AppendChild(tr);

                var th = _doc.CreateElement("th");
                th.SetAttributeValue("colspan", "6");
                th.InnerHtml = TestTypeEnumDictionary.GetTestParametersTypes().Single(m => m.Type == t.GetType()).Name;
                tr.AppendChild(th);

                tbody.AppendChild(AddNumberPositions(i.ToArray()));

                switch (t)
                {
                    case SCME.Types.InputOptions.TestParameters ioType:
                        var io = i.Cast<SCME.Types.InputOptions.TestParameters>();
                        tbody.AppendChild(AddLineValues("Тип управления:", io.Select(m => TestTypeEnumDictionary.GetTypeManagementToString()[m.TypeManagement])));
                        tbody.AppendChild(AddLineValues("Напряжение управления, В:", io.Select(m => m.ControlVoltage.ToString())));
                        tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 1, В:", io.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                        tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 2, В:", io.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                        break;

                    case SCME.Types.OutputResidualVoltage.TestParameters rvType:
                        var rv = i.Cast<SCME.Types.OutputResidualVoltage.TestParameters>();
                        tbody.AppendChild(AddLineValues("Тип управления:", rv.Select(m => TestTypeEnumDictionary.GetTypeManagementToString()[m.TypeManagement].ToString())));
                        tbody.AppendChild(AddLineValues("Напряжение управления, В:", rv.Select(m => m.ControlVoltage.ToString())));
                        tbody.AppendChild(AddLineValues("Полярность прил. пост. ком. напр.:", rv.Select(m =>
                        TestTypeEnumDictionary.GetPolarityDCSwitchingVoltageApplication().ToDictionary(n => n.Value, n => n.Key)[m.PolarityDCSwitchingVoltageApplication])));
                        //tbody.AppendChild(AddLineValues("Ком.ток, мА:", rv.Select(m => m.SwitchedAmperage.ToString())));
                        //tbody.AppendChild(AddLineValues("Ком. напр, В:", rv.Select(m => m.SwitchedVoltage.ToString())));
                        tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 1, В:", rv.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                        tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 2, В:", rv.Select(m => m.AuxiliaryVoltagePowerSupply2.ToString())));
                        tbody.AppendChild(AddLineValues("Форма импульса коммутируемого тока:", rv.Select(m =>
                        TestTypeEnumDictionary.GetSwitchingCurrentPulseShape().ToDictionary(n => n.Value, n => n.Key)[m.SwitchingCurrentPulseShape])));
                        tbody.AppendChild(AddLineValues("Длительность имп. ком.тока., мкс:", rv.Select(m => m.SwitchingCurrentPulseDuration.ToString())));
                        break;

                    case SCME.Types.OutputLeakageCurrent.TestParameters lcType:
                        var lc = i.Cast<SCME.Types.OutputLeakageCurrent.TestParameters>();
                        tbody.AppendChild(AddLineValues("Тип управления:", lc.Select(m => TestTypeEnumDictionary.GetTypeManagementToString()[m.TypeManagement])));
                        tbody.AppendChild(AddLineValues("Напряжение управления, В:", lc.Select(m => m.ControlVoltage.ToString())));
                        tbody.AppendChild(AddLineValues("Тип ком. напр. при измер. утечки:.:", lc.Select(m =>
                        TestTypeEnumDictionary.GetApplicationPolarityConstantSwitchingVoltage().ToDictionary(n => n.Value, n => n.Key)[m.ApplicationPolarityConstantSwitchingVoltage])));
                        tbody.AppendChild(AddLineValues("Полярность прил. пост. ком. напр.:", lc.Select(m =>
                        TestTypeEnumDictionary.GetPolarityDCSwitchingVoltageApplication().ToDictionary(n => n.Value, n => n.Key)[m.PolarityDCSwitchingVoltageApplication])));
                        //tbody.AppendChild(AddLineValues("Ком.ток, мА:", lc.Select(m => m.SwitchedAmperage.ToString())));
                        //tbody.AppendChild(AddLineValues("Ком. напр, В:", lc.Select(m => m.SwitchedVoltage.ToString())));
                        tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 1, В:", lc.Select(m => m.AuxiliaryVoltagePowerSupply1.ToString())));
                        tbody.AppendChild(AddLineValues("Напряжение вспом. пит. 2, В:", lc.Select(m => m.AuxiliaryVoltagePowerSupply2.ToString())));

                        break;
                    default:
                        break;
                }
            }
        }

        private void OpenFodlerResult_Click(object sender, RoutedEventArgs e)
        {
                Process.Start("explorer.exe", reportFolder);
        }
    }
}
