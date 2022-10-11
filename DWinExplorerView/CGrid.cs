using DevExpress.Accessibility;
using DevExpress.XtraGrid.Accessibility;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Base;

namespace EditButtonApp
{
  public class CGridControl : GridControl
  {
    protected override void RegisterAvailableViewsCore(InfoCollection collection)
    {
      base.RegisterAvailableViewsCore(collection);
      collection.Add(new CGridViewInfoRegistrator());
    }
    protected override BaseView CreateDefaultView()
    {
      return CreateView("CGridView");
    }
  }

  public class CGridView : GridView, IAccessibleGrid
  {
    public CGridView() : this(null) { }

    public CGridView(GridControl gc)
        : base(gc)
    {
    }

    protected override string ViewName { get { return "CGridView"; } }

    private string rowNameField;
    public string RowNameField { get { return rowNameField; } set { rowNameField = value; OnPropertiesChanged(); } }
    public string GetRowName(int rowHandle)
    {
      if (!this.IsDataRow(rowHandle)) return null;
      return this.GetRowCellValue(rowHandle, RowNameField) as string;
    }

    protected override void RaiseCustomDrawRowIndicator(RowIndicatorCustomDrawEventArgs e)
    {
      if (e.Info != null)
      {
        e.Info.DisplayText = GetRowName(e.RowHandle);
      }
      base.RaiseCustomDrawRowIndicator(e);
    }

    public new int RowHandle2AccessibleIndex(int rowHandle)
    {
      return base.RowHandle2AccessibleIndex(rowHandle);
    }

    #region IAccessibleGrid Members

    IAccessibleGridRow IAccessibleGrid.GetRow(int index)
    {
      int rowHandle = this.AccessibleIndex2RowHandle(index);
      if (!this.IsValidRowHandle(rowHandle))
      {
        return null;
      }
      if (this.IsGroupRow(rowHandle))
      {
        return new GridAccessibleGroupRow(this, rowHandle);
      }
      return new GridAccessibleDataRowEx(this, rowHandle);
    }

    #endregion
  }

  public class CGridViewInfoRegistrator : GridInfoRegistrator
  {
    public override string ViewName { get { return "CGridView"; } }
    public override BaseView CreateView(GridControl grid)
    {
      return new CGridView(grid as GridControl);
    }
  }

  class GridAccessibleDataRowEx : GridAccessibleDataRow
  {
    public GridAccessibleDataRowEx(GridView view, int rowHandle) : base(view, rowHandle) { }

    protected override string GetAccessibleRowName()
    {
      if (View is CGridView && this.View.IsDataRow(base.RowHandle))
      {
        string name = ((CGridView)View).GetRowName(RowHandle);
        if (!string.IsNullOrEmpty(name))
          return string.Format(this.GetString(AccStringId.GridRow), name);
      }
      return base.GetAccessibleRowName();
    }
    public override IAccessibleGridRowCell GetCell(int index)
    {
      if (index >= base.View.VisibleColumns.Count)
      {
        return null;
      }
      return new GridAccessibleRowCellEx(base.View, base.RowHandle, base.View.VisibleColumns[index]);
    }
  }
  class GridAccessibleRowCellEx : GridAccessibleRowCell, IAccessibleGridRowCell
  {
    public GridAccessibleRowCellEx(GridView view, int rowHandle, GridColumn column) : base(view, rowHandle, column) { }

    #region IAccessibleGridRowCell Members

    string IAccessibleGridRowCell.GetName()
    {
      string str = (this.Column.GetTextCaption() == "") ? this.Column.ToolTip : this.Column.GetTextCaption();
      string rowName = ((IAccessibleGrid)this.View).GetRow(((CGridView)this.View).RowHandle2AccessibleIndex(this.RowHandle)).GetName();
      return (str + " " + rowName);
    }

    #endregion
  }
}
