using Devart.Windows.Forms.XtraGrid.Views.WinExplorer;
using Devart.Windows.Forms.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.WinExplorer.ViewInfo;
using DevExpress.XtraGrid.Views.WinExplorer;
using DevExpress.XtraGrid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DevExpress.XtraEditors.ViewInfo.BaseListBoxViewInfo;
using System.Windows.Forms;

namespace EditButtonApp
{
  public class ImageListView : DGridControl
  {
    private CustomWinExplorerView winExplorerView;
    private System.ComponentModel.Container components = null;
    private List<ImageListViewItem> items;

    private int dataIndexToSelect;

    private class CustomWinExplorerView : DWinExplorerView
    {
      public override bool IsEditing => ActiveEditor != null;
    }

    public ImageListView()
    {
      InitializeComponent();

      this.winExplorerView.OptionsViewStyles.Large.ItemWidth = 90;
      this.winExplorerView.OptionsViewStyles.Small.ImageSize = new Size(16, 16);
      this.winExplorerView.OptionsViewStyles.Large.ImageSize = new Size(32, 32);
      LargeImageList.ImageSize = this.winExplorerView.OptionsViewStyles.Large.ImageSize;
      SmallImageList.ImageSize = this.winExplorerView.OptionsViewStyles.Small.ImageSize;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
      }

      base.Dispose(disposing);
    }

    public DevExpress.Utils.ImageCollection LargeImageList
    {
      get;
      private set;
    }

    public DevExpress.Utils.ImageCollection SmallImageList
    {
      get;
      private set;
    }

    public WinExplorerViewStyle ViewStyle
    {
      get => this.winExplorerView.OptionsView.Style;
      set => this.winExplorerView.OptionsView.Style = value;
    }

    public ImageListViewItem SelectedItem => (SelectedIndex >= 0) ? Items[SelectedIndex] : null;

    public bool IsAnyItemFocused => this.winExplorerView.FocusedRowHandle >= 0;

    public int SelectedIndex
    {
      get
      {
        if (IsAnyItemFocused) // Логично. + фикс Bug #45447
        {
          return this.winExplorerView.GetDataSourceRowIndex(this.winExplorerView.FocusedRowHandle);
        }
        else
        {
          return -1;
        }
      }
      set
      {
        if (this.winExplorerView.DataController.IsReady)
        {
          this.winExplorerView.FocusedRowHandle = this.winExplorerView.GetRowHandle(value);
        }
        else
        {
          this.dataIndexToSelect = value;
          Load += ImageListView_Load;
        }
      }
    }

    public List<ImageListViewItem> Items
    {
      get
      {
        if (this.items == null)
        {
          this.items = new List<ImageListViewItem>();
          DataSource = Items;
          this.winExplorerView.Columns.Add(new GridColumn() { FieldName = "SmallImage", Name = "SmallImage" });
          this.winExplorerView.Columns.Add(new GridColumn() { FieldName = "LargeImage", Name = "LargeImage" });
          this.winExplorerView.Columns.Add(new GridColumn() { FieldName = "Text", Name = "Text" });
          this.winExplorerView.Columns.Add(new GridColumn() { FieldName = "GroupName", Name = "GroupName" });
          this.winExplorerView.ColumnSet.TextColumn = this.winExplorerView.Columns["Text"];
          this.winExplorerView.ColumnSet.LargeImageColumn = this.winExplorerView.Columns["LargeImage"];
          this.winExplorerView.ColumnSet.SmallImageColumn = this.winExplorerView.Columns["SmallImage"];
          this.winExplorerView.ColumnSet.GroupColumn = this.winExplorerView.Columns["GroupName"];

          this.winExplorerView.ColumnSet.GroupColumn.SortMode = ColumnSortMode.Custom;
          this.winExplorerView.CustomColumnSort += winExplorerView_CustomColumnSort;
        }
        return this.items;
      }
    }

    public int GroupCount
    {
      get => this.winExplorerView.GroupCount;
      set => this.winExplorerView.GroupCount = value;
    }

    protected override void OnDoubleClick(EventArgs ev)
    {
      var arg = ev as MouseEventArgs;
      if (arg != null)
      {
        WinExplorerViewHitInfo hitInfo = this.winExplorerView.CalcHitInfo(arg.Location);
        if (hitInfo.InItem && ItemDoubleClick != null)
        {
          ItemDoubleClick(this, EventArgs.Empty);
          return;
        }
      }

      base.OnDoubleClick(ev);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.LargeImageList = new DevExpress.Utils.ImageCollection(this.components);
      this.SmallImageList = new DevExpress.Utils.ImageCollection(this.components);
      this.winExplorerView = new CustomWinExplorerView();
      ((System.ComponentModel.ISupportInitialize)this).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.winExplorerView)).BeginInit();
      this.SuspendLayout();
      // 
      // winExplorerView
      // 
      this.winExplorerView.Name = "winExplorerView";
      this.winExplorerView.GridControl = this;
      this.winExplorerView.OptionsBehavior.Editable = false;
      this.winExplorerView.OptionsNavigation.AutoMoveRowFocus = false;
      this.winExplorerView.OptionsView.Style = WinExplorerViewStyle.Large;
      this.winExplorerView.OptionsSelection.AllowMarqueeSelection = true;
      this.winExplorerView.OptionsSelection.MultiSelect = false;
      this.winExplorerView.OptionsSelection.ItemSelectionMode = IconItemSelectionMode.Click;
      this.winExplorerView.FocusedRowChanged += new DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventHandler(winExplorerView_FocusedRowChanged);
      this.winExplorerView.MouseDown += new MouseEventHandler(winExplorerView_MouseDown);
      // 
      // imageListLarge
      // 
      this.LargeImageList.ImageSize = new Size(32, 32);
      this.LargeImageList.TransparentColor = Color.Transparent;
      // 
      // imageListSmall
      //
      this.SmallImageList.ImageSize = new Size(16, 16);
      this.SmallImageList.TransparentColor = Color.Transparent;
      // 
      // ImageListView
      // 
      this.Name = "ImageListView";
      this.MainView = this.winExplorerView;
      this.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.winExplorerView});
      ((System.ComponentModel.ISupportInitialize)this).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.winExplorerView)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    private void ImageListView_Load(object sender, EventArgs e)
    {
      this.Load -= ImageListView_Load;
      SelectedIndex = this.dataIndexToSelect;
    }

    private void winExplorerView_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
    {
      this.winExplorerView.DataController.Selection.Clear();
      this.winExplorerView.DataController.Selection.SetSelected(e.FocusedRowHandle, true);

      SelectedIndexChanged?.Invoke(this, e);
    }

    private void winExplorerView_MouseDown(object sender, MouseEventArgs e)
    {
      WinExplorerViewHitInfo hitInfo = this.winExplorerView.CalcHitInfo(e.Location);
      if (!hitInfo.InItem)
      {
        ((DevExpress.Utils.DXMouseEventArgs)(e)).Handled = true;
      }
    }

    private void winExplorerView_CustomColumnSort(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnSortEventArgs e)
    {
      if (e.Column.FieldName == "GroupName")
      { // кастомная сортировка по GroupDispayOrder
        var item1 = e.RowObject1 as ImageListViewItem;
        var item2 = e.RowObject2 as ImageListViewItem;

        int diff = item1.GroupDisplayOrder - item2.GroupDisplayOrder;
        if (diff != 0)
        {
          int diffAbs = Math.Abs(diff);
          diff = diff / diffAbs;
          e.Handled = true;
        }
        e.Result = diff;
      }
    }

    public event EventHandler SelectedIndexChanged;
    public event EventHandler ItemDoubleClick;
  }

  [Serializable]
  public class ImageListViewItem
  {
    public ImageListViewItem(string text, Image small, Image large, string tooltip, string group, int groupDisplayOrder, ItemInfo info)
    {
      Text = text;
      SmallImage = small;
      LargeImage = large;
      Tooltip = tooltip;
      GroupName = group;
      GroupDisplayOrder = groupDisplayOrder;
      Info = info;
    }

    public Image SmallImage
    {
      get; private set;
    }

    public Image LargeImage
    {
      get; private set;
    }

    public string Text
    {
      get; private set;
    }

    public string GroupName
    {
      get; private set;
    }

    public int GroupDisplayOrder
    {
      get; private set;
    }

    public string Tooltip
    {
      get; private set;
    }

    public ItemInfo Info
    {
      get; private set;
    }
  }
}
