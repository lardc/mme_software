namespace SCME.RemoteReportGenerator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelMain = new System.Windows.Forms.Panel();
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.P = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RG = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IGT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VGT = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IH = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VTM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VRRM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VDRM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SCIOrd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SCIName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.cbMMEList = new System.Windows.Forms.ComboBox();
            this.btnQuery = new System.Windows.Forms.Button();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lbGroups = new System.Windows.Forms.ListBox();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.gbReportSettings = new System.Windows.Forms.GroupBox();
            this.btnPrint = new System.Windows.Forms.Button();
            this.cbCustomer = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.tbDeviceType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.sdfReport = new System.Windows.Forms.SaveFileDialog();
            this.printDialog = new System.Windows.Forms.PrintDialog();
            this.statusStrip.SuspendLayout();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.panelLeft.SuspendLayout();
            this.gbReportSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 635);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1511, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip";
            // 
            // tsslStatus
            // 
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Size = new System.Drawing.Size(71, 17);
            this.tsslStatus.Text = "Отключено";
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.dgvData);
            this.panelMain.Controls.Add(this.panelLeft);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1511, 635);
            this.panelMain.TabIndex = 1;
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvData.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Code,
            this.P,
            this.Result,
            this.RG,
            this.IGT,
            this.VGT,
            this.IH,
            this.IL,
            this.VTM,
            this.VRRM,
            this.VDRM,
            this.SCIOrd,
            this.SCIName});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvData.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvData.Location = new System.Drawing.Point(283, 3);
            this.dgvData.MultiSelect = false;
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvData.ShowCellErrors = false;
            this.dgvData.ShowCellToolTips = false;
            this.dgvData.ShowEditingIcon = false;
            this.dgvData.ShowRowErrors = false;
            this.dgvData.Size = new System.Drawing.Size(1216, 629);
            this.dgvData.TabIndex = 18;
            // 
            // Code
            // 
            this.Code.HeaderText = "S/N";
            this.Code.Name = "Code";
            this.Code.ReadOnly = true;
            this.Code.Width = 57;
            // 
            // P
            // 
            this.P.HeaderText = "P";
            this.P.Name = "P";
            this.P.ReadOnly = true;
            this.P.Width = 41;
            // 
            // Result
            // 
            this.Result.HeaderText = "Результат";
            this.Result.Name = "Result";
            this.Result.ReadOnly = true;
            this.Result.Width = 101;
            // 
            // RG
            // 
            this.RG.HeaderText = "Rg (Ом)";
            this.RG.Name = "RG";
            this.RG.ReadOnly = true;
            this.RG.Width = 85;
            // 
            // IGT
            // 
            this.IGT.HeaderText = "Igt (мА)";
            this.IGT.Name = "IGT";
            this.IGT.ReadOnly = true;
            this.IGT.Width = 87;
            // 
            // VGT
            // 
            this.VGT.HeaderText = "Ugt (В)";
            this.VGT.Name = "VGT";
            this.VGT.ReadOnly = true;
            this.VGT.Width = 81;
            // 
            // IH
            // 
            this.IH.HeaderText = "Ih (мА)";
            this.IH.Name = "IH";
            this.IH.ReadOnly = true;
            this.IH.Width = 81;
            // 
            // IL
            // 
            this.IL.HeaderText = "IL (мА)";
            this.IL.Name = "IL";
            this.IL.ReadOnly = true;
            this.IL.Width = 80;
            // 
            // VTM
            // 
            this.VTM.HeaderText = "Utm[fm] (В)";
            this.VTM.Name = "VTM";
            this.VTM.ReadOnly = true;
            this.VTM.Width = 112;
            // 
            // VRRM
            // 
            this.VRRM.HeaderText = "Urrm (В)";
            this.VRRM.Name = "VRRM";
            this.VRRM.ReadOnly = true;
            this.VRRM.Width = 88;
            // 
            // VDRM
            // 
            this.VDRM.HeaderText = "Udrm (В)";
            this.VDRM.Name = "VDRM";
            this.VDRM.ReadOnly = true;
            this.VDRM.Width = 91;
            // 
            // SCIOrd
            // 
            this.SCIOrd.HeaderText = "SCR Order";
            this.SCIOrd.Name = "SCIOrd";
            this.SCIOrd.ReadOnly = true;
            // 
            // SCIName
            // 
            this.SCIName.HeaderText = "SCR S/N";
            this.SCIName.Name = "SCIName";
            this.SCIName.ReadOnly = true;
            this.SCIName.Width = 88;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.cbMMEList);
            this.panelLeft.Controls.Add(this.btnQuery);
            this.panelLeft.Controls.Add(this.dtpTo);
            this.panelLeft.Controls.Add(this.lbGroups);
            this.panelLeft.Controls.Add(this.dtpFrom);
            this.panelLeft.Controls.Add(this.gbReportSettings);
            this.panelLeft.Controls.Add(this.btnConnect);
            this.panelLeft.Controls.Add(this.label1);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Margin = new System.Windows.Forms.Padding(4);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(276, 635);
            this.panelLeft.TabIndex = 1;
            // 
            // cbMMEList
            // 
            this.cbMMEList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMMEList.FormattingEnabled = true;
            this.cbMMEList.Location = new System.Drawing.Point(12, 29);
            this.cbMMEList.Name = "cbMMEList";
            this.cbMMEList.Size = new System.Drawing.Size(252, 24);
            this.cbMMEList.TabIndex = 35;
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.Enabled = false;
            this.btnQuery.Location = new System.Drawing.Point(141, 286);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(123, 31);
            this.btnQuery.TabIndex = 34;
            this.btnQuery.Text = "Запрос";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.BtnQuery_Click);
            // 
            // dtpTo
            // 
            this.dtpTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpTo.CalendarTitleForeColor = System.Drawing.Color.White;
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(141, 257);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(123, 23);
            this.dtpTo.TabIndex = 33;
            this.dtpTo.Tag = "1";
            this.dtpTo.Value = new System.DateTime(2015, 1, 1, 0, 0, 0, 0);
            // 
            // lbGroups
            // 
            this.lbGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbGroups.ItemHeight = 16;
            this.lbGroups.Location = new System.Drawing.Point(12, 324);
            this.lbGroups.Name = "lbGroups";
            this.lbGroups.ScrollAlwaysVisible = true;
            this.lbGroups.Size = new System.Drawing.Size(252, 292);
            this.lbGroups.TabIndex = 32;
            this.lbGroups.SelectedIndexChanged += new System.EventHandler(this.LbGroups_SelectedIndexChanged);
            // 
            // dtpFrom
            // 
            this.dtpFrom.CalendarTitleForeColor = System.Drawing.Color.White;
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(12, 257);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(120, 23);
            this.dtpFrom.TabIndex = 31;
            this.dtpFrom.Tag = "1";
            this.dtpFrom.Value = new System.DateTime(2015, 7, 1, 0, 0, 0, 0);
            // 
            // gbReportSettings
            // 
            this.gbReportSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbReportSettings.Controls.Add(this.btnPrint);
            this.gbReportSettings.Controls.Add(this.cbCustomer);
            this.gbReportSettings.Controls.Add(this.btnSave);
            this.gbReportSettings.Controls.Add(this.tbDeviceType);
            this.gbReportSettings.Controls.Add(this.label3);
            this.gbReportSettings.Controls.Add(this.label2);
            this.gbReportSettings.Location = new System.Drawing.Point(12, 108);
            this.gbReportSettings.Name = "gbReportSettings";
            this.gbReportSettings.Size = new System.Drawing.Size(252, 141);
            this.gbReportSettings.TabIndex = 3;
            this.gbReportSettings.TabStop = false;
            this.gbReportSettings.Text = "Настройки отчета";
            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrint.Enabled = false;
            this.btnPrint.Location = new System.Drawing.Point(129, 101);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(117, 31);
            this.btnPrint.TabIndex = 12;
            this.btnPrint.Tag = "P";
            this.btnPrint.Text = "Печатать";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.BtnPrintSave_Click);
            // 
            // cbCustomer
            // 
            this.cbCustomer.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbCustomer.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbCustomer.FormattingEnabled = true;
            this.cbCustomer.Items.AddRange(new object[] {
            "П-Э"});
            this.cbCustomer.Location = new System.Drawing.Point(112, 21);
            this.cbCustomer.Name = "cbCustomer";
            this.cbCustomer.Size = new System.Drawing.Size(134, 24);
            this.cbCustomer.TabIndex = 11;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(9, 101);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(111, 31);
            this.btnSave.TabIndex = 8;
            this.btnSave.Tag = "S";
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnPrintSave_Click);
            // 
            // tbDeviceType
            // 
            this.tbDeviceType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDeviceType.Location = new System.Drawing.Point(112, 51);
            this.tbDeviceType.Name = "tbDeviceType";
            this.tbDeviceType.Size = new System.Drawing.Size(134, 23);
            this.tbDeviceType.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Тип прибора:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Заказчик:";
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Location = new System.Drawing.Point(141, 57);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(123, 31);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Подключиться";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(182, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Подключение к КИП СПП:";
            // 
            // sdfReport
            // 
            this.sdfReport.DefaultExt = "xls";
            this.sdfReport.Filter = "Excel files|*.xls";
            this.sdfReport.Title = "Сохранить отчет как...";
            // 
            // printDialog
            // 
            this.printDialog.AllowPrintToFile = false;
            this.printDialog.UseEXDialog = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1511, 657);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Генератор отчетов КИП СПП";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.gbReportSettings.ResumeLayout(false);
            this.gbReportSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.GroupBox gbReportSettings;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDeviceType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.ListBox lbGroups;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.SaveFileDialog sdfReport;
        private System.Windows.Forms.ComboBox cbCustomer;
        private System.Windows.Forms.ComboBox cbMMEList;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
        private System.Windows.Forms.DataGridViewTextBoxColumn P;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result;
        private System.Windows.Forms.DataGridViewTextBoxColumn RG;
        private System.Windows.Forms.DataGridViewTextBoxColumn IGT;
        private System.Windows.Forms.DataGridViewTextBoxColumn VGT;
        private System.Windows.Forms.DataGridViewTextBoxColumn IH;
        private System.Windows.Forms.DataGridViewTextBoxColumn IL;
        private System.Windows.Forms.DataGridViewTextBoxColumn VTM;
        private System.Windows.Forms.DataGridViewTextBoxColumn VRRM;
        private System.Windows.Forms.DataGridViewTextBoxColumn VDRM;
        private System.Windows.Forms.DataGridViewTextBoxColumn SCIOrd;
        private System.Windows.Forms.DataGridViewTextBoxColumn SCIName;
        private System.Windows.Forms.PrintDialog printDialog;

    }
}

