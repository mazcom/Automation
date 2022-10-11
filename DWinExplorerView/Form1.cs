using Devart.Windows.Forms.XtraGrid;
using Devart.Windows.Forms.XtraGrid.Views.WinExplorer;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.WinExplorer;
using DevExpress.PivotGrid.QueryMode;

namespace EditButtonApp
{

  //class CustomWinExplorerView : DWinExplorerView
  //{
  //  //public override bool IsEditing => ActiveEditor != null;
  //}

  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();

      
      System.Data.DataSet dataSet1;
      System.Data.DataTable dataTable1;
      System.Data.DataColumn dataColumn1;
      System.Data.DataColumn dataColumn2;
      System.Data.DataColumn dataColumn3;

      dataSet1 = new System.Data.DataSet();
      dataTable1 = new System.Data.DataTable();
      dataColumn1 = new System.Data.DataColumn();
      dataColumn2 = new System.Data.DataColumn();
      dataColumn3 = new System.Data.DataColumn();
      //((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
      //((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
      //this.SuspendLayout();
      // 
      // dataSet1
      // 
      dataSet1.DataSetName = "NewDataSet";
      dataSet1.Tables.AddRange(new System.Data.DataTable[] {
            dataTable1});
      // 
      // dataTable1
      // 
      dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            dataColumn1,
            dataColumn2,
            dataColumn3});

      dataTable1.TableName = "Table1";
      // 
      // dataColumn1
      // 
      dataColumn1.ColumnName = "Column1";
      // 
      // dataColumn2
      // 
      dataColumn2.ColumnName = "GroupName";
      // 
      // dataColumn3
      // 
      //dataColumn3.ColumnName = "Column3";

      dataTable1.Rows.Add(new object[] { "gif", "Import Formats" });
      dataTable1.Rows.Add(new object[] { "txt", "Import Formats" });
      dataTable1.Rows.Add(new object[] { "json", "Import Formats" });
      dataTable1.Rows.Add(new object[] { "xml", "Import Formats" });

      dataTable1.Rows.Add(new object[] { "Load Template...", "User Templates" });

      //for (int i = 0; i < 5; i++)
      //{
      //  dataTable1.Rows.Add(new object[] { i, i * i, "abcde".Substring(i, 1) });
      //}


      GridControl grid = new DGridControl();
      grid.DataSource = dataTable1;

      DWinExplorerView view = new DWinExplorerView();
      view.GridControl = grid;

      view.Name = "winExplorerView";
      view.OptionsBehavior.Editable = false;
      view.OptionsNavigation.AutoMoveRowFocus = false;
      view.OptionsView.Style = WinExplorerViewStyle.Large;
      view.OptionsSelection.AllowMarqueeSelection = true;
      view.OptionsSelection.MultiSelect = false;
      view.OptionsSelection.ItemSelectionMode = IconItemSelectionMode.Click;

      grid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            view});

      view.OptionsViewStyles.Large.ItemWidth = 90;
      view.OptionsViewStyles.Small.ImageSize = new Size(16, 16);
      view.OptionsViewStyles.Large.ImageSize = new Size(32, 32);


      //view.Columns.Add(new GridColumn() { FieldName = "SmallImage", Name = "SmallImage" });
      //view.Columns.Add(new GridColumn() { FieldName = "LargeImage", Name = "LargeImage" });
      view.Columns.Add(new GridColumn() { FieldName = "Column1", Name = "Column1" });
      view.Columns.Add(new GridColumn() { FieldName = "GroupName", Name = "GroupName" });
      view.ColumnSet.TextColumn = view.Columns["Column1"];
      //view.ColumnSet.LargeImageColumn = view.Columns["LargeImage"];
      //view.ColumnSet.SmallImageColumn = view.Columns["SmallImage"];
      view.ColumnSet.GroupColumn = view.Columns["GroupName"];

      view.ColumnSet.GroupColumn.SortMode = ColumnSortMode.Custom;

      grid.MainView = view;
      view.OptionsBehavior.Editable = false;
      grid.Dock = DockStyle.Fill;
      this.Controls.Add(grid);
      

      /*
      GridControl grid = new CGridControl();
      grid.DataSource = dataTable1;
      CGridView view = new CGridView(grid);
      view.RowNameField = "Column3";
      grid.MainView = view;
      view.OptionsView.ShowAutoFilterRow = true;
      view.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
      view.IndicatorWidth = 40;
      view.OptionsBehavior.Editable = false;
      grid.Dock = DockStyle.Fill;
      this.Controls.Add(grid);
      */
    }
  }
}
