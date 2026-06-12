namespace KebuzForge.App
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            menuStrip = new MenuStrip();
            menuFile = new ToolStripMenuItem();
            menuFileOpen = new ToolStripMenuItem();
            menuFileExit = new ToolStripMenuItem();
            menuEdit = new ToolStripMenuItem();
            menuEditUndo = new ToolStripMenuItem();
            menuEditRedo = new ToolStripMenuItem();
            menuEditReset = new ToolStripMenuItem();
            menuPresets = new ToolStripMenuItem();
            menuPresetSave = new ToolStripMenuItem();
            menuExport = new ToolStripMenuItem();
            menuExportPng = new ToolStripMenuItem();
            menuExportIndexed = new ToolStripMenuItem();
            menuExportIco = new ToolStripMenuItem();
            menuExportBatch = new ToolStripMenuItem();
            menuView = new ToolStripMenuItem();
            miLight = new ToolStripMenuItem();
            miDark = new ToolStripMenuItem();
            miHybrid = new ToolStripMenuItem();
            miLangSeparator = new ToolStripSeparator();
            miLangRu = new ToolStripMenuItem();
            miLangEn = new ToolStripMenuItem();
            menuPlugins = new ToolStripMenuItem();
            menuPluginsFolder = new ToolStripMenuItem();
            toolStrip = new ToolStrip();
            btnOpen = new ToolStripButton();
            btnUndo = new ToolStripButton();
            btnRedo = new ToolStripButton();
            statusStrip = new StatusStrip();
            lblStatusOriginal = new ToolStripStatusLabel();
            lblStatusResult = new ToolStripStatusLabel();
            lblStatusZoom = new ToolStripStatusLabel();
            mainSplit = new SplitContainer();
            leftPanel = new Panel();
            grpPixelize = new GroupBox();
            lblWidthTitle = new Label();
            nudWidth = new NumericUpDown();
            chkKeepAspect = new CheckBox();
            lblHeightTitle = new Label();
            nudHeight = new NumericUpDown();
            lblAlgoTitle = new Label();
            cmbPixelAlgorithm = new ComboBox();
            btnApplyPixelize = new Button();
            grpPalette = new GroupBox();
            lblColorCountTitle = new Label();
            nudColorCount = new NumericUpDown();
            cmbRetroPalette = new ComboBox();
            btnApplyPalette = new Button();
            btnSavePalette = new Button();
            btnLoadPalette = new Button();
            grpDither = new GroupBox();
            cmbDitherMode = new ComboBox();
            lblDitherVal = new Label();
            trkDitherIntensity = new TrackBar();
            btnApplyDither = new Button();
            grpEffects = new GroupBox();
            cmbCylinderDir = new ComboBox();
            rbEffectNone = new RadioButton();
            rbEffectSphere = new RadioButton();
            rbEffectCylinder = new RadioButton();
            rbEffectRounded = new RadioButton();
            chkShapeFromRect = new CheckBox();
            lblCornerSharp = new Label();
            trkCornerSharp = new TrackBar();
            lblSphereScale = new Label();
            trkSphereScale = new TrackBar();
            lblSphereBulge = new Label();
            trkSphereBulge = new TrackBar();
            lblOffsetU = new Label();
            trkOffsetU = new TrackBar();
            lblOffsetV = new Label();
            trkOffsetV = new TrackBar();
            btnApplyEffect = new Button();
            grpLighting = new GroupBox();
            lblAmbient = new Label();
            trkAmbient = new TrackBar();
            lblDiffuse = new Label();
            trkDiffuse = new TrackBar();
            lblLightAzimuth = new Label();
            trkLightAzimuth = new TrackBar();
            lblLightElev = new Label();
            trkLightElev = new TrackBar();
            chkSpecular = new CheckBox();
            grpEdges = new GroupBox();
            chkEdges = new CheckBox();
            lblEdgeThickTitle = new Label();
            nudEdgeThickness = new NumericUpDown();
            cmbEdgeMode = new ComboBox();
            btnEdgeColor = new Button();
            grpBackground = new GroupBox();
            btnPickBgColor = new Button();
            lblBgTolTitle = new Label();
            nudBgTolerance = new NumericUpDown();
            btnRemoveBg = new Button();
            rightSplit = new SplitContainer();
            previewSplit = new SplitContainer();
            panelBefore = new Panel();
            picBefore = new PictureBox();
            btnDropHint = new Button();
            panelBeforeTitle = new Panel();
            lblBeforeTitle = new Label();
            btnDetachBefore = new Button();
            panelAfter = new Panel();
            picAfter = new KebuzForge.App.UI.PixelPictureBox();
            panelAfterTitle = new Panel();
            lblAfterTitle = new Label();
            btnDetachAfter = new Button();
            tabBottom = new TabControl();
            tabPageEditor = new TabPage();
            editorScroll = new Panel();
            _pixelEditor = new KebuzForge.App.UI.PixelEditorPanel();
            editorToolStrip = new ToolStrip();
            btnToolPencil = new ToolStripButton();
            btnToolLine = new ToolStripButton();
            btnToolRect = new ToolStripButton();
            btnToolFill = new ToolStripButton();
            btnToolEyedrop = new ToolStripButton();
            btnToolEraser = new ToolStripButton();
            btnZoom1 = new ToolStripButton();
            btnZoom2 = new ToolStripButton();
            btnZoom4 = new ToolStripButton();
            btnZoom8 = new ToolStripButton();
            btnZoom16 = new ToolStripButton();
            btnEditorColor = new ToolStripButton();
            chkEditorPaletteOnly = new ToolStripButton();
            chkEraserTransparent = new ToolStripButton();
            btnEditorApply = new ToolStripButton();
            btnDetachEditor = new ToolStripButton();
            tabPagePalette = new TabPage();
            _palettePanel = new UI.PalettePanel();
            panelPaletteBar = new Panel();
            btnPickPaletteColor = new Button();
            btnDetachPalette = new Button();
            tabPageAnimation = new TabPage();
            flpFrames = new FlowLayoutPanel();
            pnlAnimToolbar = new Panel();
            btnAnimPlay = new Button();
            btnAnimStop = new Button();
            lblAnimFrame = new Label();
            lblFpsTitle = new Label();
            nudAnimFps = new NumericUpDown();
            btnProcessFrames = new Button();
            btnExportGif = new Button();
            btnDetachAnim = new Button();
            menuStrip.SuspendLayout();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
            mainSplit.Panel1.SuspendLayout();
            mainSplit.Panel2.SuspendLayout();
            mainSplit.SuspendLayout();
            leftPanel.SuspendLayout();
            grpPixelize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).BeginInit();
            grpPalette.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudColorCount).BeginInit();
            grpDither.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkDitherIntensity).BeginInit();
            grpEffects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkSphereScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkSphereBulge).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkCornerSharp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkOffsetU).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkOffsetV).BeginInit();
            grpLighting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkAmbient).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkDiffuse).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkLightAzimuth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trkLightElev).BeginInit();
            grpEdges.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudEdgeThickness).BeginInit();
            grpBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudBgTolerance).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightSplit).BeginInit();
            rightSplit.Panel1.SuspendLayout();
            rightSplit.Panel2.SuspendLayout();
            rightSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)previewSplit).BeginInit();
            previewSplit.Panel1.SuspendLayout();
            previewSplit.Panel2.SuspendLayout();
            previewSplit.SuspendLayout();
            panelBefore.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBefore).BeginInit();
            panelBeforeTitle.SuspendLayout();
            panelAfter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAfter).BeginInit();
            panelAfterTitle.SuspendLayout();
            tabBottom.SuspendLayout();
            tabPageEditor.SuspendLayout();
            editorScroll.SuspendLayout();
            editorToolStrip.SuspendLayout();
            tabPagePalette.SuspendLayout();
            panelPaletteBar.SuspendLayout();
            tabPageAnimation.SuspendLayout();
            pnlAnimToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudAnimFps).BeginInit();
            SuspendLayout();

            menuStrip.Items.AddRange(new ToolStripItem[] { menuFile, menuEdit, menuPresets, menuExport, menuPlugins, menuView });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1284, 24);
            menuStrip.TabIndex = 2;

            menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuFileOpen, menuFileExit });
            menuFile.Name = "menuFile";
            menuFile.Size = new Size(48, 20);
            menuFile.Text = "Файл";

            menuFileOpen.Name = "menuFileOpen";
            menuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            menuFileOpen.Size = new Size(173, 22);
            menuFileOpen.Text = "Открыть...";

            menuFileExit.Name = "menuFileExit";
            menuFileExit.Size = new Size(173, 22);
            menuFileExit.Text = "Выход";

            menuEdit.DropDownItems.AddRange(new ToolStripItem[] { menuEditUndo, menuEditRedo, menuEditReset });
            menuEdit.Name = "menuEdit";
            menuEdit.Size = new Size(59, 20);
            menuEdit.Text = "Правка";

            menuEditUndo.Name = "menuEditUndo";
            menuEditUndo.ShortcutKeys = Keys.Control | Keys.Z;
            menuEditUndo.Size = new Size(188, 22);
            menuEditUndo.Text = "Отменить";

            menuEditRedo.Name = "menuEditRedo";
            menuEditRedo.ShortcutKeys = Keys.Control | Keys.Y;
            menuEditRedo.Size = new Size(188, 22);
            menuEditRedo.Text = "Повторить";

            menuEditReset.Name = "menuEditReset";
            menuEditReset.Size = new Size(188, 22);
            menuEditReset.Text = "Сбросить настройки";

            menuPresets.DropDownItems.AddRange(new ToolStripItem[] { menuPresetSave });
            menuPresets.Name = "menuPresets";
            menuPresets.Size = new Size(67, 20);
            menuPresets.Text = "Пресеты";

            menuPresetSave.Name = "menuPresetSave";
            menuPresetSave.Size = new Size(182, 22);
            menuPresetSave.Text = "Сохранить пресет...";

            menuExport.DropDownItems.AddRange(new ToolStripItem[] { menuExportPng, menuExportIndexed, menuExportIco, menuExportBatch });
            menuExport.Name = "menuExport";
            menuExport.Size = new Size(64, 20);
            menuExport.Text = "Экспорт";

            menuExportPng.Name = "menuExportPng";
            menuExportPng.Size = new Size(199, 22);
            menuExportPng.Text = "PNG...";

            menuExportIndexed.Name = "menuExportIndexed";
            menuExportIndexed.Size = new Size(199, 22);
            menuExportIndexed.Text = "Indexed PNG (8 bpp)...";

            menuExportIco.Name = "menuExportIco";
            menuExportIco.Size = new Size(199, 22);
            menuExportIco.Text = "ICO (мульти-размер)...";

            menuExportBatch.Name = "menuExportBatch";
            menuExportBatch.Size = new Size(199, 22);
            menuExportBatch.Text = "Пакетная обработка...";

            menuView.DropDownItems.AddRange(new ToolStripItem[] { miLight, miDark, miHybrid, miLangSeparator, miLangRu, miLangEn });
            menuView.Name = "menuView";
            menuView.Size = new Size(38, 20);
            menuView.Text = "Вид";

            miLight.Name = "miLight";
            miLight.Size = new Size(133, 22);
            miLight.Text = "  Светлая";

            miDark.Name = "miDark";
            miDark.Size = new Size(133, 22);
            miDark.Text = "  Тёмная";

            miHybrid.Name = "miHybrid";
            miHybrid.Size = new Size(133, 22);
            miHybrid.Text = "  Гибридная";

            miLangSeparator.Name = "miLangSeparator";
            miLangSeparator.Size = new Size(130, 6);

            miLangRu.Name = "miLangRu";
            miLangRu.Size = new Size(133, 22);
            miLangRu.Text = "  Русский";

            miLangEn.Name = "miLangEn";
            miLangEn.Size = new Size(133, 22);
            miLangEn.Text = "  English";

            menuPlugins.DropDownItems.AddRange(new ToolStripItem[] { menuPluginsFolder });
            menuPlugins.Name = "menuPlugins";
            menuPlugins.Size = new Size(67, 20);
            menuPlugins.Text = "Плагины";

            menuPluginsFolder.Name = "menuPluginsFolder";
            menuPluginsFolder.Size = new Size(220, 22);
            menuPluginsFolder.Text = "Открыть папку плагинов...";

            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { btnOpen, btnUndo, btnRedo });
            toolStrip.Location = new Point(0, 24);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(1284, 25);
            toolStrip.TabIndex = 1;

            btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(58, 22);
            btnOpen.Text = "Открыть";

            btnUndo.Enabled = false;
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(81, 22);
            btnUndo.Text = "↶  Отменить";

            btnRedo.Enabled = false;
            btnRedo.Name = "btnRedo";
            btnRedo.Size = new Size(86, 22);
            btnRedo.Text = "↷  Повторить";

            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusOriginal, lblStatusResult, lblStatusZoom });
            statusStrip.Location = new Point(0, 757);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1284, 24);
            statusStrip.TabIndex = 3;

            lblStatusOriginal.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblStatusOriginal.BorderStyle = Border3DStyle.Etched;
            lblStatusOriginal.Name = "lblStatusOriginal";
            lblStatusOriginal.Size = new Size(84, 19);
            lblStatusOriginal.Text = "Оригинал: -";

            lblStatusResult.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblStatusResult.BorderStyle = Border3DStyle.Etched;
            lblStatusResult.Name = "lblStatusResult";
            lblStatusResult.Size = new Size(82, 19);
            lblStatusResult.Text = "Результат: -";

            lblStatusZoom.Name = "lblStatusZoom";
            lblStatusZoom.Size = new Size(1103, 19);
            lblStatusZoom.Spring = true;
            lblStatusZoom.TextAlign = ContentAlignment.MiddleRight;

            mainSplit.Dock = DockStyle.Fill;
            mainSplit.Location = new Point(0, 49);
            mainSplit.Name = "mainSplit";

            mainSplit.Panel1.Controls.Add(leftPanel);
            mainSplit.Panel1MinSize = 240;

            mainSplit.Panel2.Controls.Add(rightSplit);
            mainSplit.Panel2MinSize = 400;
            mainSplit.Size = new Size(1284, 708);
            mainSplit.SplitterDistance = 340;
            mainSplit.TabIndex = 0;
            mainSplit.SplitterMoved += mainSplit_SplitterMoved;

            leftPanel.AutoScroll = true;
            leftPanel.BackColor = Color.FromArgb(245, 245, 245);
            leftPanel.Controls.Add(grpPixelize);
            leftPanel.Controls.Add(grpPalette);
            leftPanel.Controls.Add(grpDither);
            leftPanel.Controls.Add(grpEffects);
            leftPanel.Controls.Add(grpLighting);
            leftPanel.Controls.Add(grpEdges);
            leftPanel.Controls.Add(grpBackground);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(0, 0);
            leftPanel.Name = "leftPanel";
            leftPanel.Size = new Size(340, 708);
            leftPanel.TabIndex = 0;

            grpPixelize.Controls.Add(lblWidthTitle);
            grpPixelize.Controls.Add(nudWidth);
            grpPixelize.Controls.Add(chkKeepAspect);
            grpPixelize.Controls.Add(lblHeightTitle);
            grpPixelize.Controls.Add(nudHeight);
            grpPixelize.Controls.Add(lblAlgoTitle);
            grpPixelize.Controls.Add(cmbPixelAlgorithm);
            grpPixelize.Controls.Add(btnApplyPixelize);
            grpPixelize.Location = new Point(6, 3);
            grpPixelize.Name = "grpPixelize";
            grpPixelize.Size = new Size(260, 205);
            grpPixelize.TabIndex = 0;
            grpPixelize.TabStop = false;
            grpPixelize.Text = "Пикселизация";

            lblWidthTitle.Location = new Point(8, 25);
            lblWidthTitle.Name = "lblWidthTitle";
            lblWidthTitle.Size = new Size(58, 18);
            lblWidthTitle.TabIndex = 0;
            lblWidthTitle.Text = "Ширина:";
            lblWidthTitle.TextAlign = ContentAlignment.MiddleRight;

            nudWidth.Location = new Point(70, 23);
            nudWidth.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudWidth.Name = "nudWidth";
            nudWidth.Size = new Size(72, 23);
            nudWidth.TabIndex = 1;
            nudWidth.Value = new decimal(new int[] { 64, 0, 0, 0 });

            chkKeepAspect.Checked = true;
            chkKeepAspect.CheckState = CheckState.Checked;
            chkKeepAspect.Location = new Point(8, 50);
            chkKeepAspect.Name = "chkKeepAspect";
            chkKeepAspect.Size = new Size(170, 24);
            chkKeepAspect.TabIndex = 2;
            chkKeepAspect.Text = "Сохранять пропорции";

            lblHeightTitle.Location = new Point(8, 76);
            lblHeightTitle.Name = "lblHeightTitle";
            lblHeightTitle.Size = new Size(58, 18);
            lblHeightTitle.TabIndex = 3;
            lblHeightTitle.Text = "Высота:";
            lblHeightTitle.TextAlign = ContentAlignment.MiddleRight;

            nudHeight.Location = new Point(70, 74);
            nudHeight.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudHeight.Name = "nudHeight";
            nudHeight.Size = new Size(72, 23);
            nudHeight.TabIndex = 4;
            nudHeight.Value = new decimal(new int[] { 64, 0, 0, 0 });

            lblAlgoTitle.Location = new Point(8, 106);
            lblAlgoTitle.Name = "lblAlgoTitle";
            lblAlgoTitle.Size = new Size(80, 16);
            lblAlgoTitle.TabIndex = 5;
            lblAlgoTitle.Text = "Алгоритм:";

            cmbPixelAlgorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPixelAlgorithm.Items.AddRange(new object[] { "Среднее (блок)", "Центр блока" });
            cmbPixelAlgorithm.Location = new Point(8, 126);
            cmbPixelAlgorithm.Name = "cmbPixelAlgorithm";
            cmbPixelAlgorithm.Size = new Size(210, 23);
            cmbPixelAlgorithm.TabIndex = 6;

            btnApplyPixelize.Location = new Point(8, 162);
            btnApplyPixelize.Name = "btnApplyPixelize";
            btnApplyPixelize.Size = new Size(150, 28);
            btnApplyPixelize.TabIndex = 7;
            btnApplyPixelize.Text = "Пикселизировать";

            grpPalette.Controls.Add(lblColorCountTitle);
            grpPalette.Controls.Add(nudColorCount);
            grpPalette.Controls.Add(cmbRetroPalette);
            grpPalette.Controls.Add(btnApplyPalette);
            grpPalette.Controls.Add(btnSavePalette);
            grpPalette.Controls.Add(btnLoadPalette);
            grpPalette.Location = new Point(6, 214);
            grpPalette.Name = "grpPalette";
            grpPalette.Size = new Size(260, 158);
            grpPalette.TabIndex = 1;
            grpPalette.TabStop = false;
            grpPalette.Text = "Палитра";

            lblColorCountTitle.Location = new Point(8, 24);
            lblColorCountTitle.Name = "lblColorCountTitle";
            lblColorCountTitle.Size = new Size(56, 23);
            lblColorCountTitle.TabIndex = 0;
            lblColorCountTitle.Text = "Цветов:";
            lblColorCountTitle.TextAlign = ContentAlignment.MiddleRight;

            nudColorCount.Location = new Point(68, 22);
            nudColorCount.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            nudColorCount.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            nudColorCount.Name = "nudColorCount";
            nudColorCount.Size = new Size(62, 23);
            nudColorCount.TabIndex = 1;
            nudColorCount.Value = new decimal(new int[] { 16, 0, 0, 0 });

            cmbRetroPalette.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRetroPalette.Items.AddRange(new object[] { "- Своя палитра -", "CGA (4 цвета)", "CGA (16 цветов)", "EGA (16)", "GameBoy (4)", "ZX Spectrum (15)", "Amiga (32)", "Amiga (64)" });
            cmbRetroPalette.Location = new Point(8, 52);
            cmbRetroPalette.Name = "cmbRetroPalette";
            cmbRetroPalette.Size = new Size(218, 23);
            cmbRetroPalette.TabIndex = 2;

            btnApplyPalette.Location = new Point(8, 86);
            btnApplyPalette.Name = "btnApplyPalette";
            btnApplyPalette.Size = new Size(100, 26);
            btnApplyPalette.TabIndex = 3;
            btnApplyPalette.Text = "Применить";

            btnSavePalette.Location = new Point(114, 86);
            btnSavePalette.Name = "btnSavePalette";
            btnSavePalette.Size = new Size(90, 26);
            btnSavePalette.TabIndex = 4;
            btnSavePalette.Text = "Сохранить";

            btnLoadPalette.Location = new Point(8, 118);
            btnLoadPalette.Name = "btnLoadPalette";
            btnLoadPalette.Size = new Size(90, 26);
            btnLoadPalette.TabIndex = 5;
            btnLoadPalette.Text = "Загрузить";

            grpDither.Controls.Add(cmbDitherMode);
            grpDither.Controls.Add(lblDitherVal);
            grpDither.Controls.Add(trkDitherIntensity);
            grpDither.Controls.Add(btnApplyDither);
            grpDither.Location = new Point(6, 378);
            grpDither.Name = "grpDither";
            grpDither.Size = new Size(260, 148);
            grpDither.TabIndex = 2;
            grpDither.TabStop = false;
            grpDither.Text = "Дизеринг";

            cmbDitherMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDitherMode.Items.AddRange(new object[] { "Нет", "Floyd-Steinberg", "Bayer 4x4", "Bayer 8x8" });
            cmbDitherMode.Location = new Point(8, 22);
            cmbDitherMode.Name = "cmbDitherMode";
            cmbDitherMode.Size = new Size(220, 23);
            cmbDitherMode.TabIndex = 0;

            lblDitherVal.Location = new Point(8, 54);
            lblDitherVal.Name = "lblDitherVal";
            lblDitherVal.Size = new Size(160, 16);
            lblDitherVal.TabIndex = 1;
            lblDitherVal.Text = "Интенсивность: 50%";

            trkDitherIntensity.AutoSize = false;
            trkDitherIntensity.Location = new Point(8, 72);
            trkDitherIntensity.Maximum = 100;
            trkDitherIntensity.Name = "trkDitherIntensity";
            trkDitherIntensity.Size = new Size(230, 30);
            trkDitherIntensity.TabIndex = 2;
            trkDitherIntensity.TickStyle = TickStyle.None;
            trkDitherIntensity.Value = 50;

            btnApplyDither.Location = new Point(8, 110);
            btnApplyDither.Name = "btnApplyDither";
            btnApplyDither.Size = new Size(150, 28);
            btnApplyDither.TabIndex = 3;
            btnApplyDither.Text = "Применить дизеринг";

            grpEffects.Controls.Add(cmbCylinderDir);
            grpEffects.Controls.Add(rbEffectNone);
            grpEffects.Controls.Add(rbEffectSphere);
            grpEffects.Controls.Add(rbEffectCylinder);
            grpEffects.Controls.Add(rbEffectRounded);
            grpEffects.Controls.Add(chkShapeFromRect);
            grpEffects.Controls.Add(lblCornerSharp);
            grpEffects.Controls.Add(trkCornerSharp);
            grpEffects.Controls.Add(lblSphereScale);
            grpEffects.Controls.Add(trkSphereScale);
            grpEffects.Controls.Add(lblSphereBulge);
            grpEffects.Controls.Add(trkSphereBulge);
            grpEffects.Controls.Add(lblOffsetU);
            grpEffects.Controls.Add(trkOffsetU);
            grpEffects.Controls.Add(lblOffsetV);
            grpEffects.Controls.Add(trkOffsetV);
            grpEffects.Controls.Add(btnApplyEffect);
            grpEffects.Location = new Point(6, 532);
            grpEffects.Name = "grpEffects";
            grpEffects.Size = new Size(260, 402);
            grpEffects.TabIndex = 3;
            grpEffects.TabStop = false;
            grpEffects.Text = "3D Эффекты";

            cmbCylinderDir.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCylinderDir.Enabled = false;
            cmbCylinderDir.Items.AddRange(new object[] { "Горизонтальный", "Вертикальный" });
            cmbCylinderDir.Location = new Point(90, 67);
            cmbCylinderDir.Name = "cmbCylinderDir";
            cmbCylinderDir.Size = new Size(128, 23);
            cmbCylinderDir.TabIndex = 3;

            rbEffectNone.Checked = true;
            rbEffectNone.Location = new Point(8, 22);
            rbEffectNone.Name = "rbEffectNone";
            rbEffectNone.Size = new Size(130, 24);
            rbEffectNone.TabIndex = 0;
            rbEffectNone.TabStop = true;
            rbEffectNone.Text = "Нет (плоский)";

            rbEffectSphere.Location = new Point(8, 44);
            rbEffectSphere.Name = "rbEffectSphere";
            rbEffectSphere.Size = new Size(130, 24);
            rbEffectSphere.TabIndex = 1;
            rbEffectSphere.Text = "Шар (Spherize)";

            rbEffectCylinder.Location = new Point(8, 66);
            rbEffectCylinder.Name = "rbEffectCylinder";
            rbEffectCylinder.Size = new Size(80, 24);
            rbEffectCylinder.TabIndex = 2;
            rbEffectCylinder.Text = "Цилиндр";

            rbEffectRounded.Location = new Point(8, 88);
            rbEffectRounded.Name = "rbEffectRounded";
            rbEffectRounded.Size = new Size(200, 24);
            rbEffectRounded.TabIndex = 13;
            rbEffectRounded.Text = "Скруглённый квадрат";

            chkShapeFromRect.Enabled = false;
            chkShapeFromRect.Location = new Point(24, 112);
            chkShapeFromRect.Name = "chkShapeFromRect";
            chkShapeFromRect.Size = new Size(210, 20);
            chkShapeFromRect.TabIndex = 16;
            chkShapeFromRect.Text = "Инверсия: □→○";

            lblSphereScale.Location = new Point(8, 138);
            lblSphereScale.Name = "lblSphereScale";
            lblSphereScale.Size = new Size(180, 16);
            lblSphereScale.TabIndex = 4;
            lblSphereScale.Text = "Масштаб: 100%";

            trkSphereScale.AutoSize = false;
            trkSphereScale.Location = new Point(8, 154);
            trkSphereScale.Maximum = 300;
            trkSphereScale.Minimum = 25;
            trkSphereScale.Name = "trkSphereScale";
            trkSphereScale.Size = new Size(230, 26);
            trkSphereScale.TabIndex = 5;
            trkSphereScale.TickStyle = TickStyle.None;
            trkSphereScale.Value = 100;

            lblSphereBulge.Location = new Point(8, 184);
            lblSphereBulge.Name = "lblSphereBulge";
            lblSphereBulge.Size = new Size(180, 16);
            lblSphereBulge.TabIndex = 6;
            lblSphereBulge.Text = "Выпуклость: 100%";

            trkSphereBulge.AutoSize = false;
            trkSphereBulge.Location = new Point(8, 200);
            trkSphereBulge.Maximum = 100;
            trkSphereBulge.Name = "trkSphereBulge";
            trkSphereBulge.Size = new Size(230, 26);
            trkSphereBulge.TabIndex = 7;
            trkSphereBulge.TickStyle = TickStyle.None;
            trkSphereBulge.Value = 100;

            lblCornerSharp.Enabled = false;
            lblCornerSharp.Location = new Point(8, 230);
            lblCornerSharp.Name = "lblCornerSharp";
            lblCornerSharp.Size = new Size(180, 16);
            lblCornerSharp.TabIndex = 14;
            lblCornerSharp.Text = "Форма: 0%";

            trkCornerSharp.AutoSize = false;
            trkCornerSharp.Enabled = false;
            trkCornerSharp.Location = new Point(8, 246);
            trkCornerSharp.Maximum = 100;
            trkCornerSharp.Name = "trkCornerSharp";
            trkCornerSharp.Size = new Size(230, 26);
            trkCornerSharp.TabIndex = 15;
            trkCornerSharp.TickStyle = TickStyle.None;

            lblOffsetU.Location = new Point(8, 276);
            lblOffsetU.Name = "lblOffsetU";
            lblOffsetU.Size = new Size(180, 16);
            lblOffsetU.TabIndex = 8;
            lblOffsetU.Text = "Смещение X: 0%";

            trkOffsetU.AutoSize = false;
            trkOffsetU.Location = new Point(8, 292);
            trkOffsetU.Maximum = 100;
            trkOffsetU.Minimum = -100;
            trkOffsetU.Name = "trkOffsetU";
            trkOffsetU.Size = new Size(230, 26);
            trkOffsetU.TabIndex = 9;
            trkOffsetU.TickStyle = TickStyle.None;

            lblOffsetV.Location = new Point(8, 322);
            lblOffsetV.Name = "lblOffsetV";
            lblOffsetV.Size = new Size(180, 16);
            lblOffsetV.TabIndex = 10;
            lblOffsetV.Text = "Смещение Y: 0%";

            trkOffsetV.AutoSize = false;
            trkOffsetV.Location = new Point(8, 338);
            trkOffsetV.Maximum = 100;
            trkOffsetV.Minimum = -100;
            trkOffsetV.Name = "trkOffsetV";
            trkOffsetV.Size = new Size(230, 26);
            trkOffsetV.TabIndex = 11;
            trkOffsetV.TickStyle = TickStyle.None;

            btnApplyEffect.Location = new Point(8, 368);
            btnApplyEffect.Name = "btnApplyEffect";
            btnApplyEffect.Size = new Size(150, 26);
            btnApplyEffect.TabIndex = 12;
            btnApplyEffect.Text = "Применить эффект";

            grpLighting.Controls.Add(chkSpecular);
            grpLighting.Controls.Add(lblAmbient);
            grpLighting.Controls.Add(trkAmbient);
            grpLighting.Controls.Add(lblDiffuse);
            grpLighting.Controls.Add(trkDiffuse);
            grpLighting.Controls.Add(lblLightAzimuth);
            grpLighting.Controls.Add(trkLightAzimuth);
            grpLighting.Controls.Add(lblLightElev);
            grpLighting.Controls.Add(trkLightElev);
            grpLighting.Location = new Point(6, 940);
            grpLighting.Name = "grpLighting";
            grpLighting.Size = new Size(260, 236);
            grpLighting.TabIndex = 4;
            grpLighting.TabStop = false;
            grpLighting.Text = "Освещение";

            lblAmbient.Location = new Point(8, 22);
            lblAmbient.Name = "lblAmbient";
            lblAmbient.Size = new Size(210, 16);
            lblAmbient.TabIndex = 0;
            lblAmbient.Text = "Общий свет: 25%";

            trkAmbient.AutoSize = false;
            trkAmbient.Location = new Point(8, 38);
            trkAmbient.Maximum = 100;
            trkAmbient.Name = "trkAmbient";
            trkAmbient.Size = new Size(230, 26);
            trkAmbient.TabIndex = 1;
            trkAmbient.TickStyle = TickStyle.None;
            trkAmbient.Value = 25;

            lblDiffuse.Location = new Point(8, 68);
            lblDiffuse.Name = "lblDiffuse";
            lblDiffuse.Size = new Size(210, 16);
            lblDiffuse.TabIndex = 2;
            lblDiffuse.Text = "Рассеянный свет: 85%";

            trkDiffuse.AutoSize = false;
            trkDiffuse.Location = new Point(8, 84);
            trkDiffuse.Maximum = 100;
            trkDiffuse.Name = "trkDiffuse";
            trkDiffuse.Size = new Size(230, 26);
            trkDiffuse.TabIndex = 3;
            trkDiffuse.TickStyle = TickStyle.None;
            trkDiffuse.Value = 85;

            lblLightAzimuth.Location = new Point(8, 114);
            lblLightAzimuth.Name = "lblLightAzimuth";
            lblLightAzimuth.Size = new Size(210, 16);
            lblLightAzimuth.TabIndex = 4;
            lblLightAzimuth.Text = "Горизонталь: -45";

            trkLightAzimuth.AutoSize = false;
            trkLightAzimuth.Location = new Point(8, 130);
            trkLightAzimuth.Maximum = 180;
            trkLightAzimuth.Minimum = -180;
            trkLightAzimuth.Name = "trkLightAzimuth";
            trkLightAzimuth.Size = new Size(230, 26);
            trkLightAzimuth.TabIndex = 5;
            trkLightAzimuth.TickStyle = TickStyle.None;
            trkLightAzimuth.Value = -45;

            lblLightElev.Location = new Point(8, 160);
            lblLightElev.Name = "lblLightElev";
            lblLightElev.Size = new Size(210, 16);
            lblLightElev.TabIndex = 6;
            lblLightElev.Text = "Высота: 45";

            trkLightElev.AutoSize = false;
            trkLightElev.Location = new Point(8, 176);
            trkLightElev.Maximum = 90;
            trkLightElev.Minimum = -90;
            trkLightElev.Name = "trkLightElev";
            trkLightElev.Size = new Size(230, 26);
            trkLightElev.TabIndex = 7;
            trkLightElev.TickStyle = TickStyle.None;
            trkLightElev.Value = 45;

            chkSpecular.AutoSize = true;
            chkSpecular.Checked = true;
            chkSpecular.CheckState = CheckState.Checked;
            chkSpecular.Location = new Point(8, 208);
            chkSpecular.Name = "chkSpecular";
            chkSpecular.Size = new Size(140, 19);
            chkSpecular.TabIndex = 8;
            chkSpecular.Text = "Центральный блик";

            grpEdges.Controls.Add(chkEdges);
            grpEdges.Controls.Add(lblEdgeThickTitle);
            grpEdges.Controls.Add(nudEdgeThickness);
            grpEdges.Controls.Add(cmbEdgeMode);
            grpEdges.Controls.Add(btnEdgeColor);
            grpEdges.Location = new Point(6, 1182);
            grpEdges.Name = "grpEdges";
            grpEdges.Size = new Size(260, 118);
            grpEdges.TabIndex = 5;
            grpEdges.TabStop = false;
            grpEdges.Text = "Контуры";

            chkEdges.Location = new Point(8, 22);
            chkEdges.Name = "chkEdges";
            chkEdges.Size = new Size(145, 24);
            chkEdges.TabIndex = 0;
            chkEdges.Text = "Обводка";

            lblEdgeThickTitle.Location = new Point(8, 52);
            lblEdgeThickTitle.Name = "lblEdgeThickTitle";
            lblEdgeThickTitle.Size = new Size(64, 23);
            lblEdgeThickTitle.TabIndex = 1;
            lblEdgeThickTitle.Text = "Толщина:";
            lblEdgeThickTitle.TextAlign = ContentAlignment.MiddleRight;

            nudEdgeThickness.Location = new Point(76, 50);
            nudEdgeThickness.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            nudEdgeThickness.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudEdgeThickness.Name = "nudEdgeThickness";
            nudEdgeThickness.Size = new Size(52, 23);
            nudEdgeThickness.TabIndex = 2;
            nudEdgeThickness.Value = new decimal(new int[] { 1, 0, 0, 0 });

            cmbEdgeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEdgeMode.Items.AddRange(new object[] { "Снаружи", "Внутри" });
            cmbEdgeMode.Location = new Point(136, 50);
            cmbEdgeMode.Name = "cmbEdgeMode";
            cmbEdgeMode.Size = new Size(110, 23);
            cmbEdgeMode.TabIndex = 3;

            btnEdgeColor.BackColor = Color.Black;
            btnEdgeColor.ForeColor = Color.White;
            btnEdgeColor.Location = new Point(8, 82);
            btnEdgeColor.Name = "btnEdgeColor";
            btnEdgeColor.Size = new Size(140, 26);
            btnEdgeColor.TabIndex = 4;
            btnEdgeColor.Tag = "notheme";
            btnEdgeColor.Text = "Цвет обводки";
            btnEdgeColor.UseVisualStyleBackColor = false;

            grpBackground.Controls.Add(btnPickBgColor);
            grpBackground.Controls.Add(lblBgTolTitle);
            grpBackground.Controls.Add(nudBgTolerance);
            grpBackground.Controls.Add(btnRemoveBg);
            grpBackground.Location = new Point(6, 1306);
            grpBackground.Name = "grpBackground";
            grpBackground.Size = new Size(260, 116);
            grpBackground.TabIndex = 6;
            grpBackground.TabStop = false;
            grpBackground.Text = "Фон";

            btnPickBgColor.Location = new Point(8, 22);
            btnPickBgColor.Name = "btnPickBgColor";
            btnPickBgColor.Size = new Size(160, 26);
            btnPickBgColor.TabIndex = 0;
            btnPickBgColor.Text = "Выбрать цвет фона";

            lblBgTolTitle.Location = new Point(8, 57);
            lblBgTolTitle.Name = "lblBgTolTitle";
            lblBgTolTitle.Size = new Size(52, 20);
            lblBgTolTitle.TabIndex = 1;
            lblBgTolTitle.Text = "Допуск:";
            lblBgTolTitle.TextAlign = ContentAlignment.MiddleRight;

            nudBgTolerance.Location = new Point(64, 55);
            nudBgTolerance.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudBgTolerance.Name = "nudBgTolerance";
            nudBgTolerance.Size = new Size(52, 23);
            nudBgTolerance.TabIndex = 2;
            nudBgTolerance.Value = new decimal(new int[] { 15, 0, 0, 0 });

            btnRemoveBg.Location = new Point(8, 82);
            btnRemoveBg.Name = "btnRemoveBg";
            btnRemoveBg.Size = new Size(130, 26);
            btnRemoveBg.TabIndex = 3;
            btnRemoveBg.Text = "Удалить фон";

            rightSplit.Dock = DockStyle.Fill;
            rightSplit.Location = new Point(0, 0);
            rightSplit.Name = "rightSplit";
            rightSplit.Orientation = Orientation.Horizontal;

            rightSplit.Panel1.Controls.Add(previewSplit);
            rightSplit.Panel1MinSize = 150;

            rightSplit.Panel2.Controls.Add(tabBottom);
            rightSplit.Panel2MinSize = 100;
            rightSplit.Size = new Size(940, 708);
            rightSplit.SplitterDistance = 274;
            rightSplit.TabIndex = 0;

            previewSplit.Dock = DockStyle.Fill;
            previewSplit.Location = new Point(0, 0);
            previewSplit.Name = "previewSplit";

            previewSplit.Panel1.Controls.Add(panelBefore);

            previewSplit.Panel2.Controls.Add(panelAfter);
            previewSplit.Size = new Size(940, 274);
            previewSplit.SplitterDistance = 575;
            previewSplit.TabIndex = 0;

            panelBefore.Controls.Add(btnDropHint);
            panelBefore.Controls.Add(picBefore);
            panelBefore.Controls.Add(panelBeforeTitle);
            panelBefore.Dock = DockStyle.Fill;
            panelBefore.Location = new Point(0, 0);
            panelBefore.Name = "panelBefore";
            panelBefore.Size = new Size(575, 274);
            panelBefore.TabIndex = 0;

            btnDropHint.AllowDrop = true;
            btnDropHint.BackColor = Color.FromArgb(28, 28, 28);
            btnDropHint.Cursor = Cursors.Hand;
            btnDropHint.Dock = DockStyle.Fill;
            btnDropHint.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 88);
            btnDropHint.FlatAppearance.MouseDownBackColor = Color.FromArgb(38, 38, 42);
            btnDropHint.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 34, 38);
            btnDropHint.FlatStyle = FlatStyle.Flat;
            btnDropHint.Font = new Font("Segoe UI", 11F);
            btnDropHint.ForeColor = Color.FromArgb(160, 160, 168);
            btnDropHint.Location = new Point(0, 24);
            btnDropHint.Name = "btnDropHint";
            btnDropHint.Size = new Size(575, 250);
            btnDropHint.TabIndex = 2;
            btnDropHint.TabStop = false;
            btnDropHint.Tag = "notheme";
            btnDropHint.Text = "📂\n\nПеретащите изображение сюда\nили нажмите, чтобы выбрать файл\n\nСовет: лучше всего подходят\nквадратные изображения (1:1)";
            btnDropHint.UseVisualStyleBackColor = false;

            picBefore.AllowDrop = true;
            picBefore.BackColor = Color.FromArgb(28, 28, 28);
            picBefore.Dock = DockStyle.Fill;
            picBefore.Location = new Point(0, 24);
            picBefore.Name = "picBefore";
            picBefore.Size = new Size(575, 250);
            picBefore.SizeMode = PictureBoxSizeMode.Zoom;
            picBefore.TabIndex = 0;
            picBefore.TabStop = false;

            panelBeforeTitle.BackColor = Color.FromArgb(50, 50, 55);
            panelBeforeTitle.Controls.Add(lblBeforeTitle);
            panelBeforeTitle.Controls.Add(btnDetachBefore);
            panelBeforeTitle.Dock = DockStyle.Top;
            panelBeforeTitle.Location = new Point(0, 0);
            panelBeforeTitle.Name = "panelBeforeTitle";
            panelBeforeTitle.Size = new Size(575, 24);
            panelBeforeTitle.TabIndex = 1;

            lblBeforeTitle.BackColor = Color.FromArgb(50, 50, 55);
            lblBeforeTitle.Dock = DockStyle.Fill;
            lblBeforeTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblBeforeTitle.ForeColor = Color.White;
            lblBeforeTitle.Location = new Point(0, 0);
            lblBeforeTitle.Name = "lblBeforeTitle";
            lblBeforeTitle.Size = new Size(549, 24);
            lblBeforeTitle.TabIndex = 1;
            lblBeforeTitle.Text = "ДО";
            lblBeforeTitle.TextAlign = ContentAlignment.MiddleCenter;

            btnDetachBefore.BackColor = Color.FromArgb(50, 50, 55);
            btnDetachBefore.Cursor = Cursors.Hand;
            btnDetachBefore.Dock = DockStyle.Right;
            btnDetachBefore.FlatAppearance.BorderSize = 0;
            btnDetachBefore.FlatStyle = FlatStyle.Flat;
            btnDetachBefore.ForeColor = Color.White;
            btnDetachBefore.Location = new Point(549, 0);
            btnDetachBefore.Name = "btnDetachBefore";
            btnDetachBefore.Size = new Size(26, 24);
            btnDetachBefore.TabIndex = 2;
            btnDetachBefore.Text = "⊡";
            btnDetachBefore.UseVisualStyleBackColor = false;

            panelAfter.Controls.Add(picAfter);
            panelAfter.Controls.Add(panelAfterTitle);
            panelAfter.Dock = DockStyle.Fill;
            panelAfter.Location = new Point(0, 0);
            panelAfter.Name = "panelAfter";
            panelAfter.Size = new Size(361, 274);
            panelAfter.TabIndex = 0;

            picAfter.BackColor = Color.FromArgb(28, 28, 28);
            picAfter.Dock = DockStyle.Fill;
            picAfter.Location = new Point(0, 24);
            picAfter.Name = "picAfter";
            picAfter.Size = new Size(361, 250);
            picAfter.SizeMode = PictureBoxSizeMode.Zoom;
            picAfter.TabIndex = 0;
            picAfter.TabStop = false;

            panelAfterTitle.BackColor = Color.FromArgb(50, 50, 55);
            panelAfterTitle.Controls.Add(lblAfterTitle);
            panelAfterTitle.Controls.Add(btnDetachAfter);
            panelAfterTitle.Dock = DockStyle.Top;
            panelAfterTitle.Location = new Point(0, 0);
            panelAfterTitle.Name = "panelAfterTitle";
            panelAfterTitle.Size = new Size(361, 24);
            panelAfterTitle.TabIndex = 1;

            lblAfterTitle.BackColor = Color.FromArgb(50, 50, 55);
            lblAfterTitle.Dock = DockStyle.Fill;
            lblAfterTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblAfterTitle.ForeColor = Color.White;
            lblAfterTitle.Location = new Point(0, 0);
            lblAfterTitle.Name = "lblAfterTitle";
            lblAfterTitle.Size = new Size(335, 24);
            lblAfterTitle.TabIndex = 1;
            lblAfterTitle.Text = "ПОСЛЕ";
            lblAfterTitle.TextAlign = ContentAlignment.MiddleCenter;

            btnDetachAfter.BackColor = Color.FromArgb(50, 50, 55);
            btnDetachAfter.Cursor = Cursors.Hand;
            btnDetachAfter.Dock = DockStyle.Right;
            btnDetachAfter.FlatAppearance.BorderSize = 0;
            btnDetachAfter.FlatStyle = FlatStyle.Flat;
            btnDetachAfter.ForeColor = Color.White;
            btnDetachAfter.Location = new Point(335, 0);
            btnDetachAfter.Name = "btnDetachAfter";
            btnDetachAfter.Size = new Size(26, 24);
            btnDetachAfter.TabIndex = 2;
            btnDetachAfter.Text = "⊡";
            btnDetachAfter.UseVisualStyleBackColor = false;

            tabBottom.Controls.Add(tabPageEditor);
            tabBottom.Controls.Add(tabPagePalette);
            tabBottom.Controls.Add(tabPageAnimation);
            tabBottom.Dock = DockStyle.Fill;
            tabBottom.Location = new Point(0, 0);
            tabBottom.Name = "tabBottom";
            tabBottom.SelectedIndex = 0;
            tabBottom.Size = new Size(940, 430);
            tabBottom.TabIndex = 0;

            tabPageEditor.Controls.Add(editorScroll);
            tabPageEditor.Controls.Add(editorToolStrip);
            tabPageEditor.Location = new Point(4, 24);
            tabPageEditor.Name = "tabPageEditor";
            tabPageEditor.Size = new Size(932, 402);
            tabPageEditor.TabIndex = 0;
            tabPageEditor.Text = "Редактор пикселей";

            editorScroll.AutoScroll = true;
            editorScroll.BackColor = Color.FromArgb(50, 50, 50);
            editorScroll.Controls.Add(_pixelEditor);
            editorScroll.Dock = DockStyle.Fill;
            editorScroll.Location = new Point(0, 32);
            editorScroll.Name = "editorScroll";
            editorScroll.Size = new Size(932, 370);
            editorScroll.TabIndex = 0;

            _pixelEditor.BackColor = Color.FromArgb(40, 40, 40);
            _pixelEditor.EditImage = null;
            _pixelEditor.EraserTransparent = true;
            _pixelEditor.ForeColor2 = Color.Black;
            _pixelEditor.Location = new Point(0, 0);
            _pixelEditor.Name = "_pixelEditor";
            _pixelEditor.Size = new Size(512, 512);
            _pixelEditor.TabIndex = 0;
            _pixelEditor.Tool = UI.EditorTool.Pencil;
            _pixelEditor.Zoom = 8;

            editorToolStrip.BackColor = Color.FromArgb(230, 230, 230);
            editorToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            editorToolStrip.Items.AddRange(new ToolStripItem[] { btnToolPencil, btnToolLine, btnToolRect, btnToolFill, btnToolEyedrop, btnToolEraser, btnZoom1, btnZoom2, btnZoom4, btnZoom8, btnZoom16, btnEditorColor, chkEditorPaletteOnly, chkEraserTransparent, btnEditorApply, btnDetachEditor });
            editorToolStrip.Location = new Point(0, 0);
            editorToolStrip.Name = "editorToolStrip";
            editorToolStrip.Size = new Size(932, 32);
            editorToolStrip.TabIndex = 1;

            btnToolPencil.Checked = true;
            btnToolPencil.CheckOnClick = true;
            btnToolPencil.CheckState = CheckState.Checked;
            btnToolPencil.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToolPencil.Name = "btnToolPencil";
            btnToolPencil.Size = new Size(23, 29);
            btnToolPencil.Tag = "Карандаш";
            btnToolPencil.Text = "✏";
            btnToolPencil.ToolTipText = "Карандаш";

            btnToolLine.CheckOnClick = true;
            btnToolLine.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToolLine.Name = "btnToolLine";
            btnToolLine.Size = new Size(23, 29);
            btnToolLine.Tag = "Линия";
            btnToolLine.Text = "╱";
            btnToolLine.ToolTipText = "Линия";

            btnToolRect.CheckOnClick = true;
            btnToolRect.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToolRect.Name = "btnToolRect";
            btnToolRect.Size = new Size(23, 29);
            btnToolRect.Tag = "Прямоугольник";
            btnToolRect.Text = "▭";
            btnToolRect.ToolTipText = "Прямоугольник";

            btnToolFill.CheckOnClick = true;
            btnToolFill.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToolFill.Name = "btnToolFill";
            btnToolFill.Size = new Size(23, 29);
            btnToolFill.Tag = "Заливка";
            btnToolFill.Text = "⬛";
            btnToolFill.ToolTipText = "Заливка";

            btnToolEyedrop.CheckOnClick = true;
            btnToolEyedrop.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToolEyedrop.Name = "btnToolEyedrop";
            btnToolEyedrop.Size = new Size(23, 29);
            btnToolEyedrop.Tag = "Пипетка";
            btnToolEyedrop.Text = "🔍";
            btnToolEyedrop.ToolTipText = "Пипетка";

            btnToolEraser.CheckOnClick = true;
            btnToolEraser.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnToolEraser.Name = "btnToolEraser";
            btnToolEraser.Size = new Size(23, 29);
            btnToolEraser.Tag = "Ластик";
            btnToolEraser.Text = "⬜";
            btnToolEraser.ToolTipText = "Ластик";

            btnZoom1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoom1.Name = "btnZoom1";
            btnZoom1.Size = new Size(23, 29);
            btnZoom1.Text = "1x";

            btnZoom2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoom2.Name = "btnZoom2";
            btnZoom2.Size = new Size(23, 29);
            btnZoom2.Text = "2x";

            btnZoom4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoom4.Name = "btnZoom4";
            btnZoom4.Size = new Size(23, 29);
            btnZoom4.Text = "4x";

            btnZoom8.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoom8.Name = "btnZoom8";
            btnZoom8.Size = new Size(23, 29);
            btnZoom8.Text = "8x";

            btnZoom16.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoom16.Name = "btnZoom16";
            btnZoom16.Size = new Size(29, 29);
            btnZoom16.Text = "16x";

            btnEditorColor.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnEditorColor.Font = new Font("Segoe UI", 14F);
            btnEditorColor.ForeColor = Color.Black;
            btnEditorColor.Name = "btnEditorColor";
            btnEditorColor.Size = new Size(32, 29);
            btnEditorColor.Text = "■";
            btnEditorColor.ToolTipText = "Цвет карандаша (клик для выбора)";

            chkEditorPaletteOnly.CheckOnClick = true;
            chkEditorPaletteOnly.Name = "chkEditorPaletteOnly";
            chkEditorPaletteOnly.Size = new Size(58, 29);
            chkEditorPaletteOnly.Text = "Палитра";
            chkEditorPaletteOnly.ToolTipText = "Выбирать только цвета из текущей палитры";

            chkEraserTransparent.Checked = true;
            chkEraserTransparent.CheckOnClick = true;
            chkEraserTransparent.CheckState = CheckState.Checked;
            chkEraserTransparent.Name = "chkEraserTransparent";
            chkEraserTransparent.Size = new Size(89, 29);
            chkEraserTransparent.Text = "Прозр. ластик";
            chkEraserTransparent.ToolTipText = "Ластик стирает в прозрачный (иначе в фон)";

            btnEditorApply.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnEditorApply.Name = "btnEditorApply";
            btnEditorApply.Size = new Size(74, 29);
            btnEditorApply.Text = "В результат";
            btnEditorApply.ToolTipText = "Скопировать из редактора в результат";

            btnDetachEditor.Alignment = ToolStripItemAlignment.Right;
            btnDetachEditor.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDetachEditor.Name = "btnDetachEditor";
            btnDetachEditor.Size = new Size(23, 29);
            btnDetachEditor.Text = "⊡";
            btnDetachEditor.ToolTipText = "Открепить вкладку редактора";

            tabPagePalette.Controls.Add(panelPaletteBar);
            tabPagePalette.Controls.Add(_palettePanel);
            tabPagePalette.Location = new Point(4, 24);
            tabPagePalette.Name = "tabPagePalette";
            tabPagePalette.Size = new Size(932, 402);
            tabPagePalette.TabIndex = 1;
            tabPagePalette.Text = "Палитра";

            _palettePanel.Dock = DockStyle.Top;
            _palettePanel.Location = new Point(0, 0);
            _palettePanel.Name = "_palettePanel";
            _palettePanel.Size = new Size(932, 60);
            _palettePanel.TabIndex = 1;

            panelPaletteBar.BackColor = Color.FromArgb(45, 45, 48);
            panelPaletteBar.Controls.Add(btnPickPaletteColor);
            panelPaletteBar.Controls.Add(btnDetachPalette);
            panelPaletteBar.Dock = DockStyle.Top;
            panelPaletteBar.Location = new Point(0, 0);
            panelPaletteBar.Name = "panelPaletteBar";
            panelPaletteBar.Padding = new Padding(4, 2, 4, 2);
            panelPaletteBar.Size = new Size(932, 30);
            panelPaletteBar.TabIndex = 0;

            btnPickPaletteColor.BackColor = Color.FromArgb(60, 60, 60);
            btnPickPaletteColor.Cursor = Cursors.Hand;
            btnPickPaletteColor.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnPickPaletteColor.FlatStyle = FlatStyle.Flat;
            btnPickPaletteColor.ForeColor = Color.White;
            btnPickPaletteColor.Location = new Point(4, 3);
            btnPickPaletteColor.Name = "btnPickPaletteColor";
            btnPickPaletteColor.Size = new Size(80, 24);
            btnPickPaletteColor.TabIndex = 0;
            btnPickPaletteColor.Text = "Пипетка";
            btnPickPaletteColor.UseVisualStyleBackColor = false;

            btnDetachPalette.BackColor = Color.FromArgb(60, 60, 60);
            btnDetachPalette.Cursor = Cursors.Hand;
            btnDetachPalette.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnDetachPalette.FlatStyle = FlatStyle.Flat;
            btnDetachPalette.ForeColor = Color.White;
            btnDetachPalette.Location = new Point(92, 3);
            btnDetachPalette.Name = "btnDetachPalette";
            btnDetachPalette.Size = new Size(26, 24);
            btnDetachPalette.TabIndex = 1;
            btnDetachPalette.Text = "⊡";
            btnDetachPalette.UseVisualStyleBackColor = false;
            btnDetachPalette.Click += btnDetachPalette_Click;

            tabPageAnimation.Controls.Add(flpFrames);
            tabPageAnimation.Controls.Add(pnlAnimToolbar);
            tabPageAnimation.Location = new Point(4, 24);
            tabPageAnimation.Name = "tabPageAnimation";
            tabPageAnimation.Size = new Size(932, 402);
            tabPageAnimation.TabIndex = 2;
            tabPageAnimation.Text = "Анимация";

            flpFrames.AutoScroll = true;
            flpFrames.BackColor = Color.FromArgb(30, 30, 30);
            flpFrames.Dock = DockStyle.Fill;
            flpFrames.Location = new Point(0, 40);
            flpFrames.Name = "flpFrames";
            flpFrames.Size = new Size(932, 362);
            flpFrames.TabIndex = 0;
            flpFrames.WrapContents = false;

            pnlAnimToolbar.Controls.Add(btnAnimPlay);
            pnlAnimToolbar.Controls.Add(btnAnimStop);
            pnlAnimToolbar.Controls.Add(lblAnimFrame);
            pnlAnimToolbar.Controls.Add(lblFpsTitle);
            pnlAnimToolbar.Controls.Add(nudAnimFps);
            pnlAnimToolbar.Controls.Add(btnProcessFrames);
            pnlAnimToolbar.Controls.Add(btnExportGif);
            pnlAnimToolbar.Controls.Add(btnDetachAnim);
            pnlAnimToolbar.Dock = DockStyle.Top;
            pnlAnimToolbar.Location = new Point(0, 0);
            pnlAnimToolbar.Name = "pnlAnimToolbar";
            pnlAnimToolbar.Size = new Size(932, 40);
            pnlAnimToolbar.TabIndex = 1;

            btnAnimPlay.Location = new Point(8, 6);
            btnAnimPlay.Name = "btnAnimPlay";
            btnAnimPlay.Size = new Size(90, 28);
            btnAnimPlay.TabIndex = 0;
            btnAnimPlay.Text = "Играть";

            btnAnimStop.Enabled = false;
            btnAnimStop.Location = new Point(104, 6);
            btnAnimStop.Name = "btnAnimStop";
            btnAnimStop.Size = new Size(70, 28);
            btnAnimStop.TabIndex = 1;
            btnAnimStop.Text = "Стоп";

            lblAnimFrame.Location = new Point(184, 12);
            lblAnimFrame.Name = "lblAnimFrame";
            lblAnimFrame.Size = new Size(100, 23);
            lblAnimFrame.TabIndex = 2;
            lblAnimFrame.Text = "Кадр: -";

            lblFpsTitle.Location = new Point(290, 12);
            lblFpsTitle.Name = "lblFpsTitle";
            lblFpsTitle.Size = new Size(30, 23);
            lblFpsTitle.TabIndex = 3;
            lblFpsTitle.Text = "FPS:";

            nudAnimFps.Location = new Point(325, 8);
            nudAnimFps.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            nudAnimFps.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudAnimFps.Name = "nudAnimFps";
            nudAnimFps.Size = new Size(50, 23);
            nudAnimFps.TabIndex = 4;
            nudAnimFps.Value = new decimal(new int[] { 12, 0, 0, 0 });

            btnProcessFrames.Location = new Point(385, 6);
            btnProcessFrames.Name = "btnProcessFrames";
            btnProcessFrames.Size = new Size(150, 28);
            btnProcessFrames.TabIndex = 5;
            btnProcessFrames.Text = "Обработать кадры";

            btnExportGif.Location = new Point(541, 6);
            btnExportGif.Name = "btnExportGif";
            btnExportGif.Size = new Size(110, 28);
            btnExportGif.TabIndex = 6;
            btnExportGif.Text = "Экспорт GIF";

            btnDetachAnim.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDetachAnim.BackColor = Color.FromArgb(60, 60, 60);
            btnDetachAnim.Cursor = Cursors.Hand;
            btnDetachAnim.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            btnDetachAnim.FlatStyle = FlatStyle.Flat;
            btnDetachAnim.ForeColor = Color.White;
            btnDetachAnim.Location = new Point(543, 6);
            btnDetachAnim.Name = "btnDetachAnim";
            btnDetachAnim.Size = new Size(36, 28);
            btnDetachAnim.TabIndex = 7;
            btnDetachAnim.Text = "⊡";
            btnDetachAnim.UseVisualStyleBackColor = false;

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1284, 781);
            Controls.Add(mainSplit);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(960, 640);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KebuzForge";
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            mainSplit.Panel1.ResumeLayout(false);
            mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
            mainSplit.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            grpPixelize.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudHeight).EndInit();
            grpPalette.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudColorCount).EndInit();
            grpDither.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trkDitherIntensity).EndInit();
            grpEffects.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trkSphereScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkSphereBulge).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkCornerSharp).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkOffsetU).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkOffsetV).EndInit();
            grpLighting.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trkAmbient).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkDiffuse).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkLightAzimuth).EndInit();
            ((System.ComponentModel.ISupportInitialize)trkLightElev).EndInit();
            grpEdges.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudEdgeThickness).EndInit();
            grpBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudBgTolerance).EndInit();
            rightSplit.Panel1.ResumeLayout(false);
            rightSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rightSplit).EndInit();
            rightSplit.ResumeLayout(false);
            previewSplit.Panel1.ResumeLayout(false);
            previewSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)previewSplit).EndInit();
            previewSplit.ResumeLayout(false);
            panelBefore.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picBefore).EndInit();
            panelBeforeTitle.ResumeLayout(false);
            panelAfter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picAfter).EndInit();
            panelAfterTitle.ResumeLayout(false);
            tabBottom.ResumeLayout(false);
            tabPageEditor.ResumeLayout(false);
            tabPageEditor.PerformLayout();
            editorScroll.ResumeLayout(false);
            editorToolStrip.ResumeLayout(false);
            editorToolStrip.PerformLayout();
            tabPagePalette.ResumeLayout(false);
            panelPaletteBar.ResumeLayout(false);
            tabPageAnimation.ResumeLayout(false);
            pnlAnimToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)nudAnimFps).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private MenuStrip menuStrip;
        private ToolStripMenuItem menuFile, menuEdit;
        private ToolStripMenuItem menuFileOpen, menuFileExit;
        private ToolStripMenuItem menuEditUndo, menuEditRedo, menuEditReset;
        private ToolStripMenuItem menuPresets, menuPresetSave;
        private ToolStripMenuItem menuExport, menuExportPng, menuExportIndexed, menuExportIco, menuExportBatch;
        private ToolStripMenuItem menuView, miLight, miDark, miHybrid;
        private ToolStripSeparator miLangSeparator;
        private ToolStripMenuItem miLangRu, miLangEn;
        private ToolStripMenuItem menuPlugins, menuPluginsFolder;
        private ToolStrip toolStrip;
        private ToolStripButton btnOpen, btnUndo, btnRedo;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusOriginal, lblStatusResult, lblStatusZoom;
        private SplitContainer mainSplit, rightSplit, previewSplit;
        private Panel leftPanel, panelBefore, panelAfter;
        private PictureBox picBefore;
        private UI.PixelPictureBox picAfter;
        private Label lblBeforeTitle, lblAfterTitle;
        private TabControl tabBottom;
        private TabPage tabPageEditor, tabPagePalette, tabPageAnimation;

        private GroupBox grpPixelize, grpPalette, grpDither, grpEffects, grpLighting, grpEdges, grpBackground;

        private Label lblWidthTitle, lblHeightTitle, lblAlgoTitle;
        private NumericUpDown nudWidth, nudHeight;
        private CheckBox chkKeepAspect;
        private ComboBox cmbPixelAlgorithm;
        private Button btnApplyPixelize;

        private Label lblColorCountTitle;
        private NumericUpDown nudColorCount;
        private ComboBox cmbRetroPalette;
        private Button btnApplyPalette, btnSavePalette, btnLoadPalette;

        private ComboBox cmbDitherMode;
        private Label lblDitherVal;
        private TrackBar trkDitherIntensity;
        private Button btnApplyDither;

        private RadioButton rbEffectNone, rbEffectSphere, rbEffectCylinder, rbEffectRounded;
        private CheckBox chkShapeFromRect;
        private CheckBox chkSpecular;
        private ComboBox cmbCylinderDir;
        private Label lblSphereScale, lblSphereBulge, lblOffsetU, lblOffsetV, lblCornerSharp;
        private TrackBar trkSphereScale, trkSphereBulge, trkOffsetU, trkOffsetV, trkCornerSharp;
        private Button btnApplyEffect;

        private Label lblAmbient, lblDiffuse, lblLightAzimuth, lblLightElev;
        private TrackBar trkAmbient, trkDiffuse, trkLightAzimuth, trkLightElev;

        private CheckBox chkEdges;
        private ComboBox cmbEdgeMode;
        private Button btnEdgeColor;
        private Button btnDropHint;
        private Label lblEdgeThickTitle;
        private NumericUpDown nudEdgeThickness;

        private Button btnPickBgColor, btnRemoveBg;
        private Label lblBgTolTitle;
        private NumericUpDown nudBgTolerance;

        private ToolStrip editorToolStrip;
        private ToolStripButton btnToolPencil, btnToolLine, btnToolRect, btnToolFill, btnToolEyedrop, btnToolEraser;
        private ToolStripButton btnZoom1, btnZoom2, btnZoom4, btnZoom8, btnZoom16;
        private ToolStripButton btnEditorColor, btnEditorApply;
        private ToolStripButton chkEraserTransparent, chkEditorPaletteOnly;
        private Panel editorScroll;
        private UI.PixelEditorPanel _pixelEditor = null!;
        private UI.PalettePanel _palettePanel = null!;
        private ToolStripButton btnDetachEditor;

        private Panel panelPaletteBar;
        private Button btnPickPaletteColor, btnDetachPalette;

        private Panel panelBeforeTitle, panelAfterTitle;
        private Button btnDetachBefore, btnDetachAfter;

        private Panel pnlAnimToolbar;
        private Button btnAnimPlay, btnAnimStop, btnProcessFrames, btnExportGif, btnDetachAnim;
        private Label lblAnimFrame, lblFpsTitle;
        private NumericUpDown nudAnimFps;
        private FlowLayoutPanel flpFrames;
    }
}
