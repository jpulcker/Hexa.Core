#region Header

// ===================================================================================
// Copyright 2010 HexaSystems Corporation
// ===================================================================================
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// ===================================================================================
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// See the License for the specific language governing permissions and
// ===================================================================================

#endregion Header

namespace Hexa.Core.Web.UI.Controls
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class GridView : System.Web.UI.WebControls.GridView, IPostBackDataHandler, IPostBackEventHandler
    {
        #region Fields

        private bool _designMode = (HttpContext.Current == null);
        private bool _isScrollable = true;
        private Unit _maxHeight = Unit.Pixel(140);
        private bool _useDefaultCssClass = true;

        #endregion Fields

        #region Properties

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
                         MessageId = "System.Convert.ToBoolean(System.Object)")]
        public bool AllowClickRow
        {
            get
            {
                bool tempAllowClickRow = false;
                if (ViewState["AllowClickRow"] != null)
                {
                    return Convert.ToBoolean(ViewState["AllowClickRow"]);
                }
                else
                {
                    tempAllowClickRow = false;
                }
                return tempAllowClickRow;
            }
            set
            {
                ViewState["AllowClickRow"] = value;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
                         MessageId = "System.Convert.ToBoolean(System.Object)")]
        public bool AllowRowSelection
        {
            get
            {
                bool tempAllowRowSelection = false;
                if (ViewState["AllowRowSelection"] != null)
                {
                    return Convert.ToBoolean(ViewState["AllowRowSelection"]);
                }
                else
                {
                    tempAllowRowSelection = false;
                }
                return tempAllowRowSelection;
            }
            set
            {
                ViewState["AllowRowSelection"] = value;
            }
        }

        public bool IsScrollable
        {
            get
            {
                return _isScrollable;
            }
            set
            {
                _isScrollable = value;
            }
        }

        public Unit MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
                         MessageId = "System.Convert.ToBoolean(System.Object)")]
        public bool SelectFirstRow
        {
            get
            {
                bool tempSelectFirstRow = false;
                if (ViewState["SelectFirstRow"] != null)
                {
                    return Convert.ToBoolean(ViewState["SelectFirstRow"]);
                }
                else
                {
                    tempSelectFirstRow = true;
                }
                return tempSelectFirstRow;
            }
            set
            {
                ViewState["SelectFirstRow"] = value;
            }
        }

        public bool UseDefaultCssClass
        {
            get
            {
                return _useDefaultCssClass;
            }
            set
            {
                _useDefaultCssClass = value;
            }
        }

        #endregion Properties

        #region Methods

        public int GetSelectedItemIndex()
        {
            if (Rows.Count > 0)
            {
                return SelectedIndex;
            }
            else
            {
                return -1;
            }
        }

        public string GetSelectedItemUniqueId()
        {
            if (SelectedIndex >= 0)
            {
                return SelectedRow.Cells[0].Text;
            }
            else
            {
                return string.Empty;
            }
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            if (AllowRowSelection)
            {
                string hidxName = ClientID + "_idx";

                if (postCollection[hidxName] != null)
                {
                    SelectedIndex = Convert.ToInt32(postCollection[hidxName]);
                }
            }
            return false;
        }

        public void RaisePostDataChangedEvent()
        {
            //NOT IMPLEMENTED
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            base.SelectedIndexChanged += GridView_SelectedIndexChanged;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Page.RegisterRequiresPostBack(this);

            if (AllowRowSelection)
            {
                if (SelectFirstRow && SelectedIndex < 0)
                {
                    SelectedIndex = 0;
                }

                string hidxName = ClientID + "_idx";

                Attributes.Add("hidxName", hidxName);

                Page.ClientScript.RegisterHiddenField(hidxName, SelectedIndex.ToString());

                if (!(Page.ClientScript.IsClientScriptBlockRegistered("SelectMSGridRow")))
                {
                    var sb = new StringBuilder();
                    sb.Append("<script language=\"javascript\">").Append("\r\n");
                    sb.Append("function MSDatagridSelectRow(grdID, row)").Append("\r\n");
                    sb.Append("{var grd = document.getElementById(grdID);").Append("\r\n");
                    sb.Append("var hdn = document.getElementsByName(grd.getAttribute('hidxName'));").Append("\r\n");
                    sb.Append("var actualidx = parseInt(row) + 1;").Append("\r\n");
                    sb.Append("if (actualidx > 0) {").Append("\r\n");
                    sb.Append("grd.rows.item(actualidx).setAttribute('ocolor', row.className);").Append("\r\n");
                    sb.Append(
                        "grd.rows.item(parseInt(hdn.item(0).value) + 1).className = grd.rows.item(parseInt(hdn.item(0).value) + 1).getAttribute('ocolor');")
                    .Append("\r\n");
                    sb.Append("hdn.item(0).value = row;").Append("\r\n");
                    sb.Append("grd.rows.item(actualidx).className = '").Append(SelectedRowStyle.CssClass).Append(
                        "';").Append("\r\n");
                    sb.Append("}}</script>");
                    Page.ClientScript.RegisterClientScriptBlock(typeof(string), "SelectMSGridRow", sb.ToString());
                }
            }
        }

        protected override void OnRowCreated(GridViewRowEventArgs e)
        {
            if (AllowRowSelection && e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes.Add("onclick",
                                     string.Format("MSDatagridSelectRow('{0}', {1});", ClientID, e.Row.RowIndex));
            }
        }

        private void GridView_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndex = base.SelectedIndex;
        }

        #endregion Methods
    }
}